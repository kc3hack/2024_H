using System.Net.Http.Json;
using KoeBook.Core.Contracts.Services;
using KoeBook.Core.Models;
using KoeBook.Core.Models.StyleBertVits;

namespace KoeBook.Core.Services;

public class SoundGenerationSelectorService(IStyleBertVitsClientService styleBertVitsClientService) : ISoundGenerationSelectorService
{
    private readonly IStyleBertVitsClientService _styleBertVitsClientService = styleBertVitsClientService;

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
        var models = await _styleBertVitsClientService
            .GetFromJsonAsync<Dictionary<string, ModelInfo>>("/models/info", ExceptionType.InitializeFailed, cancellationToken)
            .ConfigureAwait(false);

        _models = models.Select(kvp => new SoundModel(kvp.Key, kvp.Value.FirstSpk, kvp.Value.Styles)).ToArray();
    }
}
