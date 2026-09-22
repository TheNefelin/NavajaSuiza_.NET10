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
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                }
            },
            { DevicePlatform.WinUI, new[] { ".pdf", ".docx", ".xlsx" } },
            {
                DevicePlatform.iOS,
                new[]
                {
                    "com.adobe.pdf",
                    "org.openxmlformats.wordprocessingml.document",
                    "org.openxmlformats.spreadsheetml.sheet"
                }
            },
            {
                DevicePlatform.MacCatalyst,
                new[]
                {
                    "com.adobe.pdf",
                    "org.openxmlformats.wordprocessingml.document",
                    "org.openxmlformats.spreadsheetml.sheet"
                }
            }
        });
}