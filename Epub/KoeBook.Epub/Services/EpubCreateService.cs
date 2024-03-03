using KoeBook.Epub.Contracts.Services;
using KoeBook.Epub.Models;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO.Compression;
using System.Text;

namespace KoeBook.Epub.Services;
public class EpubCreateService : IEpubCreateService
{
    public async ValueTask<bool> TryCreateEpubAsync(EpubDocument epubDocument, string tmpDirectory, CancellationToken ct)
    {
        if (!File.Exists(epubDocument.CoverFilePath))
        {
            throw new FileNotFoundException("指定されたカバーファイルが存在しません", epubDocument.CoverFilePath);
        }
        try
        {
            using var fs = File.Create(Path.Combine(tmpDirectory, $"{epubDocument.Id}.epub"));
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
                await containerStream.WriteLineAsync(epubDocument.CreateContainerXml()).ConfigureAwait(false);
                await containerStream.FlushAsync(ct).ConfigureAwait(false);
            }

            archive.CreateEntryFromFile(epubDocument.CoverFilePath, $"OEBPS/{Path.GetFileName(epubDocument.CoverFilePath)}");

            var cssEntry = archive.CreateEntry("OEBPS/style.css");
            using (var cssStream = new StreamWriter(cssEntry.Open()))
            {
                await cssStream.WriteLineAsync(epubDocument.CreateCssText()).ConfigureAwait(false);
                await cssStream.FlushAsync(ct).ConfigureAwait(false);
            }

            var navEntry = archive.CreateEntry("OEBPS/nav.xhtml");
            using (var navStream = new StreamWriter(navEntry.Open()))
            {
                await navStream.WriteLineAsync(epubDocument.CreateNavXhtml()).ConfigureAwait(false);
                await navStream.FlushAsync(ct).ConfigureAwait(false);
            }

            var opfEntry = archive.CreateEntry("OEBPS/book.opf");
            using (var opfStream = new StreamWriter(opfEntry.Open()))
            {
                await opfStream.WriteLineAsync(epubDocument.CreateOpf()).ConfigureAwait(false);
                await opfStream.FlushAsync(ct).ConfigureAwait(false);
            }

            for (var i = 0; i < epubDocument.Chapters.Count; i++)
            {
                for (var j = 0; j < epubDocument.Chapters[i].Sections.Count; j++)
                {
                    var sectionXhtmlEntry = archive.CreateEntry($"OEBPS/{epubDocument.Chapters[i].Sections[j].Id}.xhtml");
                    using (var sectionXhtmlStream = new StreamWriter(sectionXhtmlEntry.Open()))
                    {
                        await sectionXhtmlStream.WriteLineAsync(epubDocument.Chapters[i].Sections[j].CreateSectionXhtml()).ConfigureAwait(false);
                        await sectionXhtmlStream.FlushAsync(ct).ConfigureAwait(false);
                    }
                    var sectionSmilEntry = archive.CreateEntry($"OEBPS/{epubDocument.Chapters[i].Sections[j].Id}_audio.smil");
                    using (var sectionSmilStream = new StreamWriter(sectionSmilEntry.Open()))
                    {
                        await sectionSmilStream.WriteLineAsync(epubDocument.Chapters[i].Sections[j].CreateSectionSmil()).ConfigureAwait(false);
                        await sectionSmilStream.FlushAsync(ct).ConfigureAwait(false);
                    }
                    for (var k = 0; k < epubDocument.Chapters[i].Sections[j].Elements.Count; k++)
                    {
                        var element = epubDocument.Chapters[i].Sections[j].Elements[k];
                        if (element is Paragraph para && para.Audio != null)
                        {
                            var audioEntry = archive.CreateEntry($"OEBPS/{epubDocument.Chapters[i].Sections[j].Id}_p{k}.mp3");
                            using var audioStream = para.Audio.GetStream();
                            using var audioEntryStream = audioEntry.Open();
                            await audioStream.CopyToAsync(audioEntryStream, ct).ConfigureAwait(false);
                            await audioEntryStream.FlushAsync(ct).ConfigureAwait(false);
                        }
                        else if (element is Picture pic && File.Exists(pic.PictureFilePath))
                        {
                            archive.CreateEntryFromFile(pic.PictureFilePath, $"OEBPS/{epubDocument.Chapters[i].Sections[j].Id}_p{k}{Path.GetExtension(pic.PictureFilePath)}");
                        }
                    }
                }
            }
            return true;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            return false;
        }
    }
}
