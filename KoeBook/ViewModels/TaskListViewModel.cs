using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using KoeBook.Contracts.Services;
using KoeBook.Models;
using Microsoft.UI.Xaml;

namespace KoeBook.ViewModels;

public sealed partial class TaskListViewModel : ObservableObject
{
    public ObservableCollection<GenerationTask> GenerationTasks { get; }

    public Visibility ListVisibility => GenerationTasks.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

    public Visibility InfoVisibility => GenerationTasks.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

    public TaskListViewModel(IGenerationTaskService taskService)
    {
        GenerationTasks = taskService.Tasks;
        GenerationTasks.CollectionChanged += TaskCollectionChanged;
    }

    private void TaskCollectionChanged(object? sender,  NotifyCollectionChangedEventArgs e)
    {
        if(e.Action is NotifyCollectionChangedAction.Add or NotifyCollectionChangedAction.Remove or NotifyCollectionChangedAction.Reset)
        {
            OnPropertyChanged(nameof(ListVisibility));
            OnPropertyChanged(nameof(InfoVisibility));
        }
    }
}
