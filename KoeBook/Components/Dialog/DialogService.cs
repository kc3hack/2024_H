using KoeBook.Contracts.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace KoeBook.Components.Dialog;

public class DialogService : IDialogService
{
    private static Style? DefaultDialogStyle => Application.Current.Resources["DefaultContentDialogStyle"] as Style;

    public Task<ContentDialogResult> ShowAsync(
        string title,
        object content,
        string primaryText,
        string closeText,
        ContentDialogButton defaultButton,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var contentDialog = new ContentDialog()
        {
            XamlRoot = App.MainWindow.Content.XamlRoot,
            Title = title,
            Content = content,
            Style = DefaultDialogStyle,
            PrimaryButtonText = primaryText,
            CloseButtonText = closeText,
            DefaultButton = defaultButton,
        };

        return contentDialog.ShowAsync().AsTask(cancellationToken);
    }

    public Task<ContentDialogResult> ShowAsync(
        string title,
        string content,
        string primaryText,
        string closeText,
        ContentDialogButton defaultButton,
        CancellationToken cancellationToken)
    {
        return ShowAsync(title, new DialogContentControl(content), primaryText, closeText, defaultButton, cancellationToken);
    }
}
