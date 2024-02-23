using KoeBook.Contracts.Services;
using KoeBook.Core.Contracts.Services;
using KoeBook.Core.Models;
using KoeBook.Models;
using Windows.Storage;

namespace KoeBook.Services;

public class GenerationTaskRunnerService
{
    private readonly IGenerationTaskService _taskService;

    private readonly IAnalyzerService _analyzerService;

    private readonly IEpubGenerateService _epubGenService;

    private readonly string _tempFolder = ApplicationData.Current.TemporaryFolder.Path;

    public GenerationTaskRunnerService(IGenerationTaskService taskService, IAnalyzerService analyzerService, IEpubGenerateService epubGenService)
    {
        _taskService = taskService;
        _taskService.OnTasksChanged += TasksChanged;
        _analyzerService = analyzerService;
        _epubGenService = epubGenService;
    }

    private async void TasksChanged(GenerationTask task, ChangedEvents changedEvents)
    {
        switch (changedEvents)
        {
            case ChangedEvents.Registered:
                {
                    await RunAsync(task);
                    break;
                }
            case ChangedEvents.Unregistered:
                {
                    task.CancellationTokenSource.Cancel();
                    break;
                }
        }
    }

    private async ValueTask RunAsync(GenerationTask task)
    {
        try
        {
            var scripts = await _analyzerService.AnalyzeAsync(new(task.Id, task.Source, task.SourceType), task.CancellationToken);
            task.BookScripts = scripts;
            task.State = GenerationState.Editting;
            task.Progress = 0;
            task.MaximumProgress = 0;
            if (task.SkipEdit)
            {
                var resultPath = await _epubGenService.GenerateEpubAsync(scripts, _tempFolder, task.CancellationToken);
                task.State = GenerationState.Completed;
                task.Progress = 0;
                task.MaximumProgress = 0;
            }
        }
        catch (OperationCanceledException)
        {
            task.State = GenerationState.Failed;
        }
        catch
        {
            task.State = GenerationState.Failed;
        }
    }

    public async void RunGenerateEpubAsync(GenerationTask task)
    {
        if (task.CancellationToken.IsCancellationRequested || task.State == GenerationState.Failed || task.BookScripts is null)
            return;
        try
        {
            var resultPath = await _epubGenService.GenerateEpubAsync(task.BookScripts, _tempFolder, task.CancellationToken);
            task.State = GenerationState.Completed;
            task.Progress = 1;
            task.MaximumProgress = 1;
        }
        catch (OperationCanceledException)
        {
            task.State = GenerationState.Failed;
        }
        catch
        {
            task.State = GenerationState.Failed;
        }
    }

    public void StopTask(GenerationTask task)
    {
        task.CancellationTokenSource.Cancel();
        if (task.State != GenerationState.Completed)
        {
            task.State = GenerationState.Failed;
            task.Progress = 0;
            task.MaximumProgress = 0;
        }
        _taskService.Unregister(task.Id);
    }
}
