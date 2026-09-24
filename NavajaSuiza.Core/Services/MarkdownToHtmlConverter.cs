using System.Text.RegularExpressions;
using Markdig;
using NavajaSuiza.Core.Interfaces;

namespace NavajaSuiza.Core.Services;

public partial class MarkdownToHtmlConverter : IMarkdownToHtmlConverter
{
    private readonly MarkdownPipeline _pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .Build();

    [GeneratedRegex(@"src=""([^""]+)""")]
    private static partial Regex ImageSourceRegex();

    public string ConvertToHtml(string markdown, IReadOnlyDictionary<string, string>? imageDataUris = null)
    {
        var html = Markdown.ToHtml(markdown, _pipeline);

        if (imageDataUris is not null && imageDataUris.Count > 0)
        {
            html = ImageSourceRegex().Replace(html, match =>
            {
                var source = match.Groups[1].Value;
                return imageDataUris.TryGetValue(source, out var dataUri)
                    ? $"src=\"{dataUri}\""
                    : match.Value;
            });
        }

        return HtmlTemplate(html);
    }

    private static string HtmlTemplate(string body)
    {
        return $$"""
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset="utf-8" />
                <meta name="viewport" content="width=device-width, initial-scale=1.0" />
                <style>
                    body { font-family: system-ui, -apple-system, sans-serif; margin: 20px; line-height: 1.5; }
                    img { max-width: 100%; height: auto; }
                    table { width: 100%; border-collapse: collapse; }
                    th, td { text-align: center; vertical-align: top; padding: 4px; }
                    h1 { font-size: 24px; }
                    h2 { font-size: 20px; }
                    h3 { font-size: 17px; }
                </style>
            </head>
            <body>
            {{body}}
            </body>
            </html>
            """;
    }
}