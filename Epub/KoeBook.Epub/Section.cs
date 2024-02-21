namespace KoeBook.Epub;

public sealed class Section
{
    public string Id { get; }
    public string Title { get; set; }
    public List<Element> Elements { get; set; } = [];

    public Section(string title)
    {
        Title = title;
        Id = Guid.NewGuid().ToString();
    }
}
