﻿#pragma warning disable CA1416 // プラットフォームの互換性を検証
using System.Security.Cryptography;
using System.Text;
using KoeBook.Core.Contracts.Services;

namespace KoeBook.Core.Services;

public class SecretSettingsService : ISecretSettingsService
{
    private readonly byte[] _bytes;

    public SecretSettingsService()
    {
        ulong value = 2546729043367843253ul;
        value ^= value << 13;
        value ^= value >> 7;
        value ^= value << 17;
        _bytes = BitConverter.GetBytes(value);
    }

    public Task<string?> GetApiKeyAsync(string folderPath, CancellationToken cancellationToken)
    {
        return Task.Run(async () =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            var data = await File.ReadAllBytesAsync(Path.Combine(folderPath, "alt"), cancellationToken).ConfigureAwait(false);
            if (data is null)
                return null;

            var result = ProtectedData.Unprotect(data, _bytes, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(result);
        }, cancellationToken);
    }

    public Task SaveApiKeyAsync(string folderPath, string apiKey, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(apiKey);
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