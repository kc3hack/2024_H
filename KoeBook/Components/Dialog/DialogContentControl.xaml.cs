using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;

namespace KoeBook.Components.Dialog;

public sealed partial class DialogContentControl : UserControl
{
    public string Description { get; }
    public DialogContentControl(string description)
    {
        Description = description;

        InitializeComponent();
    }
}
