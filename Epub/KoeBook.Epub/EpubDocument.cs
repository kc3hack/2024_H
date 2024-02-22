using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Xml;
using NAudio.Mixer;

namespace KoeBook.Epub;

public class EpubDocument(string title, string author, string coverFIlePath)
{
    readonly string _containerXml = """
     <?xml version="1.0" encoding="UTF-8"?>
     <container version="1.0" xmlns="urn:oasis:names:tc:opendocument:xmlns:container">
         <rootfiles>
             <rootfile full-path="OEBPS/book.opf" media-type="application/oebps-package+xml" />
         </rootfiles>
     </container>
     """;

    public string Title { get; set; } = title;
    public string Author { get; set; } = author;

    public string CoverFilePath { get; set; } = coverFIlePath;

    public List<CssClass> CssClasses { get; set; } = [
            new CssClass("-epub-media-overlay-active","""
                .-epub-media-overlay-active *{
                    background-color: yellow;
                    color: black !important;
                }
                """),
            new CssClass("-epub-media-overlay-unactive","""
                .-epub-media-overlay-unactive * {
                    color: gray;
                }
                """),
        ];
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
            for (int i = 0; i < Chapters[0].Sections.Count; i++)
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
            for (int i = 0; i < Chapters?.Count; i++)
            {
                builder.AppendLine($"""
                                    <li>
                                        <span>{Chapters[i].Title}</span>
                                        <ol>
                    """);
                for (int j = 0; j < Chapters[i].Sections.Count; j++)
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
        for (int i = 0; i < Chapters?.Count; i++)
        {
            for (int j = 0; j < Chapters[i].Sections.Count; j++)
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

        for (int i = 0; i < Chapters?.Count; i++)
        {
            for (int j = 0; j < Chapters[i].Sections.Count; j++)
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
                    else if(element is Picture pic && File.Exists(pic.PictureFilePath))
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

        for (int i = 0; i < Chapters?.Count; i++)
        {
            for (int j = 0; j < Chapters[i].Sections.Count; j++)
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

    public async Task<bool> TryCreateEpubAsync(string tmpDirectory, string? name, CancellationToken ct)
    {
        if (!File.Exists(CoverFilePath))
        {
            throw new FileNotFoundException("指定されたカバーファイルが存在しません", CoverFilePath);
        }
        try
        {
            using var fs = File.Create(Path.Combine(tmpDirectory, $"{ name ?? Title}.epub"));
            using var archive = new ZipArchive(fs, ZipArchiveMode.Create);

            var mimeTypeEntry = archive.CreateEntry("mimetype", CompressionLevel.NoCompression);
            using (var mimeTypeStream = new StreamWriter(mimeTypeEntry.Open()))
            {
                await mimeTypeStream.WriteAsync("application/epub+zip").ConfigureAwait(false);
                await mimeTypeStream.FlushAsync(ct).ConfigureAwait(false);
            }

            var containerEntry = archive.CreateEntry("META-INF/container.xml");
            using (var containerStream = new StreamWriter(containerEntry.Open()))
            {
                await containerStream.WriteLineAsync(_containerXml).ConfigureAwait(false);
                await containerStream.FlushAsync(ct).ConfigureAwait(false);
            }

            var coverEntry = archive.CreateEntry($"OEBPS/{Path.GetFileName(CoverFilePath)}");
            using (var coverStream = coverEntry.Open())
            using (var coverFileStream = File.OpenRead(CoverFilePath))
            {
                await coverFileStream.CopyToAsync(coverStream, ct).ConfigureAwait(false);
                await coverStream.FlushAsync(ct).ConfigureAwait(false);
            }

            var cssEntry = archive.CreateEntry("OEBPS/style.css");
            using (var cssStream = new StreamWriter(cssEntry.Open()))
            {
                await cssStream.WriteLineAsync(CreateCssText()).ConfigureAwait(false);
                await cssStream.FlushAsync(ct).ConfigureAwait(false);
            }

            var navEntry = archive.CreateEntry("OEBPS/nav.xhtml");
            using (var navStream = new StreamWriter(navEntry.Open()))
            {
                await navStream.WriteLineAsync(CreateNavXhtml()).ConfigureAwait(false);
                await navStream.FlushAsync(ct).ConfigureAwait(false);
            }

            var opfEntry = archive.CreateEntry("OEBPS/book.opf");
            using (var opfStream = new StreamWriter(opfEntry.Open()))
            {
                await opfStream.WriteLineAsync(CreateOpf()).ConfigureAwait(false);
                await opfStream.FlushAsync(ct).ConfigureAwait(false);
            }

            for (int i = 0; i < Chapters?.Count; i++)
            {
                for (int j = 0; j < Chapters[i].Sections.Count; j++)
                {
                    var sectionXhtmlEntry = archive.CreateEntry($"OEBPS/{Chapters[i].Sections[j].Id}.xhtml");
                    using (var sectionXhtmlStream = new StreamWriter(sectionXhtmlEntry.Open()))
                    {
                        await sectionXhtmlStream.WriteLineAsync(Chapters[i].Sections[j].CreateSectionXhtml()).ConfigureAwait(false);
                        await sectionXhtmlStream.FlushAsync(ct).ConfigureAwait(false);
                    }
                    var sectionSmilEntry = archive.CreateEntry($"OEBPS/{Chapters[i].Sections[j].Id}_audio.smil");
                    using (var sectionSmilStream = new StreamWriter(sectionSmilEntry.Open()))
                    {
                        await sectionSmilStream.WriteLineAsync(Chapters[i].Sections[j].CreateSectionSmil()).ConfigureAwait(false);
                        await sectionSmilStream.FlushAsync(ct).ConfigureAwait(false);
                    }
                    for (var k = 0; k < Chapters[i].Sections[j].Elements.Count; k++)
                    {
                        var element = Chapters[i].Sections[j].Elements[k];
                        if (element is Paragraph para && para.Audio != null)
                        {
                            var audioEntry = archive.CreateEntry($"OEBPS/{Chapters[i].Sections[j].Id}_p{k}.mp3");
                            using var audioStream = await para.Audio.GetStreamAsync(ct).ConfigureAwait(false);
                            using var audioEntryStream = audioEntry.Open();
                            await audioStream.CopyToAsync(audioEntryStream, ct).ConfigureAwait(false);
                            await audioEntryStream.FlushAsync(ct).ConfigureAwait(false);
                        }
                        else if (element is Picture pic && File.Exists(pic.PictureFilePath))
                        {
                            var pictureEntry = archive.CreateEntry($"OEBPS/{Chapters[i].Sections[j].Id}_p{k}{Path.GetExtension(pic.PictureFilePath)}");
                            using var pictureEntryStream = pictureEntry.Open();
                            using var pictureFileStream = File.OpenRead(pic.PictureFilePath);
                            await pictureFileStream.CopyToAsync(pictureEntryStream, ct).ConfigureAwait(false);
                            await pictureFileStream.FlushAsync(ct).ConfigureAwait(false);
                        }
                    }
                }
            }
            return true;
        }
        catch
        {
            return false;
        }
    }
}
