using System.Collections.ObjectModel;
using KoeBook.Contracts.Services;
using KoeBook.Models;

namespace KoeBook.ViewModels;

public sealed partial class TaskListViewModel
{
    public ObservableCollection<ProcessingTask> ProcessingTasks { get; }

    public TaskListViewModel(IProcessingTaskService taskService)
    {
        ProcessingTasks = taskService.Tasks;
    }
}
