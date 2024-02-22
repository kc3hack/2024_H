using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KoeBook.Core.Contracts.Services;
using KoeBook.Models;
using KoeBook.Services;
using KoeBook.ViewModels.Edit;
using Microsoft.UI.Xaml;

namespace KoeBook.ViewModels;

public sealed partial class EditDetailsViewModel : ObservableObject, IDisposable
{
    private readonly ISoundGenerationSelectorService _soundGenerationSelectorService;
    private readonly GenerationTaskRunnerService _generationTaskRunnerService;

    public GenerationTask Task { get; private set; } = default!;

    public string TabTitle => $"「{Task.Title}」の詳細ページ";

    public BookScriptsViewModel? BookScripts { get; private set; }

    public Visibility AnalyzingTextVisibility => BookScripts is null ? Visibility.Visible : Visibility.Collapsed;

    public EditDetailsViewModel(ISoundGenerationSelectorService soundGenerationSelectorService, GenerationTaskRunnerService generationTaskRunnerService)
    {
        _soundGenerationSelectorService = soundGenerationSelectorService;
        _generationTaskRunnerService = generationTaskRunnerService;
    }

    [MemberNotNull(nameof(Task))]
    public void Initialize(GenerationTask task)
    {
        Task = task;
        if (task.BookScripts is not null)
            BookScripts = new(task.BookScripts, task.Editable, _soundGenerationSelectorService.Models);
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
                Task.Editable,
                _soundGenerationSelectorService.Models);
            foreach (var pair in bookScripts.CharacterMapping)
            {
                if (!pair.AllowedModels.Contains(pair.Model))
                {
                    pair.Model = pair.AllowedModels[0];
                }
            }
            BookScripts = bookScripts;
            OnPropertyChanged(nameof(BookScripts));
            OnPropertyChanged(nameof(AnalyzingTextVisibility));
            OnPropertyChanged(nameof(BookScripts.CharacterMapping));
        }
        else if (e.PropertyName == nameof(GenerationTask.Editable))
        {
            if (BookScripts is not null)
            {
                foreach (var pair in BookScripts.CharacterMapping)
                    pair.Editable = Task.Editable;
            }
        }
    }

    [RelayCommand]
    private void StartGenerationAsync()
    {
        BookScripts!.Apply();
        _generationTaskRunnerService.RunGenerateEpubAsync(Task);
    }

    public void Dispose()
    {
        Task.PropertyChanged -= Task_PropertyChanged;
    }
}
