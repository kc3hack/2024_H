using KoeBook.Core.Contracts.Services;
using KoeBook.Core.Models;
using KoeBook.Epub.Service;

namespace KoeBook.Core.Services;

public class AnalyzerService(IScrapingService scrapingService, IEpubDocumentStoreService epubDocumentStoreService) : IAnalyzerService
{
    private readonly IScrapingService _scrapingService = scrapingService;
    private readonly IEpubDocumentStoreService _epubDocumentStoreService = epubDocumentStoreService;

    public async ValueTask<BookScripts> AnalyzeAsync(BookProperties bookProperties, string tempDirectory, string coverFilePath, CancellationToken cancellationToken)
    {
        var document = await _scrapingService.ScrapingAsync(bookProperties.Source, coverFilePath, bookProperties.Id, cancellationToken);
        _epubDocumentStoreService.Register(document, cancellationToken);

        throw new NotImplementedException();
    }
}
