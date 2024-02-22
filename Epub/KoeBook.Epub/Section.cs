using System.Text;

namespace KoeBook.Epub;

public sealed class Section(string title)
{
    public string Id { get; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = title;
    public List<Element> Elements { get; set; } = [];

    public string CreateSectionXhtml()
    {
        var builder = new StringBuilder($"""
            <?xml version="1.0" encoding="UTF-8"?>
            <html xmlns="http://www.w3.org/1999/xhtml" xml:lang="ja" lang="ja">
                <head>  
                    <link rel="stylesheet" type="text/css" media="all" href="style.css"/>
                    <title>{Title}</title> 
                </head> 
                <body>
            """);

        for (int i = 0; i < Elements.Count; i++)
        {
            if (Elements[i] is Paragraph para)
            {
                builder.AppendLine($"""
                            <p id="s_{Id}_p{i}" {(para.ClassName != null ? $"class=\"{para.ClassName}\"" : "")}>
                                {para.Text}
                            </p>
                    """);
            }
            else if (Elements[i] is Picture pic && File.Exists(pic.PictureFilePath))
            {
                builder.AppendLine($"""
                            <p id="s_{Id}_p{i}" {(pic.ClassName != null ? $"class=\"{pic.ClassName}\"" : "")}>
                                <img src="{Path.GetFileName(pic.PictureFilePath)}"
                            </p>
                    """);
            }
        }

        builder.AppendLine("""
                </body>
            </html>
            """);
        return builder.ToString();
    }

    public string CreateSectionSmil()
    {
        var builder = new StringBuilder($"""
            <?xml version="1.0" encoding="UTF-8"?>
            <smil xmlns="http://www.w3.org/ns/SMIL" version="3.0">
                <body>
            """);

        for (int i = 0; i < Elements.Count; i++)
        {
            if (Elements[i] is Paragraph para && para.Audio != null)
            {
                builder.AppendLine($"""
                    <par id="s_{Id}_p{i}_audio" {(para.ClassName != null ? $"class=\"{para.ClassName}\"" : "")}>
                        <text src="{Id}.xhtml#s_{Id}_p{i}" />
                        <audio clipBegin="0s" clipEnd="{para.Audio?.TotalTime}" src="{Id}_p{i}.mp3"/>
                    </par>
            """);
            }
        }

        builder.AppendLine("""
                </body>
            </smil>
            """);
        return builder.ToString();
    }

    public TimeSpan GetTotalTime()
    {
        var time = TimeSpan.Zero;
        foreach (var element in Elements)
        {
            if(element is Paragraph para && para.Audio != null) {
                time += para.Audio.TotalTime;
            }
        }
        return time;
    }
}
