using Microsoft.Extensions.Logging;
using NavajaSuiza.Core.Interfaces;
using NavajaSuiza.Core.Models;
using SkiaSharp;

namespace NavajaSuiza_.NET10.Services.Implementations;

public sealed class PizarraImageExporter : IPizarraImageExporter
{
    private const string FileNamePrefix = "pizarra";
    private const string WebpMimeType = "image/webp";
    private const int MaxDimension = 2048;
    private const int LayoutPadding = 40;
    private const int WebpQuality = 95;

    private readonly ILogger<PizarraImageExporter> _logger;

    public PizarraImageExporter(ILogger<PizarraImageExporter> logger)
    {
        _logger = logger;
    }

    public async Task<PizarraExportResult> ExportAsync(IReadOnlyList<PizarraStroke> strokes, string boardColorHex)
    {
        if (strokes.Count == 0)
            return PizarraExportResult.Failed;

        try
        {
            var webpBytes = RenderWebP(strokes, boardColorHex);
            if (webpBytes is null)
                return PizarraExportResult.Failed;

            return await SaveAsync(webpBytes).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al exportar la pizarra");
            return PizarraExportResult.Failed;
        }
    }

    private static byte[]? RenderWebP(IReadOnlyList<PizarraStroke> strokes, string boardColorHex)
    {
        using var bitmap = RenderBitmap(strokes, boardColorHex);
        if (bitmap is null)
            return null;

        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Webp, WebpQuality);
        if (data is null || data.IsEmpty)
            return null;

        return data.ToArray();
    }

    private static SKBitmap? RenderBitmap(IReadOnlyList<PizarraStroke> strokes, string boardColorHex)
    {
        if (!TryComputeLayout(strokes, out var width, out var height, out var offsetX, out var offsetY, out var scale))
            return null;

        var bitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColor.Parse(boardColorHex));

        foreach (var stroke in strokes)
        {
            if (stroke.Points.Count < 2)
                continue;

            using var paint = new SKPaint
            {
                IsAntialias = true,
                StrokeCap = SKStrokeCap.Round,
                StrokeJoin = SKStrokeJoin.Round,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = Math.Max(1f, stroke.Width * scale),
                Color = SKColor.Parse(stroke.ColorHex)
            };

            using var builder = new SKPathBuilder();

            for (var i = 0; i < stroke.Points.Count; i++)
            {
                var point = stroke.Points[i];
                var x = (point.X + offsetX) * scale;
                var y = (point.Y + offsetY) * scale;

                if (i == 0)
                    builder.MoveTo(x, y);
                else
                    builder.LineTo(x, y);
            }

            using var path = builder.Detach();
            canvas.DrawPath(path, paint);
        }

        return bitmap;
    }

    private static bool TryComputeLayout(IReadOnlyList<PizarraStroke> strokes, out int width, out int height, out float offsetX, out float offsetY, out float scale)
    {
        var minX = float.MaxValue;
        var minY = float.MaxValue;
        var maxX = float.MinValue;
        var maxY = float.MinValue;
        var maxHalfWidth = 0f;

        foreach (var stroke in strokes)
        {
            maxHalfWidth = Math.Max(maxHalfWidth, stroke.Width / 2f);

            foreach (var point in stroke.Points)
            {
                minX = Math.Min(minX, point.X);
                minY = Math.Min(minY, point.Y);
                maxX = Math.Max(maxX, point.X);
                maxY = Math.Max(maxY, point.Y);
            }
        }

        if (maxX < minX || maxY < minY)
        {
            width = 0;
            height = 0;
            offsetX = 0f;
            offsetY = 0f;
            scale = 0f;
            return false;
        }

        var contentWidth = (maxX - minX) + (2f * maxHalfWidth) + (2f * LayoutPadding);
        var contentHeight = (maxY - minY) + (2f * maxHalfWidth) + (2f * LayoutPadding);
        scale = Math.Min(1f, MaxDimension / Math.Max(contentWidth, contentHeight));

        width = Math.Clamp((int)Math.Ceiling(contentWidth * scale), 1, MaxDimension);
        height = Math.Clamp((int)Math.Ceiling(contentHeight * scale), 1, MaxDimension);

        offsetX = maxHalfWidth + LayoutPadding - minX;
        offsetY = maxHalfWidth + LayoutPadding - minY;
        return true;
    }

    #if ANDROID
    private async Task<PizarraExportResult> SaveAsync(byte[] webpBytes)
    {
        if (Android.OS.Build.VERSION.SdkInt < Android.OS.BuildVersionCodes.Q)
        {
            var status = await Permissions.RequestAsync<Permissions.StorageWrite>().ConfigureAwait(false);
            if (status != PermissionStatus.Granted)
                return PizarraExportResult.Failed;
        }

        return InsertToGallery(webpBytes) ? PizarraExportResult.Saved : PizarraExportResult.Failed;
    }

    private static bool InsertToGallery(byte[] webpBytes)
    {
        var contentResolver = Platform.AppContext.ContentResolver;
        if (contentResolver is null)
            return false;

        var externalUri = Android.Provider.MediaStore.Images.Media.ExternalContentUri;
        if (externalUri is null)
            return false;

        var values = new Android.Content.ContentValues();
        values.Put(Android.Provider.MediaStore.IMediaColumns.DisplayName, $"{FileNamePrefix}.webp");
        values.Put(Android.Provider.MediaStore.IMediaColumns.MimeType, WebpMimeType);

#pragma warning disable CA1416 // MediaStore RelativePath solo existe en API 29+; la llamada está protegida por el chequeo de SDK
        if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Q)
            values.Put(Android.Provider.MediaStore.IMediaColumns.RelativePath, "Pictures/");
#pragma warning restore CA1416

        var uri = contentResolver.Insert(externalUri, values);
        if (uri is null)
            return false;

        using var outputStream = contentResolver.OpenOutputStream(uri);
        if (outputStream is null)
            return false;

        outputStream.Write(webpBytes, 0, webpBytes.Length);
        return true;
    }
#elif WINDOWS
    private Task<PizarraExportResult> SaveAsync(byte[] webpBytes)
    {
        try
        {
            var picturesPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            if (string.IsNullOrEmpty(picturesPath))
                return Task.FromResult(PizarraExportResult.Failed);

            Directory.CreateDirectory(picturesPath);

            var fileName = $"{FileNamePrefix}_{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid():N}.webp";
            var filePath = Path.Combine(picturesPath, fileName);
            File.WriteAllBytes(filePath, webpBytes);
            return Task.FromResult(PizarraExportResult.Saved);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar la pizarra en Carpeta de imágenes");
            return Task.FromResult(PizarraExportResult.Failed);
        }
    }
#else
    private Task<PizarraExportResult> SaveAsync(byte[] webpBytes)
    {
        _logger.LogInformation("Export de pizarra a galería no disponible en esta plataforma");
        return Task.FromResult(PizarraExportResult.NotAvailable);
    }
#endif
}