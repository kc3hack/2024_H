using System.Net.Http.Json;
using KoeBook.Core.Contracts.Services;

namespace KoeBook.Core.Services;

public class StyleBertVitsClientService(IHttpClientFactory httpClientFactory) : IStyleBertVitsClientService
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    public async ValueTask<T> GetFromJsonAsync<T>(string path, ExceptionType exceptionType, CancellationToken cancellationToken)
    {
        var content = await GetAsync(path, exceptionType, cancellationToken).ConfigureAwait(false);
        return await content.ReadFromJsonAsync<T>(cancellationToken).ConfigureAwait(false)
            ?? throw new EbookException(exceptionType);
    }

    public async ValueTask<byte[]> GetAsByteArrayAsync(string path, ExceptionType exceptionType, CancellationToken cancellationToken)
    {
        var content = await GetAsync(path, exceptionType, cancellationToken).ConfigureAwait(false);
        return await content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
    }

    private async ValueTask<HttpContent> GetAsync(string path, ExceptionType exceptionType, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClientFactory.CreateClient()
                .GetAsync($"http://127.0.0.1:5000{path}", cancellationToken)
                .ConfigureAwait(false) ?? throw new EbookException(exceptionType);

            if (!response.IsSuccessStatusCode)
                throw new EbookException(exceptionType);
            return response.Content;
        }
        catch (OperationCanceledException) { throw; }
        catch (EbookException) { throw; }
        catch (Exception e)
        {
            throw new EbookException(exceptionType, innerException: e);
        }
    }
}
