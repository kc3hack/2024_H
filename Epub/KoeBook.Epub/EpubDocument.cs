using System.Globalization;
using System.Xml;

namespace KoeBook.Epub;

public class EpubDocument(string title, string author)
{
    public string Title { get; set; } = title;
    public string Author { get; set; } = author;

    public string CoverFilePath { get; set; } = "";

    public List<CssClass>? CssStyles { get; set; }
    public List<Chapter>? Chapters { get; set; }
}
