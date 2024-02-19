using KoeBook.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace KoeBook.Views;

public sealed partial class MainPage : Page
{
    public static readonly Guid Id = Guid.NewGuid();

    public MainViewModel ViewModel { get; }

    public TaskListViewModel TaskListViewModel { get; }

    public MainPage()
    {
        try
        {

        ViewModel = App.GetService<MainViewModel>();
        TaskListViewModel = App.GetService<TaskListViewModel>();
        InitializeComponent();
        }
        catch (Exception e)
        {

            throw;
        }
    }
}
