using KoeBook.Contracts.Services;
using KoeBook.Models;

namespace KoeBook.Services;

public class GenerationTaskService : IGenerationTaskService
{
    private readonly List<GenerationTask> _tasks = [
        new(Guid.NewGuid(), "book.epub", SourceType.FilePath){
         Progress = 30,
         MaximumProgress = 100,
          State = GenerationState.Analyzing,
        }];

    public IReadOnlyList<GenerationTask> Tasks => _tasks;

    public event Action<GenerationTask, ChangedEvents>? OnTasksChanged;

    public GenerationTask GetProcessingTask(Guid id)
    {
        GenerationTask? task;
        lock (_tasks)
        {
            task = _tasks.FirstOrDefault(t => t.Id == id);
            if (task is null)
                throw new ArgumentException($"Task not found: {id}.");
        }
        return task;
    }

    public void Register(GenerationTask task)
    {
        lock (_tasks)
        {
            var id = task.Id;
            if (_tasks.Any(t => t.Id == id))
                throw new ArgumentException($"The key {task.Id} is already registered in {nameof(GenerationTaskService)}");

            _tasks.Add(task);
        }

        OnTasksChanged?.Invoke(task, ChangedEvents.Registered);
    }

    public void Unregister(Guid id)
    {
        GenerationTask? task;
        lock (_tasks)
        {
            task = _tasks.FirstOrDefault(t => t.Id == id);
            if (task is null)
                throw new ArgumentException($"The key {id} is already unregistered in {nameof(GenerationTaskService)}");

            _tasks.Remove(task);
        }

        OnTasksChanged?.Invoke(task, ChangedEvents.Unregistered);
    }
}
