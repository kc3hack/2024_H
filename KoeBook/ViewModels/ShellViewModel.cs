using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KoeBook.Contracts.Services;

namespace KoeBook.ViewModels;

public partial class ShellViewModel : ObservableRecipient
{
    public ITabViewService TabViewService { get; }

    public ShellViewModel(ITabViewService tabViewService)
    {
        TabViewService = tabViewService;
    }

    [RelayCommand]
    private void OnOpenSettingsTab()
    {
        TabViewService.Focus(TabViewService.GetOrCreateSettings());
    }
}
