using System.Text;
using System.Text.RegularExpressions;
using KoeBook.Core.Contracts.Services;
using KoeBook.Core.Models;
using KoeBook.Epub;
using KoeBook.Epub.Service;

namespace KoeBook.Core.Services;

public partial class AnalyzerService(IScrapingService scrapingService, IEpubDocumentStoreService epubDocumentStoreService, ILlmAnalyzerService llmAnalyzerService) : IAnalyzerService
{
    private readonly IScrapingService _scrapingService = scrapingService;
    private readonly IEpubDocumentStoreService _epubDocumentStoreService = epubDocumentStoreService;
    private readonly ILlmAnalyzerService _llmAnalyzerService = llmAnalyzerService;
    private Dictionary<string, string> _rubyReplacements = new Dictionary<string, string>();

    public async ValueTask<BookScripts> AnalyzeAsync(BookProperties bookProperties, string tempDirectory, string coverFilePath, CancellationToken cancellationToken)
    {
        var document = await _scrapingService.ScrapingAsync(bookProperties.Source, coverFilePath, tempDirectory, bookProperties.Id, cancellationToken);
        _epubDocumentStoreService.Register(document, cancellationToken);

        var scriptLines = new List<ScriptLine>();
        foreach (var chapter in document.Chapters)
        {
            foreach (var section in chapter.Sections)
            {
                foreach (var element in section.Elements)
                {
                    if (element is Paragraph paragraph)
                    {
                        var line = paragraph.Text;
                        // rubyタグがあればルビのdictionaryに登録
                        var matches = MyRegex().Matches(input: line);
                        foreach (Match match in matches)
                        {
                            var key = match.Groups[1].Value;
                            var value = match.Groups[2].Value;
                            if (!_rubyReplacements.ContainsKey(key))
                            {
                                _rubyReplacements.Add(key, value);
                            }
                        }
                        // ルビを置換
                        foreach (var replacement in _rubyReplacements)
                        {
                            line = line.Replace(replacement.Key, replacement.Value);
                        }
                        scriptLines.Add(new ScriptLine(paragraph, line, "", ""));
                    }
                }
            }
        }

        // 800文字以上になったら１チャンクに分ける
        var chunks = new List<string>();
        var chunk = new StringBuilder();
        foreach (var line in scriptLines)
        {
            if (chunk.Length + line.Text.Length > 800)
            {
                chunks.Add(chunk.ToString());
                chunk.Clear();
            }
            chunk.Append(line.Text);
        }
        if (chunk.Length > 0) chunks.Add(chunk.ToString());

        // GPT4による話者、スタイル解析
        var bookScripts = await _llmAnalyzerService.LlmAnalyzeScriptLinesAsync(bookProperties, scriptLines, chunks, cancellationToken);

        return bookScripts;
    }

    [GeneratedRegex("<ruby>(.*?)<rt>(.*?)</rt></ruby>")]
    private static partial Regex MyRegex();
}
