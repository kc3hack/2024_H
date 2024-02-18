using KoeBook.Models;

namespace KoeBook.Contracts.Services;

public interface IProcessingTaskService
{
    event Action<ProcessingTask>? OnRegistered;

    event Action<ProcessingTask>? OnUnregistered;

    ProcessingTask GetProcessingTask(Guid processId);

    void Register(ProcessingTask task);

    void Unregister(Guid processId);
}
