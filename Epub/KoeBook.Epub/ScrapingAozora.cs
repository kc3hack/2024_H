using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Io;
using KoeBook.Epub.Service;
using System.IO;
using static KoeBook.Epub.ScrapingHelper;


namespace KoeBook.Epub
{
    public partial class ScrapingAozora: IScrapingService
    {
        private int chapterNum;
        private int sectionNum;
        private bool chapterExist = false;
        private bool sectionExist = false;


        public async Task<EpubDocument> ScrapingAsync(string url, string coverFilePath, string imageDirectory, Guid id, CancellationToken ct)
        {
            var config = Configuration.Default.WithDefaultLoader();
            using var context = BrowsingContext.New(config);
            var doc = await context.OpenAsync(url, ct).ConfigureAwait(false);

            // title の取得
            var bookTitle = doc.QuerySelector(".title");
            if (bookTitle is null)
            {
                throw new EpubDocumentException($"Failed to get title properly.\nYou may be able to get proper URL at {GetCardUrl(url)}");
            }

            // auther の取得
            var bookAuther = doc.QuerySelector(".author");
            if (bookAuther is null)
            {
                throw new EpubDocumentException($"Failed to get auther properly.\nYou may be able to get proper URL at {GetCardUrl(url)}");
            }

            // EpubDocument の生成
            var document = new EpubDocument(TextReplace(bookTitle.InnerHtml), TextReplace(bookAuther.InnerHtml), coverFilePath, id)
            {
                // EpubDocument.Chapters の生成
                Chapters = new List<Chapter>()
            };

            // 目次を取得
            var contents = doc.QuerySelectorAll(".midashi_anchor");

            // 目次からEpubDocumentを構成
            List<int> contentsIds = new List<int>() { 0 };
            // Chapter, Section が存在するとき、それぞれtrue
            chapterExist = false;
            sectionExist = false;
            if (contents.Length != 0)
            {
                int previousMidashiId = 0;
                foreach (var midashi in contents)
                {
                    if (midashi.Id != null)
                    {
                        var MidashiId = int.Parse(midashi.Id.Replace("midashi", ""));
                        if ((MidashiId - previousMidashiId) == 100)
                        {
                            document.Chapters.Add(new Chapter() { Title = TextProcess(midashi) });
                            chapterExist = true;
                        }
                        if ((MidashiId - previousMidashiId) == 10)
                        {
                            checkChapter(document);
                            document.Chapters[^1].Sections.Add(new Section(TextProcess(midashi)));
                            sectionExist = true;
                        }
                        contentsIds.Add(MidashiId);
                        previousMidashiId = MidashiId;
                    }
                }
            }
            else
            {
                document.Chapters.Add(new Chapter() { Title = null });
                document.Chapters[^1].Sections.Add(new Section(bookTitle.InnerHtml));
            }

            // 本文を取得
            var mainText = doc.QuerySelector(".main_text")!;


            // 本文を分割しながらEpubDocumntに格納
            // 直前のNodeを確認した操作で、その内容をParagraphに追加した場合、true
            bool previous = false;
            // 各ChapterとSection のインデックス
            chapterNum = -1;
            sectionNum = -1;

            // 直前のimgタグにaltがなかったときtrueになる。
            bool skipCaption = false;

            foreach (var element in mainText.Children)
            {
                var nextNode = element.NextSibling;
                if (element.TagName == "BR")
                {
                    if (previous == true)
                    {
                        checkSection(document, chapterNum);
                        document.Chapters[chapterNum].Sections[sectionNum].Elements.Add(new Paragraph());
                    }

                }
                else if (element.TagName == "DIV")
                {
                    var midashi = element.QuerySelector(".midashi_anchor");
                    if (midashi != null)
                    {
                        if (midashi.Id != null)
                        {
                            if (int.TryParse(midashi.Id.Replace("midashi", ""), out var midashiId))
                            {
                                if (contentsIds.Contains(midashiId))
                                {
                                    var contentsId = contentsIds.IndexOf(midashiId);
                                    switch (contentsIds[contentsId] - contentsIds[contentsId - 1])
                                    {
                                        case 100:
                                            if (chapterNum >= 0 && sectionNum >= 0)
                                            {
                                                document.Chapters[chapterNum].Sections[sectionNum].Elements.RemoveAt(document.Chapters[chapterNum].Sections[sectionNum].Elements.Count - 1);
                                            }
                                            chapterNum++;
                                            sectionNum = -1;
                                            break;
                                        case 10:
                                            if (chapterNum == -1)
                                            {
                                                chapterNum++;
                                                sectionNum = -1;
                                            }
                                            if (chapterNum >= 0 && sectionNum >= 0)
                                            {
                                                document.Chapters[chapterNum].Sections[sectionNum].Elements.RemoveAt(document.Chapters[chapterNum].Sections[sectionNum].Elements.Count - 1);
                                            }
                                            sectionNum++;
                                            break;
                                        default:
                                            break;
                                    }
                                }
                                else //小見出し、行中小見出しの処理
                                {
                                    if (chapterNum == -1)
                                    {
                                        if (chapterExist)
                                        {
                                            document.Chapters.Insert(0, new Chapter());
                                        }
                                        chapterNum++;
                                        sectionNum = -1;
                                    }
                                    if (sectionNum == -1)
                                    {
                                        if (sectionExist)
                                        {
                                            checkChapter(document);
                                            document.Chapters[^1].Sections.Insert(0, new Section("___"));
                                        }
                                        sectionNum++;
                                    }
                                    checkParagraph(document, chapterNum, sectionNum);
                                    if ((document.Chapters[chapterNum].Sections[sectionNum].Elements[^1] is Paragraph paragraph))
                                    {
                                        paragraph.Text += TextProcess(midashi);
                                        document.Chapters[chapterNum].Sections[sectionNum].Elements.Add(new Paragraph());

                                        foreach (var splitText in SplitBrace(TextProcess(midashi)))
                                        {
                                            if (document.Chapters[chapterNum].Sections[sectionNum].Elements[^1] is Paragraph paragraph1)
                                            {
                                                paragraph1.Text += splitText;
                                            }
                                            document.Chapters[chapterNum].Sections[sectionNum].Elements.Add(new Paragraph());
                                        }

                                    }
                                }
                            }
                            else
                            {
                                throw new EpubDocumentException($"Unexpected id of Anchor tag was found: id = {midashi.Id}");
                            }
                        }
                        else
                        {
                            throw new EpubDocumentException("Unecpected structure of HTML File: div tag with class=\"midashi_anchor\", but id=\"midashi___\" exist");
                        }
                    }
                    else
                    {
                        if (element.ClassName == "caption")
                        {
                            // https://www.aozora.gr.jp/annotation/graphics.html#:~:text=%3Cdiv%20class%3D%22caption%22%3E を処理するための部分
                            checkParagraph(document, chapterNum, sectionNum);
                            if ((document.Chapters[chapterNum].Sections[sectionNum].Elements[^1] is Paragraph paragraph))
                            {
                                var split = SplitBrace(TextProcess(element));
                                for (int i = 0; i < split.Count - 1; i++)
                                {
                                    if (document.Chapters[chapterNum].Sections[sectionNum].Elements[^1] is Paragraph paragraph1)
                                    {
                                        paragraph1.Text += split[i];
                                    }
                                    document.Chapters[chapterNum].Sections[sectionNum].Elements.Add(new Paragraph());
                                }
                                if (document.Chapters[chapterNum].Sections[sectionNum].Elements[^1] is Paragraph paragraph2)
                                {
                                    paragraph2.Text += split[^1];
                                }
                            }
                        }
                        else
                        {
                            if (chapterNum == -1)
                            {
                                if (chapterExist)
                                {
                                    document.Chapters.Insert(0, new Chapter());
                                }
                                chapterNum++;
                                sectionNum = -1;
                            }
                            if (sectionNum == -1)
                            {
                                if (sectionExist)
                                {
                                    checkChapter(document);
                                    document.Chapters[^1].Sections.Insert(0, new Section("___"));
                                }
                                sectionNum++;
                            }
                            checkParagraph(document, chapterNum, sectionNum);
                            if ((document.Chapters[chapterNum].Sections[sectionNum].Elements[^1] is Paragraph paragraph))
                            {
                                foreach (var splitText in SplitBrace(TextProcess(element)))
                                {
                                    if (document.Chapters[chapterNum].Sections[sectionNum].Elements[^1] is Paragraph paragraph1)
                                    {
                                        paragraph1.Text += splitText;
                                    }
                                    document.Chapters[chapterNum].Sections[sectionNum].Elements.Add(new Paragraph());
                                }
                            }
                        }
                    }

                }
                else if (element.TagName == "IMG")
                {
                    if (element is IHtmlImageElement img)
                    {
                        if (chapterNum == -1)
                        {
                            if (chapterExist)
                            {
                                document.Chapters.Insert(0, new Chapter());
                            }
                            chapterNum++;
                            sectionNum = -1;
                        }
                        if (sectionNum == -1)
                        {
                            if (sectionExist)
                            {
                                checkChapter(document);
                                document.Chapters[^1].Sections.Insert(0, new Section("___"));
                            }
                            sectionNum++;
                        }

                        if (element.ClassName != "gaiji")
                        {
                            if (img.Source != null)
                            {
                                // 画像のダウンロード
                                var loader = context.GetService<IDocumentLoader>();
                                if (loader != null)
                                {
                                    var downloading = loader.FetchAsync(new DocumentRequest(new Url(img.Source)));
                                    ct.Register(() => downloading.Cancel());
                                    var response = await downloading.Task.ConfigureAwait(false);
                                    using var ms = new MemoryStream();
                                    await response.Content.CopyToAsync(ms, ct).ConfigureAwait(false);
                                    var filePass = imageDirectory + FileUrlToFileName().Replace(img.Source, "$1");
                                    File.WriteAllBytes(filePass, ms.ToArray());
                                    checkSection(document, chapterNum);
                                    if (document.Chapters[chapterNum].Sections[sectionNum].Elements.Count > 1)
                                    {
                                        document.Chapters[chapterNum].Sections[sectionNum].Elements.Insert(document.Chapters[chapterNum].Sections[sectionNum].Elements.Count - 1, new Picture(filePass));
                                    }
                                }
                            }
                            if (img.AlternativeText != null)
                            {
                                checkParagraph(document, chapterNum, sectionNum);
                                if ((document.Chapters[chapterNum].Sections[sectionNum].Elements[^1] is Paragraph paragraph))
                                {
                                    paragraph.Text += TextReplace(img.AlternativeText);
                                    document.Chapters[chapterNum].Sections[sectionNum].Elements.Add(new Paragraph());
                                }
                                skipCaption = false;
                            }
                            else
                            {
                                skipCaption = true;
                            }
                        }
                    }
                }
                else if (element.TagName == "SPAN")
                {
                    if (element.ClassName == "caption")
                    {
                        if (skipCaption)
                        {
                            if ((document.Chapters[chapterNum].Sections[sectionNum].Elements[^2] is Paragraph paragraph))
                            {
                                paragraph.Text = TextProcess(element) + "の画像";
                            }
                        }
                        else
                        {
                            if ((document.Chapters[chapterNum].Sections[sectionNum].Elements[^1] is Paragraph paragraph))
                            {
                                paragraph.Text = TextProcess(element) + "の画像";
                            }
                        }

                    }
                    else if (element.ClassName == "notes")
                    {
                        switch (element.InnerHtml)
                        {
                            case "［＃改丁］":
                                break;
                            case "［＃改ページ］":
                                break;
                            case "［＃改見開き］":
                                break;
                            case "［＃改段］":
                                break;
                            case "［＃ページの左右中央］":
                                break;
                            default:
                                checkParagraph(document, chapterNum, sectionNum);
                                if ((document.Chapters[chapterNum].Sections[sectionNum].Elements[^1] is Paragraph paragraph))
                                {
                                    foreach (var splitText in SplitBrace(TextProcess(element)))
                                    {
                                        if (document.Chapters[chapterNum].Sections[sectionNum].Elements[^1] is Paragraph paragraph1)
                                        {
                                            paragraph1.Text += splitText;
                                        }
                                        document.Chapters[chapterNum].Sections[sectionNum].Elements.Add(new Paragraph());
                                    }
                                }
                                break;
                        }
                    }
                    else
                    {
                        if (chapterNum == -1)
                        {
                            if (chapterExist)
                            {
                                document.Chapters.Insert(0, new Chapter());
                            }
                            chapterNum++;
                            sectionNum = -1;
                        }
                        if (sectionNum == -1)
                        {
                            if (sectionExist)
                            {
                                checkChapter(document);
                                document.Chapters[^1].Sections.Insert(0, new Section("___"));
                            }
                            sectionNum++;
                        }
                        checkParagraph(document, chapterNum, sectionNum);
                        if ((document.Chapters[chapterNum].Sections[sectionNum].Elements[^1] is Paragraph paragraph))
                        {
                            var split = SplitBrace(TextProcess(element));
                            for (int i = 0; i < split.Count - 1; i++)
                            {
                                if (document.Chapters[chapterNum].Sections[sectionNum].Elements[^1] is Paragraph paragraph1)
                                {
                                    paragraph1.Text += split[i];
                                }
                                document.Chapters[chapterNum].Sections[sectionNum].Elements.Add(new Paragraph());
                            }
                            if (document.Chapters[chapterNum].Sections[sectionNum].Elements[^1] is Paragraph paragraph2)
                            {
                                paragraph2.Text += split[^1];
                            }
                        }
                        // 想定していない構造が見つかったことをログに出力した方が良い？
                    }
                }
                else
                {
                    if (chapterNum == -1)
                    {
                        if (chapterExist)
                        {
                            document.Chapters.Insert(0, new Chapter());
                        }
                        chapterNum++;
                        sectionNum = -1;
                    }
                    if (sectionNum == -1)
                    {
                        if (sectionExist)
                        {
                            checkChapter(document);
                            document.Chapters[^1].Sections.Insert(0, new Section("___"));
                        }
                        sectionNum++;
                    }
                    checkParagraph(document, chapterNum, sectionNum);
                    if ((document.Chapters[chapterNum].Sections[sectionNum].Elements[^1] is Paragraph paragraph))
                    {
                        paragraph.Text += TextProcess(element);

                        var split = SplitBrace(TextProcess(element));
                        for (int i = 0; i < split.Count - 1; i++)
                        {
                            if (document.Chapters[chapterNum].Sections[sectionNum].Elements[^1] is Paragraph paragraph1)
                            {
                                paragraph1.Text += split[i];
                            }
                            document.Chapters[chapterNum].Sections[sectionNum].Elements.Add(new Paragraph());
                        }
                        if (document.Chapters[chapterNum].Sections[sectionNum].Elements[^1] is Paragraph paragraph2)
                        {
                            paragraph2.Text += split[^1];
                        }
                    }
                    // 想定していない構造が見つかったことをログに出力した方が良い？
                }

                if (nextNode != null)
                {
                    if (nextNode.NodeType == NodeType.Text)
                    {
                        if (nextNode.Text() != "\n")
                        {
                            previous = true;

                            if (chapterNum == -1)
                            {
                                if (chapterExist)
                                {
                                    document.Chapters.Insert(0, new Chapter());
                                }
                                chapterNum++;
                                sectionNum = -1;
                            }
                            if (sectionNum == -1)
                            {
                                if (sectionExist)
                                {
                                    checkChapter(document);
                                    document.Chapters[^1].Sections.Insert(0, new Section("___"));
                                }
                                sectionNum++;
                            }
                            checkParagraph(document, chapterNum, sectionNum);
                            if ((document.Chapters[chapterNum].Sections[sectionNum].Elements[^1] is Paragraph paragraph))
                            {
                                var split = SplitBrace(TextReplace(nextNode.Text()));
                                for (int i = 0; i < split.Count - 1; i++)
                                {
                                    if (document.Chapters[chapterNum].Sections[sectionNum].Elements[^1] is Paragraph paragraph1)
                                    {
                                        paragraph1.Text += split[i];
                                    }
                                    document.Chapters[chapterNum].Sections[sectionNum].Elements.Add(new Paragraph());
                                }
                                if (document.Chapters[chapterNum].Sections[sectionNum].Elements[^1] is Paragraph paragraph2)
                                {
                                    paragraph2.Text += split[^1];
                                }
                            }

                        }
                        else
                        {
                            previous = false;
                        }
                    }
                    else
                    {
                        previous = false;
                    }
                }
            }

            document.Chapters[^1].Sections[^1].Elements.RemoveAt(document.Chapters[^1].Sections[^1].Elements.Count - 1);

            if (checkEpubDocument(document))
            {
                Console.WriteLine("Success");
            }
            else
            {
                Console.WriteLine("False");
            }
            return document;
        }

        private bool checkEpubDocument(EpubDocument document)
        {
            foreach (var chapter in document.Chapters)
            {
                foreach (var section in chapter.Sections)
                {
                    foreach (var element in section.Elements)
                    {
                        if (element is Paragraph paragraph)
                        {
                            if (paragraph.Text == null)
                            {
                                Console.WriteLine($"{document.Chapters.IndexOf(chapter)}, {chapter.Sections.IndexOf(section)}, {section.Elements.IndexOf(element)}");
                                return false;
                            }
                        }
                        else if (element is Picture picture)
                        {
                            if (picture.PictureFilePath == null)
                            {
                                Console.WriteLine($"{document.Chapters.IndexOf(chapter)}, {chapter.Sections.IndexOf(section)}, {section.Elements.IndexOf(element)}");
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }

        private static string TextProcess(IElement element)
        {
            string text = "";
            if (element.ChildElementCount == 0)
            {
                text += TextReplace(element.InnerHtml);
            }
            else
            {
                var rubies = element.QuerySelectorAll("ruby");
                if (rubies.Length > 0)
                {
                    if (element.Children[0].PreviousSibling is INode node)
                    {
                        if (node.NodeType == NodeType.Text)
                        {
                            if (node.Text() != "\n")
                            {
                                text += TextReplace(node.Text());
                            }
                        }
                    }
                    foreach (var item in element.Children)
                    {
                        if (item.TagName == "RUBY")
                        {
                            if (item.QuerySelectorAll("img").Length > 0)
                            {
                                if (item.QuerySelector("rt") != null)
                                {
                                    text += TextReplace(item.QuerySelector("rt")!.TextContent);
                                }
                            }
                            else
                            {
                                text += TextReplace(item.OuterHtml);
                            }
                        }
                        else
                        {
                            if ((item.TextContent != "\n") && (!string.IsNullOrEmpty(item.TextContent)))
                            {
                                text += TextReplace(item.TextContent);
                            }
                        }
                        if (item.NextSibling != null)
                        {
                            if ((item.NextSibling.TextContent != "\n") && (!string.IsNullOrEmpty(item.NextSibling.TextContent)))
                            {
                                text += TextReplace(item.NextSibling.Text());
                            }
                        }
                    }
                }
                else if (element.TagName == "RUBY")
                {
                    if (element.QuerySelectorAll("img").Length > 0)
                    {
                        if (element.QuerySelector("rt") != null)
                        {
                            text += TextReplace(element.QuerySelector("rt")!.TextContent);
                        }
                    }
                    else
                    {
                        text += TextReplace(element.OuterHtml);
                    }
                }
                else
                {
                    text += TextReplace(element.TextContent);
                }
            }
            return text;
        }


        // ローマ数字、改行の置換をまとめて行う。
        private static string TextReplace(string text)
        {
            string returnText = text;
            returnText = RomanNumImg().Replace(returnText, "$1");
            returnText = RomanNumText1().Replace(returnText, "$1");
            returnText = RomanNumText2().Replace(returnText, "$1");
            returnText = returnText.Replace("\n", "");
            return returnText;
        }


        private static string GetCardUrl(string url)
        {
            return UrlBookToCard().Replace(url, "$1card$2$3");
        }

        [System.Text.RegularExpressions.GeneratedRegex(@"(https://www\.aozora\.gr\.jp/cards/\d{6}/)files/(\d{1,})_\d{1,}(\.html)")]
        private static partial System.Text.RegularExpressions.Regex UrlBookToCard();

        [System.Text.RegularExpressions.GeneratedRegex(@"<img src=""\.\./\.\./\.\./gaiji/1-1\d/1-1\d.{0,}\.png""alt=""※(ローマ数字(\d{1,})、1-1\d.{0,})"" class=""gaiji"">")]
        private static partial System.Text.RegularExpressions.Regex RomanNumImg();

        [System.Text.RegularExpressions.GeneratedRegex(@"※［＃ローマ数字(\d{1,})、1-1\d.{0,}］")]
        private static partial System.Text.RegularExpressions.Regex RomanNumText1();

        [System.Text.RegularExpressions.GeneratedRegex(@"※(ローマ数字(\d.{1,})、1-1\d.{0,})")]
        private static partial System.Text.RegularExpressions.Regex RomanNumText2();

        [System.Text.RegularExpressions.GeneratedRegex(@"http.{1,}/([^/]{0,}\.[^/]{1,})")]
        private static partial System.Text.RegularExpressions.Regex FileUrlToFileName();

        [System.Text.RegularExpressions.GeneratedRegex(@"<ruby><rb>(.{1,})</rb><rp>（</rp><rt>(.{1,})</rt><rp>）</rp></ruby>")]
        private static partial System.Text.RegularExpressions.Regex RubyToText();
    }
}
