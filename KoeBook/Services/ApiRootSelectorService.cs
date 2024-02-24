using KoeBook.Core.Contracts.Services;

namespace KoeBook.Services;

public class ApiRootSelectorService(ILocalSettingsService localSettingsService) : IApiRootSelectorService
{
    private readonly ILocalSettingsService _localSettingsService = localSettingsService;

    public string StyleBertVitsRoot { get; private set; } = string.Empty;

    public async ValueTask InitializeAsync(CancellationToken cancellationToken)
    {
        var root = await _localSettingsService.ReadSettingAsync<string>(nameof(StyleBertVitsRoot));
        if (root is not null)
            StyleBertVitsRoot = root;
    }

    public async ValueTask SetStyleBertVitsRoot(string root)
    {
        StyleBertVitsRoot = root;
        await _localSettingsService.SaveSettingAsync(nameof(StyleBertVitsRoot), root);
    }
}
