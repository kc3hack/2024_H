using System.Collections.ObjectModel;
using KoeBook.Contracts.Services;
using KoeBook.Models;

namespace KoeBook.Services;

public class ProcessingTaskService : IProcessingTaskService
{
    private readonly ObservableCollection<ProcessingTask> _tasks = [];

    public ObservableCollection<ProcessingTask> Tasks => _tasks;

    public event Action<ProcessingTask, ChangedEvents>? OnTasksChanged;

    public ProcessingTask GetProcessingTask(Guid processId)
    {
        ProcessingTask? task;
        lock (_tasks)
        {
            task = _tasks.FirstOrDefault(t => t.Id == processId);
            if (task is null)
                throw new ArgumentException($"Task not found: {processId}.");
        }
        return task;
    }

    public void Register(ProcessingTask task)
    {
        lock (_tasks)
        {
            var processId = task.Id;
            if (_tasks.Any(t => t.Id == processId))
                throw new ArgumentException($"The key {task.Id} is already registered in {nameof(ProcessingTaskService)}");

            _tasks.Insert(0, task);
        }

        OnTasksChanged?.Invoke(task, ChangedEvents.Registered);
    }

    public void Unregister(Guid processId)
    {
        ProcessingTask? task;
        lock (_tasks)
        {
            task = _tasks.FirstOrDefault(t => t.Id == processId);
            if (task is null)
                throw new ArgumentException($"The key {processId} is already unregistered in {nameof(ProcessingTaskService)}");

            _tasks.Remove(task);
        }

        OnTasksChanged?.Invoke(task, ChangedEvents.Unregistered);
    }
}
