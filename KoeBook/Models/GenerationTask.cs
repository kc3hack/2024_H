using CommunityToolkit.Mvvm.ComponentModel;
using FastEnumUtility;

namespace KoeBook.Models;

public partial class GenerationTask(Guid id, string source, SourceType sourceType) : ObservableObject
{
    public Guid Id { get; } = id;

    public string Source { get; } = source;

    public SourceType SourceType { get; } = sourceType;

    [ObservableProperty]
    private string _title = sourceType == SourceType.FilePath ? Path.GetFileName(source) : source;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProgressText))]
    private int _progress;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProgressText))]
    private int _maximumProgress;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StateText))]
    private GenerationState _state;

    public string StateText => State.GetEnumMemberValue()!;

    public string ProgressText => $"{Progress}/{MaximumProgress}";

    partial void OnMaximumProgressChanging(int value)
    {
        if (value < Progress)
            Progress = value;
    }
}
