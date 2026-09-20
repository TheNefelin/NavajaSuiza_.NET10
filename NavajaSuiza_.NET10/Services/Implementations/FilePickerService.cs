using NavajaSuiza.Core.Interfaces;

namespace NavajaSuiza_.NET10.Services.Implementations;

public class FilePickerService : IFilePickerService
{
    private static readonly FilePickerFileType TextDocuments = new(
        new Dictionary<DevicePlatform, IEnumerable<string>>
        {
            { DevicePlatform.Android, new[] { "text/plain", "text/csv", "application/csv", "text/comma-separated-values" } },
            { DevicePlatform.WinUI, new[] { ".txt", ".csv" } },
            { DevicePlatform.iOS, new[] { "public.plain-text", "public.delimited-values-text", "public.comma-separated-values-text" } },
            { DevicePlatform.MacCatalyst, new[] { "public.plain-text", "public.delimited-values-text", "public.comma-separated-values-text" } }
        });

    public async Task<string?> PickDocumentAsync(string title)
    {
        var result = await FilePicker.PickAsync(new PickOptions
        {
            PickerTitle = title,
            FileTypes = TextDocuments
        }).ConfigureAwait(false);

        return result?.FullPath;
    }
}