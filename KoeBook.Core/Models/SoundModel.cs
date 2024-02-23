namespace KoeBook.Core.Models;

public record SoundModel(
    string Name,
    IReadOnlyList<string> Styles);
