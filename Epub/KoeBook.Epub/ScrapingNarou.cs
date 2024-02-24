using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Io;
using System.IO;
using System.Net.Http.Json;
using static KoeBook.Epub.ScrapingHelper;

namespace KoeBook.Epub
{
    public partial class ScrapingNarouService
    {
        public ScrapingNarouService(IHttpClientFactory httpClientFactory)
        {
            _httpCliantFactory = httpClientFactory;
        }

        private readonly IHttpClientFactory _httpCliantFactory;

        public async Task<int> ScrapingAsync(string url, string coverFilePath, string imageDirectory, Guid id, CancellationToken ct)
        {
            var config = Configuration.Default.WithDefaultLoader();
            using var context = BrowsingContext.New(config);
            var doc = await context.OpenAsync(url, ct).ConfigureAwait(false);

            // title の取得
            //var bookTitle = doc.QuerySelector("novel_title");
            //if (bookTitle is null)
            //{
            //    throw new EpubDocumentException($"Failed to get title properly.\nYou may be able to get proper URL at {(url)}");
            //}

            // auther の取得
            var bookAuther = doc.QuerySelector(".novel_writername a");
            if (bookAuther is null)
            {
                throw new EpubDocumentException($"Failed to get auther properly.\nYou may be able to get proper URL at {url}");
            }



            var result = await _httpCliantFactory.CreateClient().GetAsync("https://api.syosetu.com/novelapi/api/?of=ga-nt&ncode=n9669bk&out=json", ct).ConfigureAwait(false);
            var test = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (result.IsSuccessStatusCode)
            {
                var content = await result.Content.ReadFromJsonAsync<BookInfo[]>(ct).ConfigureAwait(false);
                if (true)
                {

                }
            }
            else
            {
                throw new EpubDocumentException("Url may be not Correct");
            }

            return 1;

        }

        public record BookInfo(string allallcount, string noveltype, string general_all_no);

    }
}
