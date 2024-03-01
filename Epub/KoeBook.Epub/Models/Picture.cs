namespace KoeBook.Epub.Models;

public class Picture(string path) : Element
{
    public string PictureFilePath { get; set; } = path;
}
