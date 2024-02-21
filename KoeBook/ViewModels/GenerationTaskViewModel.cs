using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KoeBook.Contracts.Services;
using KoeBook.Models;

namespace KoeBook.ViewModels;

public partial class GenerationTaskViewModel(GenerationTask task, ITabViewService tabViewService) : ObservableObject
{
    public GenerationTask Task { get; } = task;

    private readonly ITabViewService _tabViewService = tabViewService;

    [RelayCommand]
    private void OpenEditTab()
    {
        var newTab = _tabViewService.GetOrCreateTab(Task);
        _tabViewService.Focus(newTab);
    }
}
