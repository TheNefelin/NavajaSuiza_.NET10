using System.Globalization;

namespace NavajaSuiza_.NET10.Converters;

public class StringNotEmptyConverter : IValueConverter
{
    public object? Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
    {
        return value is string text && !string.IsNullOrWhiteSpace(text);
    }

    public object? ConvertBack(object? value, Type? targetType, object? parameter, CultureInfo? culture)
    {
        throw new NotSupportedException();
    }
}