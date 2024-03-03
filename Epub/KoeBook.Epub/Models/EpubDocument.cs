using System.Globalization;
using System.IO.Compression;
using System.Text;

namespace KoeBook.Epub.Models;

public class EpubDocument(string title, string author, string coverFilePath, Guid id)
{
    public string Title { get; set; } = title;
    public string Author { get; set; } = author;

    public string CoverFilePath { get; set; } = coverFilePath;

    public Guid Id { get; } = id;

    public List<CssClass> CssClasses { get; set; } = [
            new CssClass("-epub-media-overlay-active", """
                .-epub-media-overlay-active *{
                    background-color: yellow;
                    color: black !important;
                }
                """),
        new CssClass("-epub-media-overlay-unactive", """
                .-epub-media-overlay-unactive * {
                    color: gray;
                }
                """),
    ];
    public List<Chapter> Chapters { get; set; } = [];

    internal void EnsureChapter()
    {
        if (Chapters.Count == 0)
            Chapters.Add(new Chapter() { Title = null });
    }

    internal void EnsureSection(int chapterIndex)
    {
        EnsureChapter();

        if (Chapters[chapterIndex].Sections.Count == 0)
        {
            if (Chapters[chapterIndex].Title != null)
                Chapters[chapterIndex].Sections.Add(new Section(Chapters[chapterIndex].Title!));
            else
                Chapters[chapterIndex].Sections.Add(new Section(Title));
        }
    }

    internal void EnsureParagraph(int chapterIndex, int sectionIndex)
    {
        EnsureSection(chapterIndex);

        if (Chapters[chapterIndex].Sections[sectionIndex].Elements.Count == 0)
            Chapters[chapterIndex].Sections[sectionIndex].Elements.Add(new Paragraph());
    }

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
        if (Chapters.Count == 1 && Chapters[0].Title == null)
        {
            for (var i = 0; i < Chapters[0].Sections.Count; i++)
            {
                builder.AppendLine($"""
                                <li>
                                    <a href="{Chapters[0].Sections[i].Id}.xhtml#s_{Chapters[0].Sections[i].Id}_p0">{Chapters[0].Sections[i].Title}</a>
                                </li>
                """);
            }
        }
        else
        {
            for (var i = 0; i < Chapters.Count; i++)
            {
                builder.AppendLine($"""
                                    <li>
                                        <span>{Chapters[i].Title}</span>
                                        <ol>
                    """);
                for (var j = 0; j < Chapters[i].Sections.Count; j++)
                {
                    builder.AppendLine($"""
                                                <li>
                                                    <a href="{Chapters[i].Sections[j].Id}.xhtml#s_{Chapters[i].Sections[j].Id}_p0">{Chapters[i].Sections[j].Title}</a>
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

    public string CreateCssText()
    {
        var builder = new StringBuilder();
        foreach (var cssClass in CssClasses)
        {
            builder.AppendLine(cssClass.Text);
        }
        return builder.ToString();
    }

    public string CreateOpf()
    {
        var builder = new StringBuilder($"""
            <package unique-identifier="pub-id" version="3.0" xmlns="http://www.idpf.org/2007/opf">
                <metadata xmlns:dc="http://purl.org/dc/elements/1.1/">
                    <dc:title  id="title">{Title}</dc:title>
                    <dc:creator id="creator">{Author}</dc:creator>
                    <meta refines="#creator" property="role" scheme="marc:relators">aut</meta>
                    <dc:identifier id="pub-id">urn:uuid:{Guid.NewGuid()}</dc:identifier>
                    <dc:language>ja</dc:language>
                    <dc:date>{DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture)}</dc:date>
                    <meta property="dcterms:modified">{DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture)}</meta>
                    <meta property="media:active-class">-epub-media-overlay-active</meta>
                    <meta property="media:playback-active-class">-epub-media-overlay-unactive</meta>

            """);

        var totalTime = TimeSpan.Zero;
        for (var i = 0; i < Chapters.Count; i++)
        {
            for (var j = 0; j < Chapters[i].Sections.Count; j++)
            {
                var time = Chapters[i].Sections[j].GetTotalTime();
                totalTime += time;
                builder.AppendLine($"""
                                <meta property="media:duration" refines="#smil_{i}_{j}">{time}</meta>
                        """);
            }
        }
        builder.AppendLine($"""
                    <meta property="media:duration">{totalTime}</meta>
                </metadata>
                <manifest>
                    <item id="css" media-type="text/css" href="style.css"/>
                    <item id="cover" href="{Path.GetFileName(CoverFilePath)}" properties="cover-image" media-type="{EpubCreateHelper.GetImagesMediaType(CoverFilePath)}" />
                    <item id="nav" href="nav.xhtml" properties="nav" media-type="application/xhtml+xml" />
            """);

        for (var i = 0; i < Chapters.Count; i++)
        {
            for (var j = 0; j < Chapters[i].Sections.Count; j++)
            {
                builder.AppendLine($"""
                                <item id="section_{i}_{j}" href="{Chapters[i].Sections[j].Id}.xhtml" media-type="application/xhtml+xml" media-overlay="smil_{i}_{j}" />
                                <item id="smil_{i}_{j}" href="{Chapters[i].Sections[j].Id}_audio.smil" media-type="application/smil+xml" />
                        """);
                for (var k = 0; k < Chapters[i].Sections[j].Elements.Count; k++)
                {
                    var element = Chapters[i].Sections[j].Elements[k];
                    if (element is Paragraph para && para.Audio != null)
                    {
                        builder.AppendLine(@$"        <item id=""audio_{i}_{j}_{k}"" href=""{Chapters[i].Sections[j].Id}_p{k}.mp3"" media-type=""audio/mpeg"" />");
                    }
                    else if (element is Picture pic && File.Exists(pic.PictureFilePath))
                    {
                        builder.AppendLine(@$"        <item id=""img_{i}_{j}_{k}"" href=""{Chapters[i].Sections[j].Id}_p{k}{Path.GetExtension(pic.PictureFilePath)}"" media-type=""{EpubCreateHelper.GetImagesMediaType(pic.PictureFilePath)}"" />");
                    }
                }
            }
        }

        builder.AppendLine($"""
                </manifest>
                <spine page-progression-direction="ltr">
            """);

        for (var i = 0; i < Chapters.Count; i++)
        {
            for (var j = 0; j < Chapters[i].Sections.Count; j++)
            {
                builder.AppendLine($"""
                                <itemref idref="section_{i}_{j}" id="itemref_{i}_{j}" />
                        """);
            }
        }

        builder.AppendLine($"""
                </spine>
            </package>
            """);
        return builder.ToString();
    }

    public string CreateContainerXml() => """
     <?xml version="1.0" encoding="UTF-8"?>
     <container version="1.0" xmlns="urn:oasis:names:tc:opendocument:xmlns:container">
         <rootfiles>
             <rootfile full-path="OEBPS/book.opf" media-type="application/oebps-package+xml" />
         </rootfiles>
     </container>
     """;
}
