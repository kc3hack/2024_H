using System.Reflection;

using CommunityToolkit.Mvvm.ComponentModel;

using KoeBook.Contracts.Services;
using KoeBook.Core.Contracts.Services;
using KoeBook.Helpers;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel;

namespace KoeBook.ViewModels;

public partial class SettingsViewModel : ObservableRecipient
{
    private readonly IThemeSelectorService _themeSelectorService;
    private readonly IApiRootSelectorService _apiRootSelectorService;
    private readonly ILocalSettingsService _localSettingsService;
    private readonly ISoundGenerationSelectorService _soundGenerationSelectorService;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedThemeIndex))]
    private ElementTheme _elementTheme;

    public int SelectedThemeIndex
    {
        get => (int)ElementTheme;
        set => ElementTheme = (ElementTheme)value;
    }

    [ObservableProperty]
    private string _apiKey = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ApiKeyRevealMode))]
    [NotifyPropertyChangedFor(nameof(ApiKeyDescription))]
    private bool _revealApiKey;

    public PasswordRevealMode ApiKeyRevealMode => RevealApiKey ? PasswordRevealMode.Visible : PasswordRevealMode.Hidden;

    public string ApiKeyDescription => RevealApiKey ? "表示" : "非表示";

    [ObservableProperty]
    private string _styleBertVitsRoot = string.Empty;

    [ObservableProperty]
    private string _versionDescription;

    public SettingsViewModel(IThemeSelectorService themeSelectorService,
        IApiRootSelectorService apiRootSelectorService,
        ISoundGenerationSelectorService soundGenerationSelectorService,
        ILocalSettingsService localSettingsService)
    {
        _themeSelectorService = themeSelectorService;
        _elementTheme = _themeSelectorService.Theme;
        _localSettingsService = localSettingsService;
        _apiRootSelectorService = apiRootSelectorService;
        _styleBertVitsRoot = apiRootSelectorService.StyleBertVitsRoot;
        _soundGenerationSelectorService = soundGenerationSelectorService;
        _versionDescription = GetVersionDescription();
    }

    public async void OnThemeChangedAsync(object _, SelectionChangedEventArgs __)
    {
        await _themeSelectorService.SetThemeAsync(ElementTheme);
    }

    partial void OnApiKeyChanged(string value)
    {
        Core(_localSettingsService, value);

        static async void Core(ILocalSettingsService service, string value)
        {
            await service.SaveApiKeyAsync(value, default);
        }
    }

    partial void OnStyleBertVitsRootChanged(string value)
    {
        Core(_apiRootSelectorService, _soundGenerationSelectorService, value);

        static async void Core(IApiRootSelectorService service, ISoundGenerationSelectorService soundService, string root)
        {
            await service.SetStyleBertVitsRoot(root);
            await soundService.InitializeAsync(default);
        }
    }

    public async void OnLoaded(object _, RoutedEventArgs __)
    {
        var key = await _localSettingsService.GetApiKeyAsync(default);
        if (key is not null)
            ApiKey = key;
    }

    private static string GetVersionDescription()
    {
        Version version;

        if (RuntimeHelper.IsMSIX)
        {
            var packageVersion = Package.Current.Id.Version;

            version = new(packageVersion.Major, packageVersion.Minor, packageVersion.Build, packageVersion.Revision);
        }
        else
        {
            version = Assembly.GetExecutingAssembly().GetName().Version!;
        }

        return $"KoeBook - {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
    }
}
