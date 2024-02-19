using KoeBook.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace KoeBook.Views;

public sealed partial class MainPage : Page
{
    public static readonly Guid Id = Guid.NewGuid();

    public MainViewModel ViewModel { get; }

    public MainPage()
    {
        ViewModel = App.GetService<MainViewModel>();
        InitializeComponent();
    } 
}
