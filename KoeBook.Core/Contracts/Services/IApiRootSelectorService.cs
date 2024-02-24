namespace KoeBook.Core.Contracts.Services;

public interface IApiRootSelectorService
{
    string StyleBertVitsRoot { get; }

    ValueTask InitializeAsync(CancellationToken cancellationToken);

    ValueTask SetStyleBertVitsRoot(string root);
}
