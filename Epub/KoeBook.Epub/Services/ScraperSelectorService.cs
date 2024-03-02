using System.Collections.Immutable;
using KoeBook.Epub.Contracts.Services;
using KoeBook.Epub.Models;

namespace KoeBook.Epub.Services;

public class ScraperSelectorService(IEnumerable<IScrapingService> scrapingServices) : IScraperSelectorService
{
    private readonly ImmutableArray<IScrapingService> _scrapingServices = scrapingServices.ToImmutableArray();

    public async ValueTask<EpubDocument> ScrapingAsync(string url, string coverFillePath, string tempDirectory, Guid id, CancellationToken ct)
    {
        var uri = new Uri(url);

        foreach (var service in _scrapingServices)
        {
            if (service.IsMatchSite(uri))
                return await service.ScrapingAsync(url, coverFillePath, tempDirectory, id, ct);
        }

        throw new ArgumentException("対応するURLではありません");
    }
}
