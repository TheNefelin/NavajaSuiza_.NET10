namespace NavajaSuiza.Core.Services;

public class FlashlightStateService : Interfaces.IFlashlightStateService
{
    private readonly object _lock = new();
    private bool _isFlashOn;

    public bool IsFlashOn
    {
        get { lock (_lock) { return _isFlashOn; } }
        set { lock (_lock) { _isFlashOn = value; } }
    }
}
