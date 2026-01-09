namespace NavajaSuiza_.NET10.Services.Interfaces;

public interface IImageProcessingService
{
    Task<ImageSource> LoadImageAsync();
    Task<Stream> ProcessImageAsync(Stream imageStream, string aspectRatio, string fitMode, string canvasBackground, int blurIntensity);
}
