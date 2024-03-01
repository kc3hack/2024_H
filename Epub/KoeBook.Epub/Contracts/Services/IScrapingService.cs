using KoeBook.Epub.Models;

namespace KoeBook.Epub.Contracts.Services;

public interface IScrapingService
{
    public Task<EpubDocument> ScrapingAsync(string url, string coverFillePath, string imageDirectory, Guid id, CancellationToken ct);
}
