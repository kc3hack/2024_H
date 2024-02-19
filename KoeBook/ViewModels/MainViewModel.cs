using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace KoeBook.ViewModels;

public partial class MainViewModel : ObservableRecipient
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EbookFileName))]
    private string? _ebookFilePath;

    public string EbookFileName => EbookFilePath is not null ? Path.GetFileName(EbookFilePath) : string.Empty;

    public MainViewModel()
    {
    }

    [RelayCommand]
    private async Task OpenEBookFileAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var picker = new FileOpenPicker();
        picker.FileTypeFilter.Add(".epub");
        var window = App.MainWindow;
        var hwnd = WindowNative.GetWindowHandle(window);
        InitializeWithWindow.Initialize(picker, hwnd);

        try
        {
            var file = await picker.PickSingleFileAsync().AsTask(cancellationToken);
            if (file is not null)
                EbookFilePath = file.Path;
        }
        catch (OperationCanceledException) { }
    }
}
