using Microsoft.Extensions.Logging;
using NavajaSuiza.Core.Interfaces;
using NavajaSuiza.Core.Models;
using SkiaSharp;

namespace NavajaSuiza_.NET10.Services.Implementations;

public sealed class BoardImageExporter : IBoardImageExporter
{
    private const string FileNamePrefix = "board";
    private const string WebpMimeType = "image/webp";
    private const int WebpQuality = 95;

    private readonly ILogger<BoardImageExporter> _logger;

    public BoardImageExporter(ILogger<BoardImageExporter> logger)
    {
        _logger = logger;
    }

    public async Task<BoardExportResult> ExportAsync(
        IReadOnlyList<BoardStroke> strokes,
        IReadOnlyList<BoardText> texts,
        string boardColorHex,
        float canvasWidth,
        float canvasHeight,
        float renderScale)
    {
        if (strokes.Count == 0 && texts.Count == 0)
            return BoardExportResult.Failed;

        if (canvasWidth <= 0f || canvasHeight <= 0f)
            return BoardExportResult.Failed;

        try
        {
            var webpBytes = RenderWebP(strokes, texts, boardColorHex, canvasWidth, canvasHeight, renderScale);
            if (webpBytes is null)
                return BoardExportResult.Failed;

            return await SaveAsync(webpBytes).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting the board");
            return BoardExportResult.Failed;
        }
    }

    private static byte[]? RenderWebP(
        IReadOnlyList<BoardStroke> strokes,
        IReadOnlyList<BoardText> texts,
        string boardColorHex,
        float canvasWidth,
        float canvasHeight,
        float renderScale)
    {
        using var bitmap = RenderBitmap(strokes, texts, boardColorHex, canvasWidth, canvasHeight, renderScale);
        if (bitmap is null)
            return null;

        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Webp, WebpQuality);
        if (data is null || data.IsEmpty)
            return null;

        return data.ToArray();
    }

    private static SKBitmap? RenderBitmap(
        IReadOnlyList<BoardStroke> strokes,
        IReadOnlyList<BoardText> texts,
        string boardColorHex,
        float canvasWidth,
        float canvasHeight,
        float renderScale)
    {
        var scale = renderScale > 0f ? renderScale : 1f;
        var width = Math.Max(1, (int)Math.Round(canvasWidth * scale));
        var height = Math.Max(1, (int)Math.Round(canvasHeight * scale));

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
                var x = point.X * scale;
                var y = point.Y * scale;

                if (i == 0)
                    builder.MoveTo(x, y);
                else
                    builder.LineTo(x, y);
            }

            using var path = builder.Detach();
            canvas.DrawPath(path, paint);
        }

        foreach (var text in texts)
        {
            if (string.IsNullOrWhiteSpace(text.Content))
                continue;

            using var paint = new SKPaint
            {
                IsAntialias = true,
                Color = SKColor.Parse(text.ColorHex)
            };

            using var font = new SKFont(SKTypeface.Default, text.FontSize * scale);
            font.GetFontMetrics(out var metrics);

            canvas.DrawText(
                text.Content,
                text.X * scale,
                (text.Y * scale) - metrics.Ascent,
                SKTextAlign.Left,
                font,
                paint);
        }

        return bitmap;
    }

    #if ANDROID
    private async Task<BoardExportResult> SaveAsync(byte[] webpBytes)
    {
        if (Android.OS.Build.VERSION.SdkInt < Android.OS.BuildVersionCodes.Q)
        {
            var status = await Permissions.RequestAsync<Permissions.StorageWrite>().ConfigureAwait(false);
            if (status != PermissionStatus.Granted)
                return BoardExportResult.Failed;
        }

        return InsertToGallery(webpBytes) ? BoardExportResult.Saved : BoardExportResult.Failed;
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
    private Task<BoardExportResult> SaveAsync(byte[] webpBytes)
    {
        try
        {
            var picturesPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            if (string.IsNullOrEmpty(picturesPath))
                return Task.FromResult(BoardExportResult.Failed);

            Directory.CreateDirectory(picturesPath);

            var fileName = $"{FileNamePrefix}_{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid():N}.webp";
            var filePath = Path.Combine(picturesPath, fileName);
            File.WriteAllBytes(filePath, webpBytes);
            return Task.FromResult(BoardExportResult.Saved);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving the board to the Pictures folder");
            return Task.FromResult(BoardExportResult.Failed);
        }
    }
#else
    private Task<BoardExportResult> SaveAsync(byte[] webpBytes)
    {
        _logger.LogInformation("Board export to gallery not available on this platform");
        return Task.FromResult(BoardExportResult.NotAvailable);
    }
#endif
}