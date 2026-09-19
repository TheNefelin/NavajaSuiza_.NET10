using Microsoft.Extensions.Logging;
using NavajaSuiza.Core.Interfaces;
using NavajaSuiza.Core.Models;

namespace NavajaSuiza_.NET10.Services.Implementations;

public class PizarraImageExporter : IPizarraImageExporter
{
    private const string FileName = "pizarra.webp";
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

#if ANDROID
        try
        {
            var (width, height, renderedStrokes) = BuildLayout(strokes);

            var webpBytes = RenderWebP(width, height, boardColorHex, renderedStrokes);
            if (webpBytes is null)
                return PizarraExportResult.Failed;

            if (Android.OS.Build.VERSION.SdkInt < Android.OS.BuildVersionCodes.Q)
            {
                var status = await Permissions.RequestAsync<Permissions.StorageWrite>();
                if (status != PermissionStatus.Granted)
                    return PizarraExportResult.Failed;
            }

            return InsertToGallery(webpBytes) ? PizarraExportResult.Saved : PizarraExportResult.Failed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al exportar la pizarra a la galería");
            return PizarraExportResult.Failed;
        }
#else
        _logger.LogInformation("Export de pizarra no disponible en esta plataforma");
        return PizarraExportResult.NotAvailable;
#endif
    }

#if ANDROID
    private static (int Width, int Height, List<RenderedStroke> Strokes) BuildLayout(IReadOnlyList<PizarraStroke> strokes)
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

        var boundsWidth = (maxX - minX) + (2f * maxHalfWidth) + (2f * LayoutPadding);
        var boundsHeight = (maxY - minY) + (2f * maxHalfWidth) + (2f * LayoutPadding);
        var scale = (double)MaxDimension / Math.Max(boundsWidth, boundsHeight);

        var width = (int)Math.Clamp(Math.Ceiling(boundsWidth * scale), 1, MaxDimension);
        var height = (int)Math.Clamp(Math.Ceiling(boundsHeight * scale), 1, MaxDimension);

        var offsetX = maxHalfWidth + LayoutPadding - minX;
        var offsetY = maxHalfWidth + LayoutPadding - minY;

        var rendered = new List<RenderedStroke>(strokes.Count);

        foreach (var stroke in strokes)
        {
            var target = new RenderedStroke
            {
                ColorHex = stroke.ColorHex,
                Thickness = Math.Max(1f, (float)(stroke.Width * scale))
            };

            foreach (var point in stroke.Points)
                target.Points.Add((point.X + offsetX, point.Y + offsetY));

            rendered.Add(target);
        }

        return (width, height, rendered);
    }

    private static byte[]? RenderWebP(int width, int height, string boardColorHex, List<RenderedStroke> strokes)
    {
        Android.Graphics.Bitmap? bitmap = null;

        try
        {
            var bitmapConfig = Android.Graphics.Bitmap.Config.Argb8888!;
            bitmap = Android.Graphics.Bitmap.CreateBitmap(width, height, bitmapConfig);
            var canvas = new Android.Graphics.Canvas(bitmap);

            using var backgroundPaint = new Android.Graphics.Paint
            {
                Color = Android.Graphics.Color.ParseColor(boardColorHex),
                AntiAlias = true
            };
            canvas.DrawRect(0, 0, width, height, backgroundPaint);

            foreach (var stroke in strokes)
            {
                if (stroke.Points.Count < 2)
                    continue;

                using var paint = new Android.Graphics.Paint
                {
                    Color = Android.Graphics.Color.ParseColor(stroke.ColorHex),
                    StrokeWidth = stroke.Thickness,
                    StrokeCap = Android.Graphics.Paint.Cap.Round,
                    StrokeJoin = Android.Graphics.Paint.Join.Round,
                    AntiAlias = true
                };
                paint.SetStyle(Android.Graphics.Paint.Style.Stroke);

                using var path = new Android.Graphics.Path();
                path.MoveTo(stroke.Points[0].Item1, stroke.Points[0].Item2);

                for (var i = 1; i < stroke.Points.Count; i++)
                    path.LineTo(stroke.Points[i].Item1, stroke.Points[i].Item2);

                canvas.DrawPath(path, paint);
            }

            canvas.Dispose();

            using var stream = new MemoryStream();
#pragma warning disable CA1422 // CompressFormat.Webp marcado obsoleto en API 30+, sigue siendo funcional
            var format = Android.Graphics.Bitmap.CompressFormat.Webp!;
#pragma warning restore CA1422
            if (!bitmap.Compress(format, WebpQuality, stream))
                return null;

            return stream.ToArray();
        }
        catch
        {
            return null;
        }
        finally
        {
            bitmap?.Recycle();
        }
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
        values.Put(Android.Provider.MediaStore.IMediaColumns.DisplayName, FileName);
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

    private sealed class RenderedStroke
    {
        public string ColorHex { get; init; } = string.Empty;

        public float Thickness { get; init; }

        public List<(float Item1, float Item2)> Points { get; } = new();
    }
#endif
}