using NavajaSuiza.Core.Interfaces;
using NavajaSuiza.Core.Services;

namespace NavajaSuiza.Test;

public class MarkdownToHtmlConverterTests
{
    private readonly IMarkdownToHtmlConverter _converter = new MarkdownToHtmlConverter();

    [Fact]
    public void ConvertToHtml_Heading_ProducesHeading()
    {
        var html = _converter.ConvertToHtml("# Título");

        Assert.Contains("<h1", html);
        Assert.Contains(">Título</h1>", html);
    }

    [Fact]
    public void ConvertToHtml_List_ProducesListItem()
    {
        var html = _converter.ConvertToHtml("- Uno\n- Dos");

        Assert.Contains("<li>Uno</li>", html);
        Assert.Contains("<li>Dos</li>", html);
    }

    [Fact]
    public void ConvertToHtml_Blockquote_ProducesQuote()
    {
        var html = _converter.ConvertToHtml("> cita");

        Assert.Contains("<blockquote>", html);
        Assert.Contains("cita", html);
    }

    [Fact]
    public void ConvertToHtml_ImageWithoutDataUri_KeepsOriginalSource()
    {
        var html = _converter.ConvertToHtml("![Pizarra](01.jpg)");

        Assert.Contains("<img src=\"01.jpg\"", html);
    }

    [Fact]
    public void ConvertToHtml_ImageWithMatchingDataUri_UsesDataUri()
    {
        var images = new Dictionary<string, string>
        {
            { "01.jpg", "data:image/jpeg;base64,AAA=" }
        };

        var html = _converter.ConvertToHtml("![Pizarra](01.jpg)", images);

        Assert.Contains("src=\"data:image/jpeg;base64,AAA=\"", html);
    }

    [Fact]
    public void ConvertToHtml_ImageWithNonMatchingDataUri_KeepsOriginalSource()
    {
        var images = new Dictionary<string, string>
        {
            { "other.png", "data:image/png;base64,BBB=" }
        };

        var html = _converter.ConvertToHtml("![Pizarra](01.jpg)", images);

        Assert.Contains("<img src=\"01.jpg\"", html);
    }
}