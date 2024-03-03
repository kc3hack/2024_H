namespace KoeBook.Epub.Utility;

public static class ScrapingHelper
{
    public static List<string> SplitBrace(string text)
    {
        if (text.Length == 1 && (text == "「" || text == "」"))
            return [text];

        var bracket = 0;
        var brackets = new int[text.Length];
        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];
            if (c == '「') bracket++;
            else if (c == '」') bracket--;
            brackets[i] = bracket;
        }

        var result = new List<string>();
        var mn = Math.Min(0, brackets.Min());
        var startIdx = 0;
        for (var i = 0; i < brackets.Length; i++)
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
