using NavajaSuiza_.NET10.Services.Interfaces;

namespace NavajaSuiza_.NET10.Services.Implementations;

public class ImageProcessingService : IImageProcessingService
{
    public async Task<ImageSource> LoadImageAsync()
    {
        var result = await FilePicker.PickAsync(new PickOptions
        {
            PickerTitle = "Selecciona una imagen",
            FileTypes = FilePickerFileType.Images
        });

        if (result != null)
            return ImageSource.FromFile(result.FullPath);

        return null;
    }

    public Task<Stream> ProcessImageAsync(Stream imageStream, string aspectRatio, string fitMode, string canvasBackground, int blurIntensity)
    {
        throw new NotImplementedException();
    }
}
