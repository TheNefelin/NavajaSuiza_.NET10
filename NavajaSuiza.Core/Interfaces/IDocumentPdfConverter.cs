namespace NavajaSuiza.Core.Interfaces;

public interface IDocumentPdfConverter
{
    Task<MemoryStream?> ConvertToPdfAsync(string path, CancellationToken cancellationToken = default);
}