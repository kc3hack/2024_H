using KoeBook.Epub.Contracts.Services;

namespace KoeBook.Epub.Services;

public class FileExtensionService : IFileExtensionService
{
    public string GetImagesMediaType(string fileName)
    {
        return Path.GetExtension(fileName) switch
        {
            ".gif" => "image/gif",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".svg" => "image/svg+xml",
            ".webp" => "image/webp",
            _ => string.Empty,
        };
    }
}
