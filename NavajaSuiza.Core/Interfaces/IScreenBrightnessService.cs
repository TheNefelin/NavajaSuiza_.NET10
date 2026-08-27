namespace NavajaSuiza.Core.Interfaces;

public interface IScreenBrightnessService
{
    double GetCurrentBrightness();
    void SetScreenBrightness(double brightness);
}
