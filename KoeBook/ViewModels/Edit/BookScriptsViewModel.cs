using CommunityToolkit.Mvvm.ComponentModel;
using KoeBook.Core.Models;

namespace KoeBook.ViewModels.Edit;

public partial class BookScriptsViewModel : ObservableObject
{
    private readonly BookScripts _bookScripts;

    public IReadOnlyList<CharacterModelPairViewModel> CharacterMapping { get; }

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
    }

    public void OnCharacterMapping()
    {
        OnPropertyChanged(nameof(CharacterMapping));
    }

    public void Apply()
    {
        _bookScripts.Options.CharacterMapping = CharacterMapping.ToDictionary(pair => pair.Character, pair => pair.Model);
    }
}
