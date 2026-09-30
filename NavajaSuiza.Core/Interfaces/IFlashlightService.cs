namespace NavajaSuiza.Core.Interfaces;

public interface IFlashlightService
{
    Task TurnOnAsync();
    Task TurnOffAsync();
    bool SupportsVariableIntensity { get; }
    int MaxIntensityLevel { get; }
    int IntensityLevel { get; set; }
}
