using System.Collections.ObjectModel;
using KoeBook.Models;

namespace KoeBook.Contracts.Services;

public interface IGenerationTaskService
{
    ObservableCollection<GenerationTask> Tasks { get; }

    event Action<GenerationTask, ChangedEvents>? OnTasksChanged;

    GenerationTask GetProcessingTask(Guid id);

    void Register(GenerationTask task);

    void Unregister(Guid id);
}

public enum ChangedEvents
{
    Registered,
    Unregistered,
}
