using KoeBook.Contracts.Services;
using KoeBook.Core.Contracts.Services;
using KoeBook.Core.Models;

namespace KoeBook.Services;

internal class NotifyStateChangedService(IGenerationTaskService taskService) : INotifyStateChangedService
{
    private readonly IGenerationTaskService _taskService = taskService;

    public void OnStateChanged(BookProperties bookProperties, GenerationState state)
    {
        var taskService = _taskService; // thisをキャプチャしないようにする
        _ = App.MainWindow.DispatcherQueue.TryEnqueue(() =>
        {
            taskService.GetProcessingTask(bookProperties.Id).State = state;
        });
    }
}
