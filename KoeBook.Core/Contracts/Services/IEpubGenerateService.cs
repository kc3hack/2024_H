using KoeBook.Core.Models;

namespace KoeBook.Core.Contracts.Services;

public interface IEpubGenerateService
{
    /// <summary>
    /// 読み上げ音声を生成し、Epubを作成します。
    /// </summary>
    /// <returns>生成したEpubのパス</returns>
    ValueTask<string> GenerateEpubAsync(BookScripts bookScripts, string tempDirectory, CancellationToken cancellationToken);
}
