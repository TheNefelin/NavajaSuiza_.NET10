using NavajaSuiza_.NET10.Resources.Languages;
using System.ComponentModel;
using System.Globalization;

namespace NavajaSuiza_.NET10.Extensions;

/// <summary>
/// Add Title="{Binding LocalizationResourceManager[ManualText], Mode=OneWay}"
/// </summary>
public class LocalizationResourceManager : INotifyPropertyChanged
{
    private CultureInfo _culture;

    private LocalizationResourceManager()
    {
        Culture = CultureInfo.CurrentCulture;
    }

    public CultureInfo Culture
    {
        get => _culture;
        set
        {
            _culture = value;
            CultureInfo.CurrentUICulture = CultureInfo.CurrentCulture = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }
    }

    public static LocalizationResourceManager Instance { get; } = new();

    public object this[string resourceKey]
        => AppResources.ResourceManager.GetObject(resourceKey, _culture) ?? Array.Empty<byte>();

    public event PropertyChangedEventHandler PropertyChanged;
}