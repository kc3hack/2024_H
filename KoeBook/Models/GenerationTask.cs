using CommunityToolkit.Mvvm.ComponentModel;
using FastEnumUtility;
using KoeBook.Core.Models;

namespace KoeBook.Models;

public partial class GenerationTask(Guid id, string source, SourceType sourceType, bool skipEdit) : ObservableObject
{
    public Guid Id { get; } = id;

    public CancellationTokenSource CancellationTokenSource { get; } = new();

    public CancellationToken CancellationToken => CancellationTokenSource.Token;

    public string Source { get; } = source;

    public SourceType SourceType { get; } = sourceType;

    public string SourceDescription => SourceType switch
    {
        SourceType.Url => "URL",
        SourceType.FilePath => "ファイルパス",
        _ => string.Empty,
    };

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
    [NotifyPropertyChangedFor(nameof(SkipEditChangable))]
    private GenerationState _state;

    public string StateText => State.GetEnumMemberValue()!;

    public string ProgressText => $"{Progress}/{MaximumProgress}";

    public bool SkipEdit
    {
        get => _skipEdit;
        set
        {
            if (_skipEdit != value && SkipEditChangable)
            {
                OnPropertyChanging(nameof(SkipEdit));
                _skipEdit = value;
                OnPropertyChanged(nameof(SkipEdit));
            }
        }
    }
    private bool _skipEdit = skipEdit;

    public bool SkipEditChangable => State < GenerationState.Editting;

    [ObservableProperty]
    private BookScripts? _bookScripts;

    partial void OnMaximumProgressChanging(int value)
    {
        if (value < Progress)
            Progress = value;
    }
}
