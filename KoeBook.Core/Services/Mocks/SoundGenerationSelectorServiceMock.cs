using KoeBook.Core.Contracts.Services;
using KoeBook.Core.Models;

namespace KoeBook.Core.Services.Mocks;

internal class SoundGenerationSelectorServiceMock : ISoundGenerationSelectorService
{
    public IReadOnlyList<SoundModel> Models { get; private set; } = [];

    public async ValueTask InitializeAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(1000, cancellationToken).ConfigureAwait(false);
        Models = [
            new SoundModel("青年1", ["neutral", "laughing", "happy", "sad", "cry", "surprised", "angry"]),
            new SoundModel("青年2", ["neutral", "laughing", "happy", "sad", "cry", "surprised", "angry"]),
            new SoundModel("女性1", ["neutral", "laughing", "happy", "sad", "cry", "surprised", "angry"]),
            new SoundModel("女性2", ["neutral", "laughing", "happy", "sad", "cry", "surprised", "angry"]),
            new SoundModel("ナレーション (男性)", ["narration"]),
            new SoundModel("ナレーション (女性)", ["narration"]),
        ];
    }
}
