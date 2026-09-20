using NavajaSuiza.Core.Interfaces;

namespace NavajaSuiza_.NET10.Services.Implementations;

public class FilePickerService : IFilePickerService
{
    private static readonly FilePickerFileType TextDocuments = new(
        new Dictionary<DevicePlatform, IEnumerable<string>>
        {
            { DevicePlatform.Android, new[] {
                "text/plain", "text/csv", "application/csv", "text/comma-separated-values",
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" } },
            { DevicePlatform.WinUI, new[] { ".txt", ".csv", ".docx", ".xlsx" } },
            { DevicePlatform.iOS, new[] {
                "public.plain-text", "public.delimited-values-text", "public.comma-separated-values-text",
                "org.openxmlformats.wordprocessingml.document", "org.openxmlformats.spreadsheetml.sheet" } },
            { DevicePlatform.MacCatalyst, new[] {
                "public.plain-text", "public.delimited-values-text", "public.comma-separated-values-text",
                "org.openxmlformats.wordprocessingml.document", "org.openxmlformats.spreadsheetml.sheet" } }
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

    public async Task<string?> PickPdfAsync(string title)
    {
        var result = await FilePicker.PickAsync(new PickOptions
        {
            PickerTitle = title,
            FileTypes = PdfDocuments
        }).ConfigureAwait(false);

        return result?.FullPath;
    }

    private static readonly FilePickerFileType PdfDocuments = new(
        new Dictionary<DevicePlatform, IEnumerable<string>>
        {
            { DevicePlatform.Android, new[] { "application/pdf" } },
            { DevicePlatform.WinUI, new[] { ".pdf" } },
            { DevicePlatform.iOS, new[] { "com.adobe.pdf" } },
            { DevicePlatform.MacCatalyst, new[] { "com.adobe.pdf" } }
        });
}