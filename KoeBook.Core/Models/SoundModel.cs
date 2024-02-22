namespace KoeBook.Core.Models;

public record SoundModel(
    string name,
    IReadOnlyList<string> styles);
