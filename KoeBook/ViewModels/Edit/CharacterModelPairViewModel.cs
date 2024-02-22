using CommunityToolkit.Mvvm.ComponentModel;

namespace KoeBook.ViewModels.Edit;

public partial class CharacterModelPairViewModel(string character, string model, IReadOnlyList<string> allowedModels) : ObservableObject
{
    public string Character { get; } = character;

    [ObservableProperty]
    private string _model = model;

    public IReadOnlyList<string> AllowedModels { get; } = allowedModels;
}
