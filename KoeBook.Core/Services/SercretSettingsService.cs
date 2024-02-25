#pragma warning disable CA1416 // プラットフォームの互換性を検証
using System.Security.Cryptography;
using System.Text;
using KoeBook.Core.Contracts.Services;

namespace KoeBook.Core.Services;

public class SecretSettingsService : ISecretSettingsService
{
    private readonly byte[] _bytes;

    public string? ApiKey { get; private set; }

    public SecretSettingsService()
    {
        ulong value = 2546729043367843253ul;
        value ^= value << 13;
        value ^= value >> 7;
        value ^= value << 17;
        _bytes = BitConverter.GetBytes(value);
    }

    public async Task<string?> InitializeAsync(string folderPath, CancellationToken cancellationToken)
    {
        ApiKey = await Task.Run(async () =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            var path = Path.Combine(folderPath, "alt");
            if (!File.Exists(path))
                return null;
            var data = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
            if (data is null)
                return null;

            var result = ProtectedData.Unprotect(data, _bytes, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(result);
        }, cancellationToken).ConfigureAwait(false);
        return ApiKey;
    }

    public Task SaveApiKeyAsync(string folderPath, string apiKey, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(apiKey);
        ApiKey = apiKey;
        return Task.Run(async () =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            var data = Encoding.UTF8.GetBytes(apiKey);

            var result = ProtectedData.Protect(data, _bytes, DataProtectionScope.CurrentUser);
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
            await File.WriteAllBytesAsync(Path.Combine(folderPath, "alt"), result, cancellationToken).ConfigureAwait(false);
        }, cancellationToken);
    }
}
