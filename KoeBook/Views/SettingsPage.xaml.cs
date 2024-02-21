using KoeBook.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace KoeBook.Views;

// TODO: Set the URL for your privacy policy by updating SettingsPage_PrivacyTermsLink.NavigateUri in Resources.resw.
public sealed partial class SettingsPage : Page
{
    public static readonly Guid Id = Guid.NewGuid();

    public SettingsViewModel ViewModel { get; }

    public SettingsPage()
    {
        ViewModel = App.GetService<SettingsViewModel>();
        InitializeComponent();
    }

    private async void Page_Loaded(object _, Microsoft.UI.Xaml.RoutedEventArgs __)
    {
        await ViewModel.OnLoaded();
    }
}
