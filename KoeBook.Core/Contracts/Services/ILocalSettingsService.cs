namespace KoeBook.Core.Contracts.Services;

public interface ILocalSettingsService
{
    Task<T?> ReadSettingAsync<T>(string key);

    Task SaveSettingAsync<T>(string key, T value);

    ValueTask<string?> GetApiKeyAsync(CancellationToken cancellationToken);

    ValueTask SaveApiKeyAsync(string apiKey, CancellationToken cancellationToken);
}
