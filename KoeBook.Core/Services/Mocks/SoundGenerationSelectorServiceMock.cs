using KoeBook.Core.Contracts.Services;
using KoeBook.Core.Models;

namespace KoeBook.Core.Services.Mocks;

public class SoundGenerationSelectorServiceMock : ISoundGenerationSelectorService
{
    public IReadOnlyList<SoundModel> Models { get; private set; } = [];

    public async ValueTask InitializeAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(1000, cancellationToken).ConfigureAwait(false);
        Models = [
            new SoundModel("0", "青年1", ["neutral", "laughing", "happy", "sad", "cry", "surprised", "angry"]),
            new SoundModel("1", "青年2", ["neutral", "laughing", "happy", "sad", "cry", "surprised", "angry"]),
            new SoundModel("2", "女性1", ["neutral", "laughing", "happy", "sad", "cry", "surprised", "angry"]),
            new SoundModel("3", "女性2", ["neutral", "laughing", "happy", "sad", "cry", "surprised", "angry"]),
            new SoundModel("4", "ナレーション (男性)", ["narration"]),
            new SoundModel("5", "ナレーション (女性)", ["narration"]),
        ];
    }
}
