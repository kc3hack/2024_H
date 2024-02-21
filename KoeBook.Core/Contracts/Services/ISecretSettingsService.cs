namespace KoeBook.Core.Contracts.Services;

public interface ISecretSettingsService
{
    Task<string?> GetApiKeyAsync(string folderPath, CancellationToken cancellationToken);

    Task SaveApiKeyAsync(string folderPath, string apiKey, CancellationToken cancellationToken);
}
