namespace NavajaSuiza.Core.Interfaces;

public interface IImagePickerService
{
    Task<string?> PickImageAsync(string title);
}
