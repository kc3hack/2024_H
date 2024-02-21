using System.Globalization;
using System.Text;
using System.Xml;

namespace KoeBook.Epub;

public class EpubDocument(string title, string author)
{
    public string Title { get; set; } = title;
    public string Author { get; set; } = author;

    public string CoverFilePath { get; set; } = "";

    public List<CssClass>? CssStyles { get; set; }
    public List<Chapter> Chapters { get; set; } = [];

    public string CreateNavXhtml()
    {
        var builder = new StringBuilder($"""
            <?xml version="1.0" encoding="UTF-8"?>
            <html xmlns="http://www.w3.org/1999/xhtml" xmlns:epub="http://www.idpf.org/2007/ops">
                <head>
                    <meta charset="UTF-8"/>
                    <title>{Title}</title>
                </head>
                <body>
                    <nav epub:type="toc" id="toc">
                        <ol>

            """);
        if (Chapters?.Count == 1 && Chapters[0].Title == null)
        {
            for (int i = 0; i < Chapters[0].Sections.Count; i++) {
                builder.AppendLine($"""
                                <li>
                                    <a href="{Chapters[0].Sections[i].Id}.xhtml#{Chapters[0].Sections[i].Id}_p0">{Chapters[0].Sections[i].Title}</a>
                                </li>
                """);
            }
        }
        else
        {
            for(int i = 0;i < Chapters?.Count; i++)
            {
                builder.AppendLine($"""
                                    <li>
                                        <span>{Chapters[i].Title}</span>
                                        <ol>
                    """);
                for(int j = 0; j < Chapters[i].Sections.Count; j++)
                {
                    builder.AppendLine($"""
                                                <li>
                                                    <a href="{Chapters[i].Sections[j].Id}.xhtml#{Chapters[i].Sections[j].Id}_p0">{Chapters[i].Sections[j].Title}</a>
                                                </li>
                        """);
                }
                builder.AppendLine($"""
                                        </ol>
                                    </li>
                    """);
            }
        }
        builder.AppendLine($"""
                        </ol>
                    </nav>
                </body>
            </html>
            """);
        return builder.ToString();
    }
}
