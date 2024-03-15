namespace KoeBook.Epub.Contracts.Services;

public interface IScrapingClientService
{
    /// <summary>
    /// スクレイピングでGETする用
    /// APIは不要
    /// </summary>
    ValueTask<HttpResponseMessage> GetAsync(string url, CancellationToken ct);
}
