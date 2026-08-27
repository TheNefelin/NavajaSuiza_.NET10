namespace NavajaSuiza.Core.Interfaces;

public interface IFlashlightService
{
    Task TurnOnAsync();
    Task TurnOffAsync();
}
