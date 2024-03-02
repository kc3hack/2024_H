using KoeBook.Epub.Models;

namespace KoeBook.Epub.Contracts.Services;

public interface IScrapingService : IScraperSelectorService
{
    public bool IsMatchSite(Uri url);
}
