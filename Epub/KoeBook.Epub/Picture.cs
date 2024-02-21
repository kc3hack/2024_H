namespace KoeBook.Epub;

public class Picture(string path) : Element
{
    public string PictureFilePath { get; set; } = path;
}
