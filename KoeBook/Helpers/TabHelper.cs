using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace KoeBook.Helpers;

// Helper class to set the navigation target for a NavigationViewItem.
//
// Usage in XAML:
// <NavigationViewItem x:Uid="Shell_Main" Icon="Document" helpers:NavigationHelper.NavigateTo="AppName.ViewModels.MainViewModel" />
//
// Usage in code:
// NavigationHelper.SetNavigateTo(navigationViewItem, typeof(MainViewModel).FullName);
public class TabHelper
{
    public static Guid GetNavigateTo(TabViewItem item) => (Guid)item.GetValue(NavigateToProperty);

    public static void SetNavigateTo(TabViewItem item, Guid value) => item.SetValue(NavigateToProperty, value);

    public static readonly DependencyProperty NavigateToProperty =
        DependencyProperty.RegisterAttached("NavigateTo", typeof(Guid), typeof(TabHelper), new PropertyMetadata(default(Guid)));
}
