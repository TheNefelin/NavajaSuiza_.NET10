namespace NavajaSuiza_.NET10.Extensions;

/// <summary>
/// Add xmlns:services="clr-namespace:NavajaSuiza_.NET10.Services.Implementations"
/// Add Title="{services:Translate AboutText}""
/// </summary>
[ContentProperty(nameof(Name))]
public class TranslateExtension : IMarkupExtension<BindingBase>
{
    public string Name { get; set; } 

    public BindingBase ProvideValue(IServiceProvider serviceProvider)
    {
        return new Binding
        {
            Mode = BindingMode.OneWay,
            Path = $"[{Name}]",
            Source = LocalizationResourceManager.Instance
        };
    }

    object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
    {
        return ProvideValue(serviceProvider);
    }
}