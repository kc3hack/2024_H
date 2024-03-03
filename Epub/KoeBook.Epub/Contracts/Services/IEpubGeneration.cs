using KoeBook.Epub.Models;

namespace KoeBook.Epub.Contracts.Services;

public interface IEpubGeneration
{
    ValueTask<bool> TryCreateEpubAsync(EpubDocument epubDocument, string tmpDirectory, CancellationToken ct);
}
