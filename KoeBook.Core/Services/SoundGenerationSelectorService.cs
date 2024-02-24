using System.Net.Http.Json;
using KoeBook.Core.Contracts.Services;
using KoeBook.Core.Models;
using KoeBook.Core.Models.StyleBertVits;

namespace KoeBook.Core.Services;

public class SoundGenerationSelectorService(HttpClient httpClient) : ISoundGenerationSelectorService
{
    private readonly HttpClient _httpClient = httpClient;

    public IReadOnlyList<SoundModel> Models
    {
        get
        {
            if (_models is null)
                EbookException.Throw(ExceptionType.DoesNotInitialized);
            return _models;
        }
    }
    private IReadOnlyList<SoundModel>? _models;

    public async ValueTask InitializeAsync(CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync("http://127.0.0.1:5000/models/info", cancellationToken).ConfigureAwait(false)!;
            if (!response.IsSuccessStatusCode)
                throw new EbookException(ExceptionType.InitializeFailed);
            var models = await response.Content.ReadFromJsonAsync<Dictionary<string, ModelInfo>>(cancellationToken)
                .ConfigureAwait(false) ?? throw new EbookException(ExceptionType.InitializeFailed);
            _models = models.Select(kvp => new SoundModel(kvp.Key, kvp.Value.FirstSpk, kvp.Value.Styles)).ToArray();
        }
        catch (OperationCanceledException) { throw; }
        catch (EbookException) { throw; }
        catch (Exception e)
        {
            throw new EbookException(ExceptionType.InitializeFailed, innerException: e);
        }
    }
}
