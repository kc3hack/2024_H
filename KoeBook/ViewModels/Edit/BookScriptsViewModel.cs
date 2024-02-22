using CommunityToolkit.Mvvm.ComponentModel;
using KoeBook.Core.Models;

namespace KoeBook.ViewModels.Edit;

public partial class BookScriptsViewModel(BookScripts bookScripts, IReadOnlyList<string> allowedModels) : ObservableObject
{
    private readonly BookScripts _bookScripts = bookScripts;

    public IReadOnlyList<CharacterModelPairViewModel> CharacterMapping { get; }
        = bookScripts.Options.CharacterMapping
            .Select(kvp => new CharacterModelPairViewModel(kvp.Key, kvp.Value, allowedModels))
            .ToArray();

    public void Apply()
    {
        _bookScripts.Options.CharacterMapping = CharacterMapping.ToDictionary(pair => pair.Character, pair => pair.Model);
    }
}
