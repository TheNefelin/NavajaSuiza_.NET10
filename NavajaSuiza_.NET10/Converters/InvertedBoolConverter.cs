using System.Globalization;

namespace NavajaSuiza_.NET10.Converters;

public class InvertedBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
    {
        if (value is bool b)
            return !b;
        return false;
    }

    public object? ConvertBack(object? value, Type? targetType, object? parameter, CultureInfo? culture)
    {
        if (value is bool b)
            return !b;
        return false;
    }
}
