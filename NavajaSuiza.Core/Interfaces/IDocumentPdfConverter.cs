using NavajaSuiza.Core.Models;

namespace NavajaSuiza.Core.Interfaces;

public interface IDocumentPdfConverter
{
    Task<Stream> ConvertToPdfAsync(DocumentType documentType, string path);
}