namespace KoeBook.Epub;

public abstract class Element
{
    public CssStyle? Style { get; set; }

    internal string FileName { get; set; } = "";
}
