using CommunityToolkit.Mvvm.ComponentModel;
using FastEnumUtility;

namespace KoeBook.Models;

public partial class ProcessingTask(Guid id, string source, SourceType sourceType) : ObservableObject
{
    public Guid Id { get; } = id;

    public string Source { get; } = source;

    public SourceType SourceType { get; } = sourceType;

    [ObservableProperty]
    private string _title = sourceType == SourceType.FilePath ? Path.GetFileName(source) : source;

    /// <summary>
    /// 進捗 (0~255)
    /// </summary>
    [ObservableProperty]
    private byte _progress;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StateText))]
    private ProcessingState _state;

    public string StateText => State.GetEnumMemberValue()!;
}
