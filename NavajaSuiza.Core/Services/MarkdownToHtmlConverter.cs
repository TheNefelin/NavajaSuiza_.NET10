using System.Text.RegularExpressions;
using Markdig;
using NavajaSuiza.Core.Interfaces;

namespace NavajaSuiza.Core.Services;

public partial class MarkdownToHtmlConverter : IMarkdownToHtmlConverter
{
    // Paleta alineada con Resources/Styles/Colors.xaml. Background, Foreground y
    // Surface salen de MyBackgroundLight/Dark, MyPrimaryTextLight/Dark y
    // MyBackgroundMenuLight/Dark; Border y Muted no existen en ese diccionario y
    // son las mezclas necesarias para que tablas y citas tengan contraste en
    // ambos temas.
    private static readonly GuidePalette LightPalette = new(
        Background: "#FFFFFF",
        Foreground: "#2E4057",
        Border: "#D6D2CA",
        Surface: "#F7F5F0",
        Muted: "#4A5A6B");

    private static readonly GuidePalette DarkPalette = new(
        Background: "#1A2332",
        Foreground: "#F0EDE6",
        Border: "#3A4A5F",
        Surface: "#243042",
        Muted: "#BFCBD6");

    private readonly MarkdownPipeline _pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .Build();

    [GeneratedRegex(@"src=""([^""]+)""")]
    private static partial Regex ImageSourceRegex();

    public string ConvertToHtml(string markdown, bool isDarkTheme, IReadOnlyDictionary<string, string>? imageDataUris = null)
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

        return HtmlTemplate(html, isDarkTheme);
    }

    private static string HtmlTemplate(string body, bool isDarkTheme)
    {
        var palette = isDarkTheme ? DarkPalette : LightPalette;

        // La guia no usa enlaces ni bloques de codigo, asi que no se estilan.
        // hr y border de tabla si: el default del WebView es casi invisible
        // sobre fondo oscuro.
        return $$"""
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset="utf-8" />
                <meta name="viewport" content="width=device-width, initial-scale=1.0" />
                <style>
                    body { font-family: system-ui, -apple-system, sans-serif; margin: 20px; line-height: 1.5;
                           background-color: {{palette.Background}}; color: {{palette.Foreground}}; }
                    img { max-width: 100%; height: auto; }
                    table { width: 100%; border-collapse: collapse; }
                    th, td { text-align: center; vertical-align: top; padding: 4px; border: 1px solid {{palette.Border}}; }
                    th { background-color: {{palette.Surface}}; }
                    h1 { font-size: 24px; }
                    h2 { font-size: 20px; }
                    h3 { font-size: 17px; }
                    blockquote { margin: 0 0 16px; padding: 8px 12px; border-left: 3px solid {{palette.Border}};
                                 color: {{palette.Muted}}; }
                    hr { border: 0; border-top: 1px solid {{palette.Border}}; margin: 20px 0; }
                </style>
            </head>
            <body>
            {{body}}
            </body>
            </html>
            """;
    }

    private readonly record struct GuidePalette(
        string Background,
        string Foreground,
        string Border,
        string Surface,
        string Muted);
}