﻿using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using KoeBook.Contracts.Services;
using KoeBook.Models;
using KoeBook.Services;
using Microsoft.UI.Xaml;

namespace KoeBook.ViewModels;

public sealed partial class TaskListViewModel : ObservableObject
{
    private readonly ITabViewService _tabViewService;
    private readonly GenerationTaskRunnerService _generationTaskRunnerService;

    public ObservableCollection<GenerationTaskViewModel> GenerationTasks { get; }

    public Visibility ListVisibility => GenerationTasks.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

    public Visibility InfoVisibility => GenerationTasks.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

    public TaskListViewModel(IGenerationTaskService taskService, ITabViewService tabViewService, GenerationTaskRunnerService runnerService)
    {
        GenerationTasks = new(taskService.Tasks
            .Select(t =>
            {
                var viewModel = App.GetService<GenerationTaskViewModel>();
                viewModel.Task = t;
                return viewModel;
            }));
        GenerationTasks.CollectionChanged += TaskCollectionChanged;
        taskService.OnTasksChanged += OnTasksChanged;
        _tabViewService = tabViewService;
        _generationTaskRunnerService = runnerService;
    }

    private void OnTasksChanged(GenerationTask task, ChangedEvents action)
    {
        switch (action)
        {
            case ChangedEvents.Registered:
                {
                    var viewModel = App.GetService<GenerationTaskViewModel>();
                    viewModel.Task = task;
                    GenerationTasks.Insert(0, viewModel);
                }
                break;
            case ChangedEvents.Unregistered:
                {
                    var id = task.Id;
                    var taskViewModel = GenerationTasks.FirstOrDefault(tvm => tvm.Task?.Id == id);
                    if (taskViewModel is not null)
                        GenerationTasks.Remove(taskViewModel);
                    break;
                }
        }
    }

    private void TaskCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action is NotifyCollectionChangedAction.Add or NotifyCollectionChangedAction.Remove or NotifyCollectionChangedAction.Reset)
        {
            OnPropertyChanged(nameof(ListVisibility));
            OnPropertyChanged(nameof(InfoVisibility));
        }
    }
}
