using System.Globalization;

namespace NavajaSuiza_.NET10.Converters;

public class BoolToColorConverter : IValueConverter
{
    public Color TrueColor { get; set; } = Colors.Transparent;
    public Color FalseColor { get; set; } = Colors.Transparent;
    public Color FalseColorLight { get; set; } = Colors.Transparent;
    public Color FalseColorDark { get; set; } = Colors.Transparent;

    public object? Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
    {
        if (value is bool isOn && isOn)
        {
            return TrueColor;
        }

        if (FalseColorLight != Colors.Transparent && FalseColorDark != Colors.Transparent)
        {
            return Application.Current?.RequestedTheme == AppTheme.Dark
                ? FalseColorDark
                : FalseColorLight;
        }

        return FalseColor;
    }

    public object? ConvertBack(object? value, Type? targetType, object? parameter, CultureInfo? culture)
    {
        throw new NotImplementedException();
    }
}