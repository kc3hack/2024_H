using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using KoeBook.Models;
using Microsoft.UI.Xaml;

namespace KoeBook.ViewModels;

public sealed partial class EditDetailsViewModel : ObservableObject
{
    public GenerationTask Task { get; private set; } = default!;

    public string TabTitle => $"「{Task.Title}」の詳細ページ";

    [MemberNotNull(nameof(Task))]
    public void Initialize(GenerationTask task)
    {
        Task = task;
        OnPropertyChanged(nameof(Visibility));
    }
}
