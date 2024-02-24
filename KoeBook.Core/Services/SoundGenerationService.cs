using System.Web;
using KoeBook.Core.Contracts.Services;
using KoeBook.Core.Models;

namespace KoeBook.Core.Services;

public class SoundGenerationService(
    HttpClient htppClient,
    ISoundGenerationSelectorService soundGenerationSelectorService) : ISoundGenerationService
{
    private readonly HttpClient _httpClient = htppClient;
    private readonly ISoundGenerationSelectorService _soundGenerationSelectorService = soundGenerationSelectorService;

    public async ValueTask<byte[]> GenerateLineSoundAsync(ScriptLine scriptLine, BookOptions bookOptions, CancellationToken cancellationToken)
    {
        var model = bookOptions.CharacterMapping[scriptLine.Character];
        var soundModel = _soundGenerationSelectorService.Models.FirstOrDefault(m => m.Name == model)
            ?? throw new EbookException(ExceptionType.SoundGenerationFailed);
        var style = soundModel.Styles.Contains(scriptLine.Style) ? scriptLine.Style : soundModel.Styles.First();
        var queryCollection = HttpUtility.ParseQueryString(string.Empty);
        queryCollection.Add("text", scriptLine.Text);
        queryCollection.Add("model_id", soundModel.Id);
        queryCollection.Add("style", scriptLine.Style);
        try
        {
            var response = await _httpClient.GetAsync($"http://127.0.0.1:5000/voice?${queryCollection}", cancellationToken)
                .ConfigureAwait(false) ?? throw new EbookException(ExceptionType.SoundGenerationFailed);

            if (!response.IsSuccessStatusCode)
                throw new EbookException(ExceptionType.SoundGenerationFailed);
            var audio = await response.Content.ReadAsByteArrayAsync(cancellationToken)
                .ConfigureAwait(false) ?? throw new EbookException(ExceptionType.SoundGenerationFailed);
            return audio;
        }
        catch (OperationCanceledException) { throw; }
        catch (EbookException) { throw; }
        catch (Exception e)
        {
            throw new EbookException(ExceptionType.SoundGenerationFailed, innerException: e);
        }
    }
}
