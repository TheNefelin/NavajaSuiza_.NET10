namespace NavajaSuiza_.NET10.Diagnostics;

/// <summary>
/// Rutas de los archivos de diagnóstico de la aplicación.
/// </summary>
public static class DiagnosticsPaths
{
    /// <summary>
    /// Archivo de log donde se registran los eventos y las excepciones no controladas.
    /// En Android vive en el almacenamiento interno privado (no navegable por el usuario).
    /// </summary>
    public static string LogFilePath { get; } =
        Path.Combine(FileSystem.AppDataDirectory, "NavajaSuiza.log");
}
