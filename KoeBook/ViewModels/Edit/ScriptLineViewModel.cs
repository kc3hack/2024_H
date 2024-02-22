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

    public ScriptLineViewModel(ScriptLine scriptLine, IReadOnlyList<string> allowedStyles)
    {
        _scriptLine = scriptLine;
        _style = scriptLine.Style;
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
