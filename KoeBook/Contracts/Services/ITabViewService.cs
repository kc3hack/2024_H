using KoeBook.Models;
using Microsoft.UI.Xaml.Controls;

namespace KoeBook.Contracts.Services;

public interface ITabViewService
{
    void Initialize(TabView tabView);

    void UnregisterEvents();

    void Focus(TabViewItem? tabViewItem);

    TabViewItem? GetExistingTab(Guid id);

    TabViewItem? GetOrCreateTab(GenerationTask processingTask);

    TabViewItem? GetOrCreateSettings();
}
