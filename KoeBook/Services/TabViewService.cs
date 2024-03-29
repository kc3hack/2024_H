﻿using System.Diagnostics.CodeAnalysis;
using KoeBook.Contracts.Services;
using KoeBook.Helpers;
using KoeBook.Models;
using KoeBook.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace KoeBook.Services;

public sealed class TabViewService : ITabViewService, IDisposable
{
    private readonly IGenerationTaskService _taskService;

    private TabView? _tabView;

    private static Thickness TabContentThickness => new(20, 10, 20, 10);

    private bool _disposed;

    public TabViewService(IGenerationTaskService taskService)
    {
        _taskService = taskService;
        _taskService.OnTasksChanged += OnTasksChanged;
    }

    private void OnTasksChanged(GenerationTask task, ChangedEvents e)
    {
        if (e == ChangedEvents.Unregistered)
        {
            var tab = GetExistingTab(task.Id);
            if (tab is not null)
            {
                _tabView!.TabItems.Remove(tab);
            }
        }
    }

    [MemberNotNull(nameof(_tabView))]
    public void Initialize(TabView tabView)
    {
        _tabView = tabView;
        _tabView.TabCloseRequested += TabCloseRequested;
    }

    public void UnregisterEvents()
    {
        if (_tabView is not null)
        {
            _tabView.TabCloseRequested -= TabCloseRequested;
        }
    }

    public void Focus(TabViewItem? tabViewItem)
    {
        if (_tabView is null || tabViewItem is null)
            return;

        _tabView.SelectedItem = tabViewItem;
    }

    public TabViewItem? GetExistingTab(Guid id)
    {
        if (_tabView is null)
            return null;

        foreach (var item in _tabView.TabItems.OfType<TabViewItem>())
        {
            if (IsTabViewItemForId(item, id))
                return item;
        }

        return null;
    }

    public TabViewItem? GetOrCreateTab(GenerationTask task)
    {
        if (_tabView is null)
            return null;

        var tab = GetExistingTab(task.Id);
        if (tab is null)
        {
            tab = new EditDetailsTab(task);
            _tabView.TabItems.Add(tab);
        }

        return tab;
    }

    public TabViewItem? GetOrCreateSettings()
    {
        if (_tabView is null)
            return null;

        var tab = GetExistingTab(SettingsPage.Id);
        if (tab is null)
        {
            var frame = new Frame
            {
                Margin = TabContentThickness,
            };
            tab = new TabViewItem()
            {
                Content = frame,
                Header = "設定",
                IconSource = new SymbolIconSource()
                {
                    Symbol = Symbol.Setting,
                },
            };
            frame.Navigate(typeof(SettingsPage));
            TabHelper.SetNavigateTo(tab, SettingsPage.Id);
            _tabView.TabItems.Add(tab);
        }

        return tab;
    }

    private void TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
    {
        sender.TabItems.Remove(args.Tab);
    }

    private static bool IsTabViewItemForId(TabViewItem tab, Guid id)
    {
        if (tab.GetValue(TabHelper.NavigateToProperty) is Guid tabId)
            return tabId == id;

        return false;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _taskService.OnTasksChanged -= OnTasksChanged;
            _disposed = true;
        }
    }
}
