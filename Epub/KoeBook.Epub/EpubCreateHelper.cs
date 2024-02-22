namespace KoeBook.Epub;

internal static class EpubCreateHelper
{
    internal static string GetImagesMediaType(string path)
    {
        return Path.GetExtension(path) switch
        {
            ".gif" => "image/gif",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".svg" => "image/svg+xml",
            ".webp" => "image/webp",
            _ => ""
        };
    }
}
