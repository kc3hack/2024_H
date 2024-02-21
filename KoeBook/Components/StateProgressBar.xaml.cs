using System.ComponentModel;
using KoeBook.Contracts.Services;
using KoeBook.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.UI.ViewManagement;

namespace KoeBook.Components;

public sealed partial class StateProgressBar : UserControl, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

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
            if (State != value)
            {
                SetValue(StateProperty, value);
                OnPropertyChanged(nameof(BackgroundPosition));
                OnPropertyChanged(nameof(BackgroundWidth));
                OnPropertyChanged(nameof(ProccessingVisibility));
                OnPropertyChanged(nameof(FailedVisbility));
            }
        }
    }

    public SourceType SourceType { get; set; }

    public Visibility GrayoutDownloading => SourceType == SourceType.FilePath ? Visibility.Visible : Visibility.Collapsed;

    public Visibility ProccessingVisibility => State == GenerationState.Failed ? Visibility.Collapsed : Visibility.Visible;

    public Visibility FailedVisbility => State == GenerationState.Failed ? Visibility.Visible : Visibility.Collapsed;

    public StateProgressBar()
    {
        InitializeComponent();
    }

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new(propertyName));
    }

    public int BackgroundWidth(int position)
    {
        var state = State;
        if (state == GenerationState.Failed)
            return 5;

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
        if (state == GenerationState.Failed)
            return 0;

        return position switch
        {
            < 0 => 0,
            0 => (int)state,
            > 0 => state == GenerationState.Completed ? 5 : 1 + (int)state,
        };
    }
}
