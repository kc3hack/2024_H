using KoeBook.Epub.Contracts.Services;
using KoeBook.Epub.Models;

namespace KoeBook.Epub.Services;

public class ScraperSelectorService(IScrapingAozoraService scrapingAozoraService, IScrapingNaroService scrapingNaroService) : IScraperSelectorService
{
    public ValueTask<EpubDocument> ScrapingAsync(string url, string coverFillePath, string tempDirectory, Guid id, CancellationToken ct)
    {
        var uri = new Uri(url);

        return uri.Host switch
        {
            "www.aozora.gr.jp" => scrapingAozoraService.ScrapingAsync(url, coverFillePath, tempDirectory, id, ct),
            "ncode.syosetu.com" => scrapingNaroService.ScrapingAsync(url, coverFillePath, tempDirectory, id, ct),
            _ => throw new ArgumentException("有効なドメインではありません。"),
        };
    }
}
