using KoeBook.Core.Contracts.Services;
using KoeBook.Core.Helpers;
using KoeBook.Core.Models;

namespace KoeBook.Services.CoreMocks;

public class EpubGenerateServiceMock(IDisplayStateChangeService displayStateChangeService) : IEpubGenerateService
{
    private readonly IDisplayStateChangeService _displayStateChangeService = displayStateChangeService;

    public async ValueTask<string> GenerateEpubAsync(BookScripts bookScripts, string tempDirectory, CancellationToken cancellationToken)
    {
        var stateChanging = _displayStateChangeService.ResetProgress(bookScripts.BookProperties, GenerationState.SoundProducing, 4000);
        await Task.Delay(200, cancellationToken).ConfigureAwait(false);
        stateChanging.UpdateProgress(200);
        await Task.Delay(2000, cancellationToken).ConfigureAwait(false);
        stateChanging.UpdateProgress(2000);
        await Task.Delay(1000, cancellationToken).ConfigureAwait(false);
        stateChanging.UpdateProgress(3000);
        await Task.Delay(200, cancellationToken).ConfigureAwait(false);
        stateChanging.UpdateProgress(4000);

        stateChanging = _displayStateChangeService.ResetProgress(bookScripts.BookProperties, GenerationState.Publishing, 200);
        await Task.Delay(1000, cancellationToken).ConfigureAwait(false);
        stateChanging.UpdateProgress(50);
        await Task.Delay(1000, cancellationToken).ConfigureAwait(false);
        stateChanging.UpdateProgress(120);
        var resultPath = Path.Combine(tempDirectory, "mock.epub");
        await File.WriteAllTextAsync(resultPath, "sample", cancellationToken).ConfigureAwait(false);
        stateChanging.UpdateProgress(200);
        return resultPath;
    }
}
