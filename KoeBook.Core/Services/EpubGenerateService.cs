using KoeBook.Core.Contracts.Services;
using KoeBook.Core.Models;
using KoeBook.Epub;
using KoeBook.Epub.Models;

namespace KoeBook.Core.Services;

public class EpubGenerateService(ISoundGenerationService soundGenerationService, IEpubDocumentStoreService epubDocumentStoreService) : IEpubGenerateService
{
    private ISoundGenerationService _soundGenerationService = soundGenerationService;
    private IEpubDocumentStoreService _documentStoreService = epubDocumentStoreService;

    public async ValueTask<string> GenerateEpubAsync(BookScripts bookScripts, string tempDirectory, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var document = _documentStoreService.Documents.Where(doc => doc.Id == bookScripts.BookProperties.Id).FirstOrDefault()
            ?? throw new InvalidOperationException($"The epub document ({bookScripts.BookProperties.Id}) can't be found.");

        foreach (var scriptLine in bookScripts.ScriptLines)
        {
            scriptLine.Paragraph.Audio = new Audio(await _soundGenerationService.GenerateLineSoundAsync(scriptLine, bookScripts.Options, cancellationToken).ConfigureAwait(false));
        }

        if (await document.TryCreateEpubAsync(tempDirectory, bookScripts.BookProperties.Id.ToString(), cancellationToken).ConfigureAwait(false))
        {
            _documentStoreService.Unregister(bookScripts.BookProperties.Id);
            return Path.Combine(tempDirectory, $"{bookScripts.BookProperties.Id}.epub");
        }
        else
        {
            throw new EbookException(ExceptionType.EpubCreateError);
        }
    }
}
