using System.Net;
using System.Reflection.Metadata;
using KoeBook.Epub.Models;

namespace KoeBook.Epub;

public static class ScrapingHelper
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
            }
            else
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

    public static List<string> SplitBrace(string text)
    {
        if (text.Length == 1 && text != "「" && text != "」")
        {
            return new List<string>() { text };
        }
        var result = new List<string>();
        int bracket = 0;
        var brackets = new List<int>();
        foreach (char c in text)
        {
            if (c == '「') bracket++;
            if (c == '」') bracket--;
            brackets.Add(bracket);
        }
        var mn = Math.Min(0, brackets.Min());
        int startIdx = 0;
        for (int i = 0; i < brackets.Count; i++)
        {
            brackets[i] -= mn;
            if (text[i] == '「' && brackets[i] == 1 && i != 0)
            {
                result.Add(text[startIdx..i]);
                startIdx = i;
            }
            if (text[i] == '」' && brackets[i] == 0 && i != 0)
            {
                result.Add(text[startIdx..(i + 1)]);
                startIdx = i + 1;
            }
        }
        if (startIdx != text.Length - 1)
        {
            result.Add(text[startIdx..]);
        }
        if (result[^1] == "")
        {
            result.RemoveAt(result.Count - 1);
        }

        return result;
    }
}
