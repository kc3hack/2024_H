using CommunityToolkit.Mvvm.ComponentModel;

namespace KoeBook.ViewModels.Edit;

public partial class CharacterModelPairViewModel(string character, string model, bool editable, IReadOnlyList<string> allowedModels) : ObservableObject
{
    public string Character { get; } = character;

    [ObservableProperty]
    private string _model = model;

    [ObservableProperty]
    private bool _editable = editable;

    public IReadOnlyList<string> AllowedModels { get; } = allowedModels;
}
