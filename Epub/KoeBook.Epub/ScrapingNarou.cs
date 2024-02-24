using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Io;
using KoeBook.Epub.Service;
using System.IO;
using System.Net.Http.Json;
using static KoeBook.Epub.ScrapingHelper;

namespace KoeBook.Epub
{
    public partial class ScrapingNarouService : IScrapingService
    {
        public ScrapingNarouService(IHttpClientFactory httpClientFactory)
        {
            _httpCliantFactory = httpClientFactory;
        }

        private readonly IHttpClientFactory _httpCliantFactory;

        public async Task<EpubDocument> ScrapingAsync(string url, string coverFilePath, string imageDirectory, Guid id, CancellationToken ct)
        {
            var config = Configuration.Default.WithDefaultLoader();
            using var context = BrowsingContext.New(config);
            var doc = await context.OpenAsync(url, ct).ConfigureAwait(false);

            // title の取得
            var bookTitle = doc.QuerySelector(".novel_title");
            if (bookTitle is null)
            {
                throw new EpubDocumentException($"Failed to get title properly.\nUrl may be not collect");
            }

            // auther の取得
            var bookAuther = doc.QuerySelector(".novel_writername a");
            if (bookAuther is null)
            {
                throw new EpubDocumentException($"Failed to get auther properly.\nUrl may be not collect");
            }

            bool isRensai = true;
            int allNum = 0;

            var apiUrl = $"https://api.syosetu.com/novelapi/api/?of=ga-nt&ncode={UrlToNcode().Replace(url, "$1")}&out=json";

            // APIを利用して、noveltype : 連載(1)か短編(2)か、general_all_no : 全掲載部分数
            var message = new HttpRequestMessage(System.Net.Http.HttpMethod.Get, apiUrl);
            message.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.45 Safari/537.36");
            var client = _httpCliantFactory.CreateClient();
            var result = await client.SendAsync(message, ct).ConfigureAwait(false);
            var test = await result.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            if (result.IsSuccessStatusCode)
            {
                var content = await result.Content.ReadFromJsonAsync<BookInfo[]>(ct).ConfigureAwait(false);
                if (content != null)
                {
                    if (content[1].noveltype != null)
                    {
                        if (content[1].noveltype == 2)
                        {
                            isRensai = false;
                        }
                    }
                    else
                    {
                        throw new EpubDocumentException("faild to get data by Narou API");
                    }
                    if (content[1].general_all_no != null)
                    {
                        allNum = (int)content[1].general_all_no!;
                    }
                    if (allNum == 0)
                    {
                        throw new EpubDocumentException("faild to get data by Narou API");
                    }
                }
            }
            else
            {
                throw new EpubDocumentException("Url may be not Correct");
            }

            if (isRensai)
            {
                //ReadPageAsync(url, imageDirectory, ct);
                var temp = ReadPageAsync("https://ncode.syosetu.com/n1443bp/2/", imageDirectory, ct);
                Console.WriteLine(temp.Result.Elements.Count);
            }
            else
            {
                ReadPageAsync(url, imageDirectory, ct);
            }

            return new EpubDocument("", "", "", id);

        }

        public record BookInfo(int? allcount, int? noveltype, int? general_all_no);

        private async  Task<Section> ReadPageAsync(string url, string imageDirectory, CancellationToken ct)
        {
            var config = Configuration.Default.WithDefaultLoader();
            using var context = BrowsingContext.New(config);
            var doc = await context.OpenAsync(url, ct).ConfigureAwait(false);

            var chapterTitle = doc.QuerySelector(".chapter_title");

            var sectionTitle = doc.QuerySelector(".novel_subtitle");
            if (sectionTitle == null)
            {
                throw new EpubDocumentException("Can not find title of page");
            }

            var section = new Section(sectionTitle.InnerHtml);


            var main_text = doc.QuerySelector("#novel_honbun");
            if (main_text != null)
            {
                foreach (var item in main_text.Children)
                {
                    if (item is IHtmlParagraphElement)
                    {
                        if (item.ChildElementCount == 0)
                        {
                            if (!string.IsNullOrWhiteSpace(item.InnerHtml))
                            {
                                section.Elements.Add(new Paragraph() { Text = item.InnerHtml });
                            }
                        }
                        else if (item.ChildElementCount == 1)
                        {
                            if (item.Children[0] is IHtmlAnchorElement aElement)
                            {
                                if (aElement.ChildElementCount == 1)
                                {
                                    if (aElement.Children[0] is IHtmlImageElement img)
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

                                            }
                                        }
                                        else
                                        {
                                            throw new EpubDocumentException("Unexpected structure");
                                        }
                                    }
                                }
                                else
                                {
                                    throw new EpubDocumentException("Unexpected structure");
                                }
                            }
                            else
                            {

                            }
                        }
                        else
                        {
                            throw new EpubDocumentException("Unexpected structure");
                        }
                    }
                    else
                    {
                        throw new EpubDocumentException("Unexpected structure");
                    }
                }
            }
            else
            {
                throw new EpubDocumentException("There is no honbun.");
            }
            return section;
        }

        

        [System.Text.RegularExpressions.GeneratedRegex(@"https://.{5,7}.syosetu.com/(.{7}).?")]
        private static partial System.Text.RegularExpressions.Regex UrlToNcode();

        [System.Text.RegularExpressions.GeneratedRegex(@"http.{1,}/([^/]{0,}\.[^/]{1,})")]
        private static partial System.Text.RegularExpressions.Regex FileUrlToFileName();
    }
}
