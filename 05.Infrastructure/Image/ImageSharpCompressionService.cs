/*
 * Sistem Manajemen Loyalitas Pelanggan (SMLP)
 * Image Compression Implementation — SixLabors.ImageSharp
 */

using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using Smlp.Application.Services.Image;

namespace Smlp.Infrastructure.Image;

/// <summary>
/// Implementasi kompresi gambar menggunakan SixLabors.ImageSharp.
/// Mendukung JPEG, PNG, dan WebP. Secara otomatis resize gambar
/// agar tidak melebihi batas dimensi yang dikonfigurasi.
/// </summary>
public sealed class ImageSharpCompressionService : IImageCompressionService
{
    private readonly ILogger<ImageSharpCompressionService> _logger;

    private static readonly HashSet<string> SupportedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/jpg", "image/png", "image/webp", "image/gif", "image/bmp"
    };

    public ImageSharpCompressionService(ILogger<ImageSharpCompressionService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public bool IsImageFile(byte[] data)
    {
        if (data is null || data.Length < 4) return false;

        // Check magic bytes
        // JPEG: FF D8 FF
        if (data[0] == 0xFF && data[1] == 0xD8 && data[2] == 0xFF) return true;
        // PNG: 89 50 4E 47
        if (data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47) return true;
        // WebP: 52 49 46 46 ... 57 45 42 50
        if (data.Length >= 12 && data[0] == 0x52 && data[1] == 0x49 && data[2] == 0x46 && data[3] == 0x46
            && data[8] == 0x57 && data[9] == 0x45 && data[10] == 0x42 && data[11] == 0x50) return true;
        // GIF: 47 49 46
        if (data[0] == 0x47 && data[1] == 0x49 && data[2] == 0x46) return true;
        // BMP: 42 4D
        if (data[0] == 0x42 && data[1] == 0x4D) return true;

        return false;
    }

    /// <inheritdoc />
    public bool IsImageContentType(string contentType)
        => !string.IsNullOrWhiteSpace(contentType)
           && SupportedContentTypes.Contains(contentType.Split(';')[0].Trim());

    /// <inheritdoc />
    public async Task<(bool IsValid, string? ErrorMessage)> ValidateImageAsync(
        byte[] data,
        string contentType,
        long maxSizeBytes = 5 * 1024 * 1024)
    {
        if (data is null || data.Length == 0)
            return (false, "File gambar tidak boleh kosong.");

        if (!IsImageContentType(contentType))
            return (false, $"Format gambar tidak didukung: {contentType}. Gunakan JPEG, PNG, atau WebP.");

        if (data.Length > maxSizeBytes)
            return (false, $"Ukuran file gambar ({data.Length / 1024 / 1024.0:F1} MB) melebihi batas maksimum ({maxSizeBytes / 1024 / 1024.0:F0} MB).");

        if (!IsImageFile(data))
            return (false, "File tidak dapat dikenali sebagai gambar yang valid.");

        try
        {
            using var ms = new MemoryStream(data);
            var info = await SixLabors.ImageSharp.Image.IdentifyAsync(ms);
            if (info is null)
                return (false, "Gambar tidak dapat dibaca atau rusak.");

            // Batas dimensi maksimum yang wajar
            if (info.Width > 10000 || info.Height > 10000)
                return (false, $"Dimensi gambar terlalu besar ({info.Width}×{info.Height}px). Maksimum 10000×10000px.");

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Validasi gambar gagal.");
            return (false, "Gambar tidak dapat diproses. Pastikan file tidak rusak.");
        }
    }

    /// <inheritdoc />
    public async Task<ImageCompressionResult?> CompressAsync(
        byte[] imageData,
        ImageCompressionOptions? options = null)
    {
        if (imageData is null || imageData.Length == 0) return null;

        using var inputStream = new MemoryStream(imageData);
        return await CompressAsync(inputStream, options);
    }

    /// <inheritdoc />
    public async Task<ImageCompressionResult?> CompressAsync(
        Stream imageStream,
        ImageCompressionOptions? options = null)
    {
        options ??= ImageCompressionOptions.Standard;

        try
        {
            var originalPosition = imageStream.CanSeek ? imageStream.Position : 0;
            var originalSize = (int)(imageStream.CanSeek ? imageStream.Length : 0);

            using var image = await SixLabors.ImageSharp.Image.LoadAsync(imageStream);

            var originalWidth = image.Width;
            var originalHeight = image.Height;

            // Resize jika perlu (pertahankan aspect ratio)
            if ((options.MaxWidthPx > 0 && image.Width > options.MaxWidthPx)
                || (options.MaxHeightPx > 0 && image.Height > options.MaxHeightPx))
            {
                image.Mutate(ctx => ctx.Resize(new ResizeOptions
                {
                    Size = new Size(
                        options.MaxWidthPx > 0 ? options.MaxWidthPx : int.MaxValue,
                        options.MaxHeightPx > 0 ? options.MaxHeightPx : int.MaxValue),
                    Mode = ResizeMode.Max
                }));
            }

            // Tentukan encoder
            var (encoder, contentType) = GetEncoder(options);

            using var outputStream = new MemoryStream();
            await image.SaveAsync(outputStream, encoder);
            var compressedBytes = outputStream.ToArray();

            _logger.LogDebug(
                "Kompresi gambar selesai: {OrigW}×{OrigH}→{OutW}×{OutH}, {OrigKb}KB→{CompKb}KB ({Ratio}% lebih kecil)",
                originalWidth, originalHeight, image.Width, image.Height,
                originalSize / 1024, compressedBytes.Length / 1024,
                originalSize > 0 ? Math.Round((1.0 - (double)compressedBytes.Length / originalSize) * 100, 1) : 0);

            return new ImageCompressionResult
            {
                CompressedData = compressedBytes,
                OriginalSizeBytes = originalSize,
                CompressedSizeBytes = compressedBytes.Length,
                OriginalWidthPx = originalWidth,
                OriginalHeightPx = originalHeight,
                OutputWidthPx = image.Width,
                OutputHeightPx = image.Height,
                ContentType = contentType
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Kompresi gambar gagal, file akan disimpan tanpa kompresi.");
            return null;
        }
    }

    // ── Private Helpers ──────────────────────────────────────────────────────

    private static (IImageEncoder Encoder, string ContentType) GetEncoder(ImageCompressionOptions options)
    {
        return options.OutputFormat switch
        {
            ImageOutputFormat.Png => (new PngEncoder { CompressionLevel = PngCompressionLevel.BestSpeed }, "image/png"),
            ImageOutputFormat.WebP => (new WebpEncoder { Quality = options.Quality }, "image/webp"),
            _ => (new JpegEncoder { Quality = options.Quality }, "image/jpeg")
        };
    }
}
