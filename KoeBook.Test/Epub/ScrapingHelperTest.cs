using KoeBook.Epub.Utility;

namespace KoeBook.Test.Epub;

public class ScrapingHelperTest
{
    public static IEnumerable<object[]> Data()
    {
        yield return new object[] { "「", new List<string> { "「" } };
        yield return new object[] { "」", new List<string> { "」" } };
        yield return new object[] { "a", new List<string> { "a" } };
        yield return new object[] { "abc「abc」abc", new List<string> { "abc", "「abc」", "abc" } };
        yield return new object[] { "abc「abc」", new List<string> { "abc", "「abc」" } };
        yield return new object[] { "「abc」abc", new List<string> { "「abc」", "abc", } };
        yield return new object[] { "abc「abc」", new List<string> { "abc", "「abc」" } };
        yield return new object[] { "「abc」", new List<string> { "「abc」", } };
        yield return new object[] { "abc「abc", new List<string> { "abc", "「abc" } };
        yield return new object[] { "abc「", new List<string> { "abc", "「" } };
        yield return new object[] { "「abc", new List<string> { "「abc" } };
        yield return new object[] { "abc」abc", new List<string> { "abc」", "abc" } };
        yield return new object[] { "abc」", new List<string> { "abc」" } };
        yield return new object[] { "」abc", new List<string> { "」", "abc" } };
        yield return new object[] { "abc「abc」abc「abc」abc", new List<string> { "abc", "「abc」", "abc", "「abc」", "abc" } };
        yield return new object[] { "「abc」abc「abc」abc", new List<string> { "「abc」", "abc", "「abc」", "abc" } };
        yield return new object[] { "abc「abc」「abc」abc", new List<string> { "abc", "「abc」", "「abc」", "abc" } };
        yield return new object[] { "abc「abc」abc「abc」", new List<string> { "abc", "「abc」", "abc", "「abc」" } };
    }

    [Theory]
    [MemberData(nameof(Data))]
    public void SplitBraceTest(string text, List<string> expected)
    {
        Assert.Equal(expected, ScrapingHelper.SplitBrace(text));

    }
}
