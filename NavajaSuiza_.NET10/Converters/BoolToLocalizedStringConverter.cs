using NavajaSuiza_.NET10.Extensions;
using NavajaSuiza_.NET10.Resources.Languages;
using System.Globalization;

namespace NavajaSuiza_.NET10.Converters;

public class BoolToLocalizedStringConverter : IValueConverter
{
    public string TrueResourceKey { get; set; }
    public string FalseResourceKey { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isOn)
        {
            var key = isOn ? TrueResourceKey : FalseResourceKey;
            // Accede al diccionario de LocalizationResourceManager
            return LocalizationResourceManager.Instance[key] ?? key;
        }
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}