using KoeBook.Epub.Contracts.Services;

namespace KoeBook.Epub.Services;

public sealed class ScrapingClientService(IHttpClientFactory httpClientFactory, TimeProvider timeProvider) : IScrapingClientService, IDisposable
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly PeriodicTimer _periodicTimer = new(TimeSpan.FromSeconds(10), timeProvider);

    public void Dispose()
    {
        _periodicTimer.Dispose();
    }

    public async ValueTask<HttpResponseMessage> GetAsync(string url, CancellationToken ct)
    {
        await _periodicTimer.WaitForNextTickAsync(ct).ConfigureAwait(false);

        var httpClient = _httpClientFactory.CreateClient();
        return await httpClient.GetAsync(url, ct).ConfigureAwait(false);
    }
}
