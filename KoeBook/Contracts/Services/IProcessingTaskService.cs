using System.Collections.ObjectModel;
using KoeBook.Models;

namespace KoeBook.Contracts.Services;

public interface IProcessingTaskService
{
    ObservableCollection<ProcessingTask> Tasks { get; }

    event Action<ProcessingTask, ChangedEvents>? OnTasksChanged;

    ProcessingTask GetProcessingTask(Guid processId);

    void Register(ProcessingTask task);

    void Unregister(Guid processId);
}

public enum ChangedEvents
{
    Registered,
    Unregistered,
}
