using NavajaSuiza.Core.Interfaces;

namespace NavajaSuiza_.NET10.Services.Implementations;

public class FilePickerService : IFilePickerService
{
    public async Task<string?> PickDocumentAsync(string title)
    {
        var result = await FilePicker.PickAsync(new PickOptions
        {
            PickerTitle = title,
            FileTypes = Documents
        }).ConfigureAwait(false);

        return result?.FullPath;
    }

    private static readonly FilePickerFileType Documents = new(
        new Dictionary<DevicePlatform, IEnumerable<string>>
        {
            {
                DevicePlatform.Android,
                new[]
                {
                    "application/pdf",
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "text/comma-separated-values",
                    "text/plain",
                    "text/*",
                    "application/javascript",
                    "application/json",
                    "application/typescript",
                    "text/html",
                    "text/markdown"
                }
            },
            {
                DevicePlatform.WinUI,
                new[]
                {
                    ".pdf",
                    ".docx",
                    ".xlsx",
                    ".csv",
                    ".txt",
                    ".js",
                    ".css",
                    ".ts",
                    ".json",
                    ".html",
                    ".md"
                }
            },
            {
                DevicePlatform.iOS,
                new[]
                {
                    "com.adobe.pdf",
                    "org.openxmlformats.wordprocessingml.document",
                    "org.openxmlformats.spreadsheetml.sheet",
                    "public.comma-separated-values-text",
                    "public.plain-text",
                    "public.source-code",
                    "public.json",
                    "public.html",
                    "net.daringfireball.markdown"
                }
            },
            {
                DevicePlatform.MacCatalyst,
                new[]
                {
                    "com.adobe.pdf",
                    "org.openxmlformats.wordprocessingml.document",
                    "org.openxmlformats.spreadsheetml.sheet",
                    "public.comma-separated-values-text",
                    "public.plain-text",
                    "public.source-code",
                    "public.json",
                    "public.html",
                    "net.daringfireball.markdown"
                }
            }
        });
}