using NavajaSuiza.Core.Interfaces;

namespace NavajaSuiza_.NET10.Services.Implementations;

public class ImagePickerService : IImagePickerService
{
    public async Task<string?> PickImageAsync(string title)
    {
        var result = await FilePicker.PickAsync(new PickOptions
        {
            PickerTitle = title,
            FileTypes = FilePickerFileType.Images
        });

        return result?.FullPath;
    }
}
