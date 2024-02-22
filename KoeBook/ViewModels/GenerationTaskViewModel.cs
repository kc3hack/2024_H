using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KoeBook.Contracts.Services;
using KoeBook.Models;
using KoeBook.Services;

namespace KoeBook.ViewModels;

public partial class GenerationTaskViewModel(GenerationTask task, ITabViewService tabViewService, GenerationTaskRunnerService runner) : ObservableObject
{
    public GenerationTask Task { get; } = task;

    private readonly ITabViewService _tabViewService = tabViewService;
    private readonly GenerationTaskRunnerService _runner = runner;

    [RelayCommand]
    private void OpenEditTab()
    {
        var newTab = _tabViewService.GetOrCreateTab(Task);
        _tabViewService.Focus(newTab);
    }

    [RelayCommand]
    private void StopTask()
    {
        _runner.StopTask(Task);
    }
}
