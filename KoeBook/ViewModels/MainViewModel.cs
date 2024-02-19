using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KoeBook.Contracts.Services;
using KoeBook.Models;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace KoeBook.ViewModels;

public sealed partial class MainViewModel : ObservableRecipient
{
    private readonly IProcessingTaskService _taskService;

    private readonly IDialogService _dialogService;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EbookFileName))]
    [NotifyCanExecuteChangedFor(nameof(StartProcessCommand))]
    private string? _ebookFilePath;

    public string EbookFileName => EbookFilePath is not null ? Path.GetFileName(EbookFilePath) : string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartProcessCommand))]
    private string? _ebookUrl;

    private bool CanExecuteStartProcess => EbookFilePath is not null || !string.IsNullOrEmpty(EbookUrl);

    public MainViewModel(IProcessingTaskService taskService, IDialogService dialogService)
    {
        _taskService = taskService;
        _dialogService = dialogService;
    }

    [RelayCommand]
    private async Task OpenEBookFileAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            if (!string.IsNullOrEmpty(EbookUrl))
            {
                var result = await _dialogService.ShowAsync("ローカルファイルを使用する場合、外部URLは無視されます。", cancellationToken);
                if (result == ContentDialogResult.Primary)
                    EbookUrl = null;
                else
                {
                    EbookFilePath = null;
                    return;
                }
            }

            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".epub");
            var window = App.MainWindow;
            var hwnd = WindowNative.GetWindowHandle(window);
            InitializeWithWindow.Initialize(picker, hwnd);

            var file = await picker.PickSingleFileAsync().AsTask(cancellationToken);
            if (file is not null)
                EbookFilePath = file.Path;
        }
        catch (OperationCanceledException)
        {
            return;
        }
    }

    [RelayCommand(CanExecute = nameof(CanExecuteStartProcess))]
    private async Task StartProcessAsync(CancellationToken cancellationToken)
    {
        try
        {
            var result = await _dialogService.ShowAsync(
                "EPubの生成", "読み上げの生成を開始しますか？この先の処理ではAPI料金がかかります。",
                "開始", cancellationToken);

            if (result != ContentDialogResult.Primary)
                return;
        }
        catch (OperationCanceledException)
        {
            return;
        }

        var source = EbookFilePath ?? EbookUrl!;
        EbookFilePath = null;
        EbookUrl = null;
        _taskService.Register(new(Guid.NewGuid(), source, SourceType.FilePath));
    }

    public async void BeforeTextChanging(TextBox _, TextBoxBeforeTextChangingEventArgs args)
    {
        if (EbookFilePath is null || string.IsNullOrEmpty(args.NewText))
            return;

        var result = await _dialogService.ShowAsync("外部URLから使用する場合、ローカルファイルは無視されます。", default);
        if (result == ContentDialogResult.Primary)
        {
            EbookFilePath = null;
        }
        else
        {
            args.Cancel = true;
            EbookUrl = string.Empty;
        }
    }
}
