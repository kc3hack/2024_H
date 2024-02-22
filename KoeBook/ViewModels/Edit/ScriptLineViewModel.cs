using CommunityToolkit.Mvvm.ComponentModel;
using KoeBook.Core.Models;

namespace KoeBook.ViewModels.Edit;

public partial class ScriptLineViewModel : ObservableObject
{
    private readonly ScriptLine _scriptLine;

    public string Character => _scriptLine.Character;

    public string Text => _scriptLine.Text;

    [ObservableProperty]
    private string _style;

    [ObservableProperty]
    private IReadOnlyList<string> _allowedStyles = [];

    [ObservableProperty]
    private bool _editable;

    public ScriptLineViewModel(ScriptLine scriptLine, bool editable, IReadOnlyList<string> allowedStyles)
    {
        _scriptLine = scriptLine;
        _style = scriptLine.Style;
        _editable = editable;
        AllowedStyles = allowedStyles;
    }

    partial void OnAllowedStylesChanged(IReadOnlyList<string> value)
    {
        // AllowedStylesが変更されてからStyleを変更する
        App.MainWindow.DispatcherQueue.TryEnqueue(() =>
        {
            if (!value.Contains(Style))
                Style = value[0];
        });
    }

    public void Apply()
    {
        _scriptLine.Style = Style;
    }
}
