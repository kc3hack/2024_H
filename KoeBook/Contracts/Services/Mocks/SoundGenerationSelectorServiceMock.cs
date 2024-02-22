using KoeBook.Core.Contracts.Services;

namespace KoeBook.Contracts.Services.Mocks;

internal class SoundGenerationSelectorServiceMock : ISoundGenerationSelectorService
{
    public IReadOnlyList<SoundModel> Models { get; private set; } = Array.Empty<SoundModel>();

    public async ValueTask InitializeAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(1000, cancellationToken);
        Models = [
            new SoundModel("メロス",["narration", "neutral", "laughing", "happy", "sad", "cry", "surprised", "angry" ]),
            new SoundModel("老爺",["narration", "neutral", "laughing", "happy", "sad", "cry", "surprised", "angry" ]),
            new SoundModel("王",["narration", "neutral", "laughing", "happy", "sad", "cry", "surprised", "angry" ]),
        ];
    }
}
