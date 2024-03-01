using KoeBook.Epub.Models;

namespace KoeBook.Epub.Contracts.Services;

public interface IScrapingAozoraService
{
    public ValueTask<EpubDocument> ScrapingAsync(string url, string coverFillePath, string imageDirectory, Guid id, CancellationToken ct);
}
