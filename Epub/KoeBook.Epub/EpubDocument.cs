using System.Globalization;
using System.Xml;

namespace KoeBook.Epub;

public class EpubDocument(string title, string author)
{
    string Title { get; set; } = title;
    string Author { get; set; } = author;

    string CoverFilePath { get; set; } = "";

    List<CssStyle>? CssStyles { get; set; }
    List<Chapter>? Chapters { get; set; }
}
