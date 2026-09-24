using System.Text.RegularExpressions;
using NavajaSuiza.Core.Interfaces;
using NavajaSuiza.Core.ViewModels;

namespace NavajaSuiza_.NET10.Pages;

public partial class GuidePage : ContentPage
{
    private const string GuideAssetPath = "guide/USER_GUIDE.es.md";

    private readonly IServiceProvider _serviceProvider;
    private ILanguageService? _languageService;

    public GuidePage(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _serviceProvider = serviceProvider;
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        var viewModel = _serviceProvider.GetRequiredService<GuideViewModel>();
        BindingContext = viewModel;
        _languageService = _serviceProvider.GetRequiredService<ILanguageService>();
        _languageService.LanguageChanged += OnLanguageChanged;

        LoadGuideAsync();
    }

    protected override void OnNavigatingFrom(NavigatingFromEventArgs args)
    {
        base.OnNavigatingFrom(args);

        if (_languageService is not null)
            _languageService.LanguageChanged -= OnLanguageChanged;
    }

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        LoadGuideAsync();
    }

    private async void LoadGuideAsync()
    {
        if (BindingContext is not GuideViewModel viewModel)
            return;

        viewModel.IsBusy = true;
        try
        {
            var markdown = await ReadGuideMarkdownAsync(_languageService!);
            var images = await ReadReferencedImagesAsync(markdown);
            var converter = _serviceProvider.GetRequiredService<IMarkdownToHtmlConverter>();
            viewModel.HtmlContent = converter.ConvertToHtml(markdown, images);
        }
        catch (Exception)
        {
            var message = _languageService!.GetString("GuideLoadErrorText");
            viewModel.HtmlContent = $"<p>{message}</p>";
        }
        finally
        {
            viewModel.IsBusy = false;
        }
    }

    private static async Task<string> ReadGuideMarkdownAsync(ILanguageService languageService)
    {
        var languageCode = languageService.GetCurrentLanguage();
        var path = $"guide/USER_GUIDE.{languageCode}.md";

        try
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync(path);
            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync();
        }
        catch (FileNotFoundException)
        {
            using var fallbackStream = await FileSystem.OpenAppPackageFileAsync(GuideAssetPath);
            using var fallbackReader = new StreamReader(fallbackStream);
            return await fallbackReader.ReadToEndAsync();
        }
    }

    private static async Task<Dictionary<string, string>> ReadReferencedImagesAsync(string markdown)
    {
        var dataUris = new Dictionary<string, string>();

        foreach (Match match in Regex.Matches(markdown, @"(?:!\[[^\]]*\]\(([^)]+)\)|src\s*=\s*""([^""]+)"")"))
        {
            var fileName = match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value;
            if (string.IsNullOrWhiteSpace(fileName) || dataUris.ContainsKey(fileName))
                continue;

            var bytes = await ReadAssetBytesAsync($"guide/{fileName}");
            if (bytes is null)
                continue;

            var mime = GetMimeType(fileName);
            dataUris[fileName] = $"data:{mime};base64,{Convert.ToBase64String(bytes)}";
        }

        return dataUris;
    }

    private static async Task<byte[]?> ReadAssetBytesAsync(string path)
    {
        try
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync(path);
            using var memory = new MemoryStream();
            await stream.CopyToAsync(memory);
            return memory.ToArray();
        }
        catch (FileNotFoundException)
        {
            return null;
        }
    }

    private static string GetMimeType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".webp" => "image/webp",
            ".svg" => "image/svg+xml",
            _ => "application/octet-stream"
        };
    }
}