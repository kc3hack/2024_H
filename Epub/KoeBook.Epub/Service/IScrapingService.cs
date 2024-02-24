namespace KoeBook.Epub.Service;

public interface IScrapingService
{
    public Task<EpubDocument> ScrapingAsync(string url, string coverFillePath, string imageDirectory, Guid id, CancellationToken ct);
}
