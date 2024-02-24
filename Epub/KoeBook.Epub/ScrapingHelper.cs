using System.Net;
using System.Reflection.Metadata;

namespace KoeBook.Epub;

internal static class ScrapingHelper
{
    internal static void checkChapter(EpubDocument document)
    {
        if (document.Chapters.Count == 0)
        {
            document.Chapters.Add(new Chapter() { Title = null });
        }

        return;

    }

    internal static void checkSection(EpubDocument document, int ChapterNum)
    {

        checkChapter(document);

        if (document.Chapters[ChapterNum].Sections.Count == 0)
        {
            if (document.Chapters[ChapterNum].Title != null)
            {
                document.Chapters[ChapterNum].Sections.Add(new Section(document.Chapters[ChapterNum].Title!));
            } else
            {
                document.Chapters[ChapterNum].Sections.Add(new Section(document.Title));
            }
            
        }
        return;
    }

    internal static void checkParagraph(EpubDocument document, int chapterNum, int sectionNum)
    {
        checkSection(document, chapterNum);
        if (document.Chapters[chapterNum].Sections[sectionNum].Elements.Count == 0)
        {
            document.Chapters[chapterNum].Sections[sectionNum].Elements.Add(new Paragraph());
        }
        return;
    }
}
