namespace NavajaSuiza.Core.Interfaces;

/// <summary>
/// Lecturas crudas de estado del dispositivo, específicas de plataforma. La validación de rangos,
/// el fallback y el manejo de errores viven en <see cref="Services.DeviceStatusService"/> (Core,
/// testeable), que consume esta interfaz.
/// </summary>
public interface IDeviceStatusSource
{
    int GetBatteryCapacity();
    double GetBatteryChargeLevel();
    long GetAvailableStorageBytes();
    long GetTotalStorageBytes();
}
