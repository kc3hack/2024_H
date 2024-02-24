using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KoeBook.Contracts.Services;
using KoeBook.Models;
using KoeBook.Services;
using Microsoft.UI.Xaml.Controls;

namespace KoeBook.ViewModels;

public partial class GenerationTaskViewModel(ITabViewService tabViewService, GenerationTaskRunnerService runner, IDialogService dialogService) : ObservableObject
{
    public GenerationTask? Task { get; set; }

    private readonly ITabViewService _tabViewService = tabViewService;
    private readonly GenerationTaskRunnerService _runner = runner;
    private readonly IDialogService _dialogService = dialogService;

    [RelayCommand]
    private void OpenEditTab()
    {
        if (Task is not null)
        {
            var newTab = _tabViewService.GetOrCreateTab(Task);
            _tabViewService.Focus(newTab);
        }
    }

    [RelayCommand]
    private async Task StopTask(CancellationToken cancellationToken)
    {
        if (Task is null)
            return;

        var result = await _dialogService.ShowAsync("KoeBookからのお知らせ", "実行中のタスクをキャンセルし、削除します。この操作は戻せません。", "削除", cancellationToken);
        if (result != ContentDialogResult.Primary)
            return;
        _runner.StopTask(Task);
    }
}
