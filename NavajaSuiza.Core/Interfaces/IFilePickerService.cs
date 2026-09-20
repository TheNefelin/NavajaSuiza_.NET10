namespace NavajaSuiza.Core.Interfaces;

public interface IFilePickerService
{
    Task<string?> PickDocumentAsync(string title);

    Task<string?> PickPdfAsync(string title);
}