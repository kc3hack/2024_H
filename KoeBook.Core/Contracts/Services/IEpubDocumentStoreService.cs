using KoeBook.Epub;

namespace KoeBook.Core.Contracts.Services;

public interface IEpubDocumentStoreService
{
    IReadOnlyList<EpubDocument> Documents { get; }

    void Register(EpubDocument document, CancellationToken cancellationToken);

    void Unregister(Guid id);
}
