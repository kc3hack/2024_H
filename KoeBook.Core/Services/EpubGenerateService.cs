using KoeBook.Core.Contracts.Services;
using KoeBook.Core.Models;
using KoeBook.Epub;

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

        for (int i = 0; i < document.Chapters.Count; i++)
        {
            for (int j = 0; j < document.Chapters[i].Sections.Count; j++)
            {
                for (int k = 0; k < document.Chapters[i].Sections[j].Elements.Count; k++)
                {
                    if (document.Chapters[i].Sections[j].Elements[k] is Paragraph para)
                    {
                        var scriptLine = bookScripts.ScriptLines.Where(sl => sl.Id == $"{document.Chapters[i].Sections[j].Id}_p{k}").FirstOrDefault();
                        if (scriptLine != null)
                        {
                            para.Audio = new Audio(await _soundGenerationService.GenerateLineSoundAsync(scriptLine, bookScripts.Options, cancellationToken));
                        }
                    }
                }
            }
        }

        if (await document.TryCreateEpubAsync(tempDirectory, bookScripts.BookProperties.Id.ToString(), cancellationToken))
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
