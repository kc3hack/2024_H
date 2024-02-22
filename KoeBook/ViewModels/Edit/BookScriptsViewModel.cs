using CommunityToolkit.Mvvm.ComponentModel;
using KoeBook.Core.Models;

namespace KoeBook.ViewModels.Edit;

public partial class BookScriptsViewModel : ObservableObject
{
    private readonly BookScripts _bookScripts;

    public IReadOnlyList<CharacterModelPairViewModel> CharacterMapping { get; }

    public IReadOnlyList<ScriptLineViewModel> ScriptLines { get; }

    public BookScriptsViewModel(BookScripts bookScripts, bool editable, IReadOnlyList<SoundModel> allowedModels)
    {
        _bookScripts = bookScripts;
        CharacterMapping = bookScripts.Options.CharacterMapping
            .Select(kvp => new CharacterModelPairViewModel(
                kvp.Key,
                kvp.Value,
                editable,
                allowedModels.Select(model => model.Name).ToArray()))
            .ToArray();
        var characterMapping = bookScripts.Options.CharacterMapping;
        ScriptLines = bookScripts.ScriptLines.Select(line =>
        {
            var model = characterMapping[line.Character];
            return new ScriptLineViewModel(line, editable, allowedModels.First(m => m.Name == model).Styles);
        }).ToArray();

        foreach (var pair in CharacterMapping)
        {
            var targets = ScriptLines.Where(line => line.Character == pair.Character).ToArray();
            pair.PropertyChanged += (sender, e) =>
            {
                var pair = (CharacterModelPairViewModel)sender!;
                foreach (var line in targets)
                {
                    line.AllowedStyles = allowedModels.First(m => m.Name == pair.Model).Styles;
                }
            };
        }
    }

    public void OnCharacterMapping()
    {
        OnPropertyChanged(nameof(CharacterMapping));
    }

    public void Apply()
    {
        _bookScripts.Options.CharacterMapping = CharacterMapping.ToDictionary(pair => pair.Character, pair => pair.Model);
        foreach (var line in ScriptLines)
            line.Apply();
    }
}
