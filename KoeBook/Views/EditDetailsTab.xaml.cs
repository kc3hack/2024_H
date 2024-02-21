using KoeBook.Helpers;
using KoeBook.Models;
using KoeBook.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace KoeBook.Views;

public sealed partial class EditDetailsTab : TabViewItem
{
    public EditDetailsViewModel ViewModel { get; }

    public EditDetailsTab(GenerationTask task)
    {
        ViewModel = App.GetService<EditDetailsViewModel>();
        ViewModel.Initialize(task);
        InitializeComponent();

        TabHelper.SetNavigateTo(this, task.Id);
    }
}
