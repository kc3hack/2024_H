using System.ComponentModel;
using KoeBook.Contracts.Services;
using KoeBook.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.UI.ViewManagement;

namespace KoeBook.Components;

public sealed partial class StateProgressBar : UserControl, INotifyPropertyChanged
{
    public static readonly DependencyProperty StateProperty = DependencyProperty.Register(
        nameof(State),
        typeof(GenerationState),
        typeof(StateProgressBar),
        new PropertyMetadata(GenerationState.Waiting));

    public GenerationState State
    {
        get => (GenerationState)GetValue(StateProperty);
        set
        {
            SetValue(StateProperty, value);
            OnPropertyChanged(nameof(BackgroundPosition));
            OnPropertyChanged(nameof(BackgroundWidth));
        }
    }

    public SourceType SourceType { get; set; }

    public Visibility GrayoutDownloading => SourceType == SourceType.FilePath ? Visibility.Visible : Visibility.Collapsed;

    private readonly IThemeSelectorService _themeSelectorService;

    public event PropertyChangedEventHandler? PropertyChanged;

    private static readonly UISettings _uiSettings = new();

    public StateProgressBar()
    {
        _themeSelectorService = App.GetService<IThemeSelectorService>();
        InitializeComponent();
    }

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new(propertyName));
    }

    public int BackgroundWidth(int position)
    {
        var state = State;
        var width = position switch
        {
            < 0 => (int)state,
            0 => 1,
            > 0 => 5 - (int)state,
        };
        return int.Max(1, width);
    }

    public int BackgroundPosition(int position)
    {
        var state = State;
        return position switch
        {
            < 0 => 0,
            0 => (int)state,
            > 0 => state == GenerationState.Completed ? 5 : 1 + (int)state,
        };
    }
}
