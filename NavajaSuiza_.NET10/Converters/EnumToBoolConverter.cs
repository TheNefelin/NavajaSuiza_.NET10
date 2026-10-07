using System.Globalization;

namespace NavajaSuiza_.NET10.Converters;

public class EnumToBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
    {
        return value is not null && parameter is not null && value.ToString() == parameter.ToString();
    }

    public object? ConvertBack(object? value, Type? targetType, object? parameter, CultureInfo? culture)
    {
        if (value is true && parameter is not null && targetType is { IsEnum: true })
            return Enum.Parse(targetType, parameter.ToString()!, ignoreCase: true);

        return Binding.DoNothing;
    }
}
