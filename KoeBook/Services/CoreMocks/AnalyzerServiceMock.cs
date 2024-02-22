﻿using KoeBook.Core.Contracts.Services;
using KoeBook.Core.Helpers;
using KoeBook.Core.Models;

namespace KoeBook.Services.CoreMocks;

public class AnalyzerServiceMock(IDisplayStateChangeService stateService) : IAnalyzerService
{
    private readonly IDisplayStateChangeService _stateService = stateService;

    public async ValueTask<BookScripts> AnalyzeAsync(BookProperties bookProperties, CancellationToken cancellationToken)
    {
        var stateChanging = _stateService.ResetProgress(bookProperties, GenerationState.Downloading, 300);
        await Task.Delay(5000, cancellationToken).ConfigureAwait(false);
        stateChanging.UpdateProgress(30);
        await Task.Delay(500, cancellationToken).ConfigureAwait(false);
        stateChanging.UpdateProgress(100);
        await Task.Delay(500, cancellationToken).ConfigureAwait(false);
        stateChanging.UpdateProgress(300);

        stateChanging = _stateService.ResetProgress(bookProperties, GenerationState.Downloading, 400);
        await Task.Delay(500, cancellationToken).ConfigureAwait(false);
        stateChanging.UpdateProgress(240);
        await Task.Delay(500, cancellationToken).ConfigureAwait(false);
        stateChanging.UpdateProgress(400);

        var characterMapping = new Dictionary<string, string>()
        {
            ["Hoge"] = "男性1",
            ["Fuga"] = "女性1",
            ["Narration"] = "ナレーション (男性)",
        };
        return new(bookProperties, new(characterMapping))
        {
            ScriptLines = [
                new("a", "読み上げテキスト1", "Hoge", "Angry"),
                new("b", "読み上げテキスト2", "Fuga", "Sad"),
                new("c", "読み上げテキスト3", "Narration", "Narration"),
            ],
        };
    }
}
