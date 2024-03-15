using System.Net.Http.Headers;
using KoeBook.Epub.Contracts.Services;

namespace KoeBook.Epub.Services;

public sealed class ScrapingClientService : IScrapingClientService, IDisposable
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly PeriodicTimer _periodicTimer;
    private readonly Queue<Func<HttpClient, ValueTask>> _actionQueue = [];
    private bool _workerActivated;

    public ScrapingClientService(IHttpClientFactory httpClientFactory, TimeProvider timeProvider)
    {
        _httpClientFactory = httpClientFactory;
        _periodicTimer = new(TimeSpan.FromSeconds(10), timeProvider);

        Worker();
    }

    public void Dispose()
    {
        _periodicTimer.Dispose();
    }

    private async void Worker()
    {
        lock (_actionQueue)
        {
            _workerActivated = true;
        }

        while (await _periodicTimer.WaitForNextTickAsync().ConfigureAwait(false) && _actionQueue.Count > 0)
        {
            if (_actionQueue.TryDequeue(out var action))
            {
                await action(_httpClientFactory.CreateClient()).ConfigureAwait(false);
            }
        }

        lock (_actionQueue)
        {
            _workerActivated = false;
        }
    }

    public Task<string> GetAsStringAsync(string url, CancellationToken ct)
    {
        var taskCompletion = new TaskCompletionSource<string>();
        _actionQueue.Enqueue(async httpClient =>
        {
            if (ct.IsCancellationRequested)
                taskCompletion.SetCanceled(ct);

            try
            {
                var response = await httpClient.GetAsync(url, ct).ConfigureAwait(false);
                taskCompletion.SetResult(await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false));
            }
            catch (Exception ex)
            {
                taskCompletion.SetException(ex);
            }
        });

        lock (_actionQueue)
        {
            if (!_workerActivated)
                Worker();
        }

        return taskCompletion.Task;
    }

    public Task<ContentDispositionHeaderValue?> GetAsStreamAsync(string url, Stream destination, CancellationToken ct)
    {
        var taskCompletion = new TaskCompletionSource<ContentDispositionHeaderValue?>();
        _actionQueue.Enqueue(async httpClient =>
        {
            if (ct.IsCancellationRequested)
                taskCompletion.SetCanceled(ct);

            try
            {
                var response = await httpClient.GetAsync(url, ct).ConfigureAwait(false);
                await response.Content.CopyToAsync(destination, ct).ConfigureAwait(false);
                taskCompletion.SetResult(response.Content.Headers.ContentDisposition);
            }
            catch (Exception ex)
            {
                taskCompletion.SetException(ex);
            }
        });

        lock (_actionQueue)
        {
            if (!_workerActivated)
                Worker();
        }

        return taskCompletion.Task;
    }
}
