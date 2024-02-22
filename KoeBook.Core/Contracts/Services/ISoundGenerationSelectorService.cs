namespace KoeBook.Core.Contracts.Services;

public interface ISoundGenerationSelectorService
{
    public SoundModel[] Models { get; }

    public ValueTask InitializeAsync(CancellationToken cancellationToken);
}

public record SoundModel(string name, string[] styles);
