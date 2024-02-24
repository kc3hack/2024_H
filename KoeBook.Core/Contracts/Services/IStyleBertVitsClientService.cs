namespace KoeBook.Core.Contracts.Services;

public interface IStyleBertVitsClientService
{
    ValueTask<T> GetFromJsonAsync<T>(string path, ExceptionType exceptionType, CancellationToken cancellationToken);

    ValueTask<byte[]> GetAsByteArrayAsync(string path, ExceptionType exceptionType, CancellationToken cancellationToken);
}
