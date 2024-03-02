using KoeBook.Epub.Models;

namespace KoeBook.Epub.Contracts.Services;

/// <summary>
/// スクレイピングを行い、EpubDocumentを作成します。
/// </summary>
public interface IScraperSelectorService
{
    public ValueTask<EpubDocument> ScrapingAsync(string url, string coverFillePath, string tempDirectory, Guid id, CancellationToken ct);
}
