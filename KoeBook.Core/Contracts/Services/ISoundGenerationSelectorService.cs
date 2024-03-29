﻿using KoeBook.Core.Models;

namespace KoeBook.Core.Contracts.Services;

public interface ISoundGenerationSelectorService
{
    /// <summary>
    /// サウンドモデル・スタイルの一覧
    /// </summary>
    public IReadOnlyList<SoundModel> Models { get; }

    public ValueTask InitializeAsync(CancellationToken cancellationToken);
}
