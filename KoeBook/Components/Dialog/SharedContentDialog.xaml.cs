using Microsoft.UI.Xaml.Controls;

namespace KoeBook.Components.Dialog;

public sealed partial class SharedContentDialog : ContentDialog
{
    public SharedContentDialog(string title, string primaryText, string closeText, ContentDialogButton defaultButton)
    {
        Title = title;
        PrimaryButtonText = primaryText;
        CloseButtonText = closeText;
        DefaultButton = defaultButton;
        InitializeComponent();
    }
}
