using KoeBook.Core.Models;

namespace KoeBook.Contracts.Services.Mocks;

internal class SoundGenerationMock
{
    public async ValueTask<byte[]> GenerateLineSoundAsync(ScriptLine scriptLine, BookOptions bookOptions, CancellationToken cancellationToken)
    {
        await Task.Delay(1000, cancellationToken);
        // 適当なバイト列を返す
        var random = new Random();
        var buffer = new byte[44100 * 2];
        random.NextBytes(buffer);
        return buffer;
    }
}
