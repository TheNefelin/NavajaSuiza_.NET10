namespace NavajaSuiza.Core.Interfaces;

public interface IMarkdownToHtmlConverter
{
    /// <param name="isDarkTheme">
    /// El WebView renderiza HTML plano, asi que AppThemeBinding no alcanza: la
    /// paleta se decide aqui y se recibe desde <c>IThemeService</c>.
    /// </param>
    string ConvertToHtml(string markdown, bool isDarkTheme, IReadOnlyDictionary<string, string>? imageDataUris = null);
}