namespace KoeBook.Epub.Service;

public interface IScrapingService
{
    public Task<EpubDocument> ScrapingAsync(string url, string coverFil9lePath);
}
