namespace KoeBook.Core.Models;

/// <summary>
/// 読み上げる本の情報
/// </summary>
public class BookProperties(Guid id, string source, SourceType sourceType)
{
    public Guid Id { get; } = id;

    public string Source { get; } = source;

    public SourceType SourceType { get; } = sourceType;
}
