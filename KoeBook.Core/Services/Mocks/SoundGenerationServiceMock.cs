using KoeBook.Core.Contracts.Services;
using KoeBook.Core.Models;

namespace KoeBook.Core.Services.Mocks;

public class SoundGenerationServiceMock : ISoundGenerationService
{
    public async ValueTask<byte[]> GenerateLineSoundAsync(ScriptLine scriptLine, BookOptions bookOptions, CancellationToken cancellationToken)
    {
        await Task.Delay(1000, cancellationToken).ConfigureAwait(false);
        // 適当なバイト列を返す
        var random = new Random();
        var buffer = new byte[44100 * 2];
        random.NextBytes(buffer);
        return buffer;
    }
}
