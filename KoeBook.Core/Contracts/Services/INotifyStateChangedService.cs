using KoeBook.Core.Models;

namespace KoeBook.Core.Contracts.Services;

public interface INotifyStateChangedService
{
    /// <summary>
    /// 状態が移行したことを通知します
    /// </summary>
    void OnStateChanged(BookProperties bookProperties, GenerationState state);
}
