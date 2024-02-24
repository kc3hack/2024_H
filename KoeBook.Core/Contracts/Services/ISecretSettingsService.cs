namespace KoeBook.Core.Contracts.Services;

public interface ISecretSettingsService
{
    string? ApiKey { get; }

    Task<string?> InitializeAsync(string folderPath, CancellationToken cancellationToken);

    Task SaveApiKeyAsync(string folderPath, string apiKey, CancellationToken cancellationToken);
}
