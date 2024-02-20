namespace KoeBook.Epub;

public class Section
{
    public string Id { get; }
    public string Title { get; set; } = "";
    public List<Element> Elements { get; set; } = [];

    internal Section(string title, string id)
    {
        Title = title;
        Id = id;
    }
}
