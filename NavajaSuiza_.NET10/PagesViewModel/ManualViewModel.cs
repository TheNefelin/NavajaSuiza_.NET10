using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;

namespace NavajaSuiza_.NET10.PagesViewModel;

public partial class ManualViewModel : BaseViewModel
{
    private readonly ILogger<ManualViewModel> _logger;

    public ManualViewModel(ILogger<ManualViewModel> logger)
    {
        _logger = logger;
    }

    [ObservableProperty]
    private ObservableCollection<InstrumentItem> instruments = new()
    {
        new InstrumentItem
        {
            Name = "Guitarra Clásica",
            ImagePath = "icon_sun.svg",
            Description = "Guitarra de nylon tradicional"
        },
        new InstrumentItem
        {
            Name = "Guitarra Acústica",
            ImagePath = "guitar2.png",
            Description = "Guitarra acústica de cuerdas de acero"
        },
        new InstrumentItem
        {
            Name = "Guitarra Eléctrica",
            ImagePath = "guitar3.png",
            Description = "Guitarra eléctrica amplificada"
        },
        new InstrumentItem
        {
            Name = "Bajo",
            ImagePath = "bass.png",
            Description = "Bajo eléctrico de 4 cuerdas"
        },
        new InstrumentItem
        {
            Name = "Ukelele",
            ImagePath = "ukelele.png",
            Description = "Ukelele hawaiano"
        },
        new InstrumentItem
        {
            Name = "Mandolina",
            ImagePath = "mandolin.png",
            Description = "Mandolina italiana"
        }
    };

    [RelayCommand]
    private void ItemChanged(InstrumentItem item)
    {
        // Se ejecuta cuando cambias de instrumento
        _logger.LogInformation("Instrumento seleccionado: {InstrumentName}", item.Name);
    }
}

public class InstrumentItem
{
    public string Name { get; set; }
    public string ImagePath { get; set; }
    public string Description { get; set; }
}