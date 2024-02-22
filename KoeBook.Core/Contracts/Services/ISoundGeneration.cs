using KoeBook.Core.Models;

namespace KoeBook.Core.Contracts.Services;

internal interface ISoundGeneration
{
    /// <summary>
    /// 1文の音声を生成します
    /// </summary>
    /// <param name="scriptLine"></param>
    /// <param name="bookOptions"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    ValueTask<byte[]> GenerateLineSoundAsync(ScriptLine scriptLine, BookOptions bookOptions, CancellationToken cancellationToken);
}
