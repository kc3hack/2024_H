using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using KoeBook.Core.Contracts.Services;
using KoeBook.Core.Models;
using KoeBook.Models;
using KoeBook.ViewModels.Edit;

namespace KoeBook.ViewModels;

public sealed partial class EditDetailsViewModel : ObservableObject, IDisposable
{
    public GenerationTask Task { get; private set; } = default!;

    public string TabTitle => $"「{Task.Title}」の詳細ページ";

    public BookScriptsViewModel BookScripts { get; private set; } = default!;

    private readonly ISoundGenerationSelectorService _soundGenerationSelectorService;

    public EditDetailsViewModel(ISoundGenerationSelectorService soundGenerationSelectorService)
    {
        _soundGenerationSelectorService = soundGenerationSelectorService;
    }

    [MemberNotNull(nameof(Task))]
    public void Initialize(GenerationTask task)
    {
        Task = task;
        if (task.BookScripts is not null)
            BookScripts = new(task.BookScripts, _soundGenerationSelectorService.Models.Select(model => model.name).ToArray());
        else
        {
            task.PropertyChanged += Task_PropertyChanged;
        }
    }

    private void Task_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(GenerationTask.BookScripts) && Task.BookScripts is not null)
        {
            var bookScripts = new BookScriptsViewModel(
                Task.BookScripts,
                _soundGenerationSelectorService.Models.Select(model => model.name).ToArray());
            if(bookScripts.CharacterMapping.Any(pair => !pair.AllowedModels.Contains(pair.Model)))
            {
                Task.CancellationTokenSource.Cancel();
                Task.State = GenerationState.Failed;
                return;
            }
            BookScripts = bookScripts;
            OnPropertyChanged(nameof(BookScripts));
            OnPropertyChanged(nameof(BookScripts.CharacterMapping));
        }
    }

    public void Dispose()
    {
        Task.PropertyChanged -= Task_PropertyChanged;
    }
}
