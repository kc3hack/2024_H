namespace KoeBook.Core.Models;

public record SoundModel(
    string Id,
    string Name,
    IReadOnlyList<string> Styles);
