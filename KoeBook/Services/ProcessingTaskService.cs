using KoeBook.Contracts.Services;
using KoeBook.Models;

namespace KoeBook.Services;

public class ProcessingTaskService : IProcessingTaskService
{
    private readonly Dictionary<Guid, ProcessingTask> _tasks = [];

    public event Action<ProcessingTask>? OnRegistered;

    public event Action<ProcessingTask>? OnUnregistered;

    public ProcessingTask GetProcessingTask(Guid processId)
    {
        ProcessingTask? task;
        lock (_tasks)
        {
            if (!_tasks.TryGetValue(processId, out task))
                throw new ArgumentException($"Task not found: {processId}.");
        }
        return task;
    }

    public void Register(ProcessingTask task)
    {
        lock (_tasks)
        {
            if (_tasks.ContainsKey(task.Id))
                throw new ArgumentException($"The key {task.Id} is already registered in {nameof(ProcessingTaskService)}");

            _tasks.Add(task.Id, task);
        }

        OnRegistered?.Invoke(task);
    }

    public void Unregister(Guid processId)
    {
        ProcessingTask processingTask;
        lock (_tasks)
        {
            if (!_tasks.ContainsKey(processId))
                throw new ArgumentException($"The key {processId} is already unregistered in {nameof(ProcessingTaskService)}");

            processingTask = _tasks[processId];
            _tasks.Remove(processId);
        }

        OnUnregistered?.Invoke(processingTask);
    }
}
