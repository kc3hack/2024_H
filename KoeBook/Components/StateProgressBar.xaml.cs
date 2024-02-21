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
                OnPropertyChanged(nameof(BackgroundWidth));
            }
        }
    }

    public SourceType SourceType { get; set; }

    public Visibility DownloadingVisibility => SourceType != SourceType.FilePath ? Visibility.Visible : Visibility.Collapsed;

    public Visibility FailedVisbility => State == GenerationState.Failed ? Visibility.Visible : Visibility.Collapsed;

    public StateProgressBar()
    {
        InitializeComponent();
    }

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new(propertyName));
    }

    public double BackgroundWidth(int position)
    {
        const int ItemWidth = 80;
        var state = State;
        if (state == GenerationState.Failed)
            return position <= 0 ? 0 : SourceType == SourceType.FilePath ? ItemWidth * 5 : ItemWidth * 6;

        if (SourceType != SourceType.FilePath)
        {
            return position switch
            {
                < 0 => (int)state * ItemWidth,
                0 => ItemWidth,
                > 0 => (5 - (int)state) * ItemWidth,
            };
        }
        else
        {
            if (state > GenerationState.Downloading)
                state -= 1;

            return position switch
            {
                < 0 => (int)state * ItemWidth,
                0 => ItemWidth,
                > 0 => (4 - (int)state) * ItemWidth,
            };
        }
    }
}
