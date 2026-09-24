namespace NavajaSuiza.Core.Interfaces;

public interface IMarkdownToHtmlConverter
{
    string ConvertToHtml(string markdown, IReadOnlyDictionary<string, string>? imageDataUris = null);
}