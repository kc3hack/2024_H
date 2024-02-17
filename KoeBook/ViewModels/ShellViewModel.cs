using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KoeBook.Contracts.Services;
using KoeBook.Views;
using Microsoft.UI.Xaml;
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

    public ShellViewModel(INavigationService navigationService, INavigationViewService navigationViewService)
    {
        NavigationService = navigationService;
        NavigationService.Navigated += OnNavigated;
        NavigationViewService = navigationViewService;
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
    private void OnOpenSettingsTab(TabView tabView)
    {
        var newTab = tabView.TabItems
            .OfType<TabViewItem>()
            .FirstOrDefault(tab => tab.Header?.ToString() == "設定");

        if (newTab is null)
        {
            newTab = new TabViewItem()
            {
                IconSource = new SymbolIconSource()
                {
                    Symbol = Symbol.Setting
                },
                Header = "設定",
            };
            var frame = new Frame();
            newTab.Content = frame;
            frame.Navigate(typeof(SettingsPage));
            tabView.TabItems.Add(newTab);
        }
        tabView.SelectedItem = tabView;
    }
}
