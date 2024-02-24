using System.Text;
using System.Text.RegularExpressions;
using KoeBook.Core.Contracts.Services;
using KoeBook.Core.Models;
using OpenAI.Interfaces;
using OpenAI.ObjectModels.RequestModels;

namespace KoeBook.Core.Services;

public partial class ChaGptAnalyzerService(IOpenAIService openAIService) : ILlmAnalyzerService
{
    private readonly IOpenAIService _openAiService = openAIService;

    public async ValueTask<BookScripts> LlmAnalyzeScriptLinesAsync(BookProperties bookProperties, List<ScriptLine> scriptLines, List<string> chunks, CancellationToken cancellationToken)
    {
        Queue<string> summaryList = new();
        Queue<string> characterList = new();
        for (int i = 0; i < chunks.Count; i++)
        {
            // 話者・スタイル解析
            var Task1 = CharacterStyleAnalysisAsync(scriptLines, chunks, summaryList.Peek(), characterList.Peek(), i, cancellationToken);
            // 要約・キャラクターリスト解析
            var Task2 = SummaryCharacterListAnalysisAsync(scriptLines, chunks, summaryList.Peek(), characterList.Peek(), i, cancellationToken);
            // WhenAllで非同期処理を待つ
            await Task.WhenAll(Task1, Task2);
            // 結果をキューに追加
            summaryList.Enqueue(Task2.Result.summary);
            characterList.Enqueue(Task2.Result.characterList);
            // 5個以上になったら古いものを削除
            if (summaryList.Count > 5)
            {
                summaryList.Dequeue();
                characterList.Dequeue();
            }
        }

        // キャラクター名と声のマッピング
        var characterVoiceMapping = await GetCharacterVoiceMappingAsync(scriptLines, characterList.Peek(), cancellationToken);

        var bookScripts = new BookScripts
        (
            bookProperties,
            new BookOptions
            {
                CharacterMapping = characterVoiceMapping
            }
        )
        {
            ScriptLines = scriptLines
        };
        return bookScripts;
    }

    private async Task CharacterStyleAnalysisAsync(List<ScriptLine> scriptLines,
                                                   List<string> chunks,
                                                   string summary,
                                                   string characterList,
                                                   int idx,
                                                   CancellationToken cancellationToken)
    {
        var completionResult = await _openAiService.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest
        {
            Messages = new List<ChatMessage>
            {
                ChatMessage.FromSystem($$"""
                    All Information
                    - Goal
                    - Information about this story
                    - Output Format
                    - Notes


                    # Goal
                    Set speaker name and style of target sentence


                    # Information about this story
                    1. Summary so far
                    {{summary}}


                    2. Character List
                    {{characterList}}


                    3. Target Sentence
                    {{chunks[idx]}}


                    # Output Format
                    ```
                    #### CharacterList
                    - name1
                    - name2
                    ...


                    #### Summery
                    - point1
                    - point2
                    - point3


                    #### Talker and Style Setting
                    {SENTENCE}[{TALKER}:{STYLE}]
                    {SENTENCE}[{TALKER}:{STYLE}]
                    ...
                    {SENTENCE}[{TALKER}:{STYLE}]
                    ```
                    STYLE = narration | neutral | laughing | happy | sad | cry | surprised | angry
                    assign the name of the speaker to {TALKER} if the sentence is dialogue, or use 'ナレーション' for narrative sentences. If the sentence is part of a monologue, use the name of the character who is speaking.
                    The {SENTENCE} should be a direct excerpt from the 'Target Sentence' section."


                    # **Notes**
                    ## **Separate Narration and lines.** Be careful as you often make mistakes!!
                    Input
                    ```
                    落ちてくる落ち葉を見て、清美は言った。
                    「悲しいね…。あなたとの思い出が、一枚一枚、地面に落ちていくみたい…。」
                    ```
                    Output
                    ```
                    落ちてくる落ち葉を見て、清美は言った。[ナレーション:narration]
                    「悲しいね…。あなたとの思い出が、一枚一枚、地面に落ちていくみたい…。」[漆原清美:sad]
                    ```
                    ## First person
                    Input
                    ```
                    俺は泣きながら勇気を振り絞って言った。
                    「お前は本当に、俺のことを愛してるのか？」
                    ```
                    Output
                    ```
                    俺は泣きながら勇気を振り絞って言った。[松原一馬:narration]
                    「お前は本当に、俺のことを愛してるのか？」[松原一馬:cry]
                    ```
                    ## What is not enclosed in brackets is basically narration style
                    Input
                    ```
                    ファーラン王子はそっと、王のもとへ行き、王の耳元で何かを囁いた。
                    そして、王は、その言葉を聞いて、驚いたような顔をした。
                    アミダス王は立ち上がり、民衆の前で声を張り上げた。
                    「私は、この国を、息子ファーランに譲る！」
                    ```
                    Output
                    ```
                    ファーラン王子はそっと、王のもとへ行き、王の耳元で何かを囁いた。[ナレーション:narration]
                    そして、王は、その言葉を聞いて、驚いたような顔をした。[ナレーション:narration]
                    アミダス王は立ち上がり、民衆の前で声を張り上げた。[ナレーション:narration]
                    「私は、この国を、息子ファーランに譲る！」[アミダス王:neutral]
                    ```
                    """
                    )
            },
            Model = OpenAI.ObjectModels.Models.Gpt_4_0125_preview,
            MaxTokens = 2000
        });
        if (completionResult.Successful)
        {
            var result = completionResult.Choices.First().Message.Content;
            // "#### Talker and Style Setting"以下の文章を改行区切りでリスト化
            List<string> output = new List<string>();
            var lines = result?.Split("\n");
            var start = false;
            for (var i = 0; i < lines?.Length; i++)
            {
                var line = lines[i];
                if (start)
                {
                    if (line.Contains("```"))
                    {
                        break;
                    }
                    output.Add(line);
                }
                if (line.Contains("#### Talker and Style Setting"))
                {
                    start = true;
                }
            }
            // パラグラフと対応するoutputをマッピング
            for (int i = 0; i < output.Count; i++)
            {
                // {SENTENCE}[{TALKER}:{STYLE}]の形式になっているかチェック
                var match = Regex.Match(output[i], @"(.+)\[(.+):(.+)]");
                if (match.Success)
                {
                    var sentence = match.Groups[1].Value;
                    var talker = match.Groups[2].Value;
                    var style = match.Groups[3].Value;
                    if (sentence == scriptLines[idx + i].Text || Math.Abs(sentence.Length - scriptLines[idx + i].Text.Length) < 5)
                    {
                        scriptLines[idx + i].Character = talker;
                        scriptLines[idx + i].Style = style;
                    }
                    else
                    {
                        EbookException.Throw(ExceptionType.Gpt4TalkerAndStyleSettingFailed);
                    }
                }
                else
                {
                    EbookException.Throw(ExceptionType.Gpt4TalkerAndStyleSettingFailed);
                }
            }
        }
    }

    private async Task<(string summary, string characterList)> SummaryCharacterListAnalysisAsync(List<ScriptLine> scriptLines,
                                                         List<string> chunks,
                                                         string summary,
                                                         string characterList,
                                                         int idx,
                                                         CancellationToken cancellationToken)
    {
        var storyText = string.Join("\n", chunks.Skip(int.Max(0, idx - 4)).Take(4));
        var completionResult = await _openAiService.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest
        {
            Messages = new List<ChatMessage>
            {
                ChatMessage.FromSystem($$"""
                    All Information
                    - Goal
                    - Information about this story
                    - Output Format


                    # Goal
                    Make CharacterList and Summery of the story.


                    # Information about this story
                    1. Character List
                    {{characterList}}


                    2. Summary so far
                    {{summary}}


                    3. Story
                    {{storyText}}


                    # Output Format (write name,description,summary in Japanese!!)
                    ```
                    #### CharacterList
                    {name1}
                    - {description1}
                    - {description2}
                    ...(up to 10 descriptions)


                    {name2}
                    - {description1}
                    - {description2}
                    ...
                    ...


                    #### Summery of 5 points
                    - {summary1}
                    - {summary2}
                    ...
                    ```
                    """),
            },
            Model = OpenAI.ObjectModels.Models.Gpt_4_0125_preview,
            MaxTokens = 2000
        });
        if (completionResult.Successful)
        {
            var result = completionResult.Choices.First().Message.Content;
            var lines = result?.Split("\n");
            bool summaryStart = false;
            bool characterListStart = false;
            StringBuilder _summary = new();
            StringBuilder _characterList = new();

            for (var i = 0; i < lines?.Length; i++)
            {
                var line = lines[i];
                if (line.Contains("#### CharacterList"))
                {
                    characterListStart = true;
                    continue;
                }
                if (line.Contains("#### Summery of"))
                {
                    summaryStart = true;
                    characterListStart = false;
                    continue;
                }
                if (characterListStart)
                {
                    _characterList.AppendLine(line);
                }
                if (summaryStart)
                {
                    if (line.Contains("```"))
                    {
                        break;
                    }
                    _summary.AppendLine(line);
                }
            }

            return (_summary.ToString(), _characterList.ToString());
        }
        else
        {
            EbookException.Throw(ExceptionType.Gpt4TalkerAndStyleSettingFailed);
            return default!;
        }
    }

    private async Task<Dictionary<string, string>> GetCharacterVoiceMappingAsync(List<ScriptLine> scriptLines, string characterDescription, CancellationToken cancellationToken)
    {
        // キャラクター名一覧の取得
        var characterList = scriptLines.Select(x => "- " + x.Character).Distinct().ToList();
        var characterListString = string.Join("\n", characterList);
        var completionResult = await _openAiService.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest
        {
            Messages = new List<ChatMessage>
            {
                ChatMessage.FromSystem($$"""
                All Information
                    - Goal
                    - Character Description
                    - Character List
                    - Voice List
                    - Output Format

                #### Goal
                Make a table of character names and voices

                #### Character Description
                {{characterDescription}}

                #### Character List
                {{characterListString}}

                #### Voice List
                - ナレーション女
                - 女子小学生
                - 女子中高生
                - 成人女性
                - 老年女性
                - ナレーション男
                - 男子小学生
                - 男子中高生
                - 成人男性
                - 老年男性

                #### Output Format
                ```
                {character1} - {voice1}
                {character2} - {voice2}
                ...
                ```
                """)
            },
            Model = OpenAI.ObjectModels.Models.Gpt_4_0125_preview,
            MaxTokens = 2000
        });
        if (completionResult.Successful)
        {
            var result = completionResult.Choices.First().Message.Content;
            var lines = result?.Split("\n");
            Dictionary<string, string> characterVoiceMapping = new();
            foreach (var match in from line in lines
                                  let match = CharacterMappingRegex().Match(line)
                                  select match)
            {
                if (match.Success)
                {
                    characterVoiceMapping.Add(match.Groups[1].Value, match.Groups[2].Value);
                }
                else
                {
                    EbookException.Throw(ExceptionType.Gpt4TalkerAndStyleSettingFailed);
                }
            }
            return characterVoiceMapping;
        }
        else
        {
            EbookException.Throw(ExceptionType.Gpt4TalkerAndStyleSettingFailed);
            return default!;
        }
    }

    [GeneratedRegex(@"(.+) - (.+)")]
    private static partial Regex CharacterMappingRegex();
}
