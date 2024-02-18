using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KoeBook.Contracts.Services;
using KoeBook.Views;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace KoeBook.ViewModels;

public partial class ShellViewModel : ObservableRecipient
{
    [ObservableProperty]
    private bool isBackEnabled;

    [ObservableProperty]
    private object? _selected;

    public INavigationService NavigationService { get; }

    public INavigationViewService NavigationViewService { get; }

    public   ITabViewService TabViewService { get; }

    public ShellViewModel(INavigationService navigationService, INavigationViewService navigationViewService, ITabViewService tabViewService)
    {
        NavigationService = navigationService;
        NavigationService.Navigated += OnNavigated;
        NavigationViewService = navigationViewService;
        TabViewService = tabViewService;
    }

    private void OnNavigated(object sender, NavigationEventArgs e)
    {
        IsBackEnabled = NavigationService.CanGoBack;

        if (e.SourcePageType == typeof(SettingsPage))
        {
            Selected = NavigationViewService.SettingsItem;
            return;
        }

        var selectedItem = NavigationViewService.GetSelectedItem(e.SourcePageType);
        if (selectedItem != null)
        {
            Selected = selectedItem;
        }
    }

    [RelayCommand]
    private void OnOpenSettingsTab( )
    {
        TabViewService.Focus(TabViewService.GetOrCreateSettings());
    }
}
