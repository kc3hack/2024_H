using CommunityToolkit.Mvvm.ComponentModel;

namespace KoeBook.Common;
internal partial class ScriptLine : ObservableObject
{
    [ObservableProperty] private string _text = string.Empty;
    [ObservableProperty] private string _character = string.Empty;
    [ObservableProperty] private string _style = string.Empty;

    public string SpeakingModel => Characters.Mapping[Character];
}
