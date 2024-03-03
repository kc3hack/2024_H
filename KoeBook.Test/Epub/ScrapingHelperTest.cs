using KoeBook.Epub.Utility;

namespace KoeBook.Test.Epub;

public class ScrapingHelperTest
{
    public static object[][] TestCases()
    {
        (string, List<string>)[] cases = [
            ("「", ["「"]),
            ("」", ["」"]),
            ("a", ["a"]),
            ("abc「abc」abc", ["abc", "「abc」", "abc"]),
            ("abc「abc」", ["abc", "「abc」"]),
            ("「abc」abc", ["「abc」", "abc",]),
            ("abc「abc」", ["abc", "「abc」"]),
            ("「abc」", ["「abc」",]),
            ("abc「abc", ["abc", "「abc"]),
            ("abc「", ["abc", "「"]),
            ("「abc", ["「abc"]),
            ("abc」abc", ["abc」", "abc"]),
            ("abc」", ["abc」"]),
            ("」abc", ["」", "abc"]),
            ("abc「abc」abc「abc」abc", ["abc", "「abc」", "abc", "「abc」", "abc"]),
            ("「abc」abc「abc」abc", ["「abc」", "abc", "「abc」", "abc"]),
            ("abc「abc」「abc」abc", ["abc", "「abc」", "「abc」", "abc"]),
            ("abc「abc」abc「abc」", ["abc", "「abc」", "abc", "「abc」"]),
            ("abc「abc「abc」abc", ["abc", "「abc「abc」abc"]),
            ("abc「abc」abc」abc", ["abc「abc」abc」", "abc"]),
            ("abc「abc「abc", ["abc", "「abc「abc"]),
            ("abc」abc」abc", ["abc」abc」", "abc"])
        ];
        return cases.Select(c => new object[] { c.Item1, c.Item2 }).ToArray();
    }

    [Theory]
    [MemberData(nameof(TestCases))]
    public void SplitBraceTest(string text, List<string> expected)
    {
        Assert.Equal(expected, ScrapingHelper.SplitBrace(text));

    }
}
