/*
 * Sistem Manajemen Loyalitas Pelanggan (SMLP)
 * Image Compression Service Interface
 */

namespace Smlp.Application.Services.Image;

/// <summary>
/// Opsi kompresi gambar.
/// </summary>
public sealed class ImageCompressionOptions
{
    /// <summary>Kualitas JPEG/WebP output (1–100). Default: 80.</summary>
    public int Quality { get; init; } = 80;

    /// <summary>Lebar maksimum gambar output (pixel). 0 = tidak diubah.</summary>
    public int MaxWidthPx { get; init; } = 1280;

    /// <summary>Tinggi maksimum gambar output (pixel). 0 = tidak diubah.</summary>
    public int MaxHeightPx { get; init; } = 1280;

    /// <summary>Format output. Null = pertahankan format asli (fallback ke JPEG).</summary>
    public ImageOutputFormat? OutputFormat { get; init; }

    /// <summary>Opsi preset untuk avatar/foto profil kecil.</summary>
    public static ImageCompressionOptions Avatar => new()
    {
        Quality = 85,
        MaxWidthPx = 256,
        MaxHeightPx = 256,
        OutputFormat = ImageOutputFormat.Jpeg
    };

    /// <summary>Opsi preset untuk foto produk medium.</summary>
    public static ImageCompressionOptions ProductImage => new()
    {
        Quality = 80,
        MaxWidthPx = 800,
        MaxHeightPx = 800,
        OutputFormat = ImageOutputFormat.Jpeg
    };

    /// <summary>Opsi preset untuk dokumen/attachment standar.</summary>
    public static ImageCompressionOptions Standard => new()
    {
        Quality = 80,
        MaxWidthPx = 1280,
        MaxHeightPx = 1280,
        OutputFormat = ImageOutputFormat.Jpeg
    };
}

/// <summary>Format gambar output.</summary>
public enum ImageOutputFormat
{
    Jpeg,
    Png,
    WebP
}

/// <summary>
/// Hasil kompresi gambar.
/// </summary>
public sealed class ImageCompressionResult
{
    public byte[] CompressedData { get; init; } = Array.Empty<byte>();
    public int OriginalSizeBytes { get; init; }
    public int CompressedSizeBytes { get; init; }
    public int OriginalWidthPx { get; init; }
    public int OriginalHeightPx { get; init; }
    public int OutputWidthPx { get; init; }
    public int OutputHeightPx { get; init; }
    public string ContentType { get; init; } = "image/jpeg";
    public double CompressionRatio => OriginalSizeBytes > 0
        ? Math.Round((1.0 - (double)CompressedSizeBytes / OriginalSizeBytes) * 100, 1)
        : 0;
}

/// <summary>
/// Service untuk kompresi dan resize gambar sebelum disimpan ke storage.
/// Implementasi menggunakan SixLabors.ImageSharp.
/// </summary>
public interface IImageCompressionService
{
    /// <summary>
    /// Mengecek apakah byte array adalah gambar yang didukung.
    /// </summary>
    bool IsImageFile(byte[] data);

    /// <summary>
    /// Mengecek apakah content-type adalah gambar yang didukung.
    /// </summary>
    bool IsImageContentType(string contentType);

    /// <summary>
    /// Memvalidasi file upload gambar (MIME type + ukuran + dimensi).
    /// </summary>
    Task<(bool IsValid, string? ErrorMessage)> ValidateImageAsync(
        byte[] data,
        string contentType,
        long maxSizeBytes = 5 * 1024 * 1024);

    /// <summary>
    /// Mengompresi dan me-resize gambar sesuai opsi yang diberikan.
    /// Jika data bukan gambar, returns null tanpa error.
    /// </summary>
    Task<ImageCompressionResult?> CompressAsync(
        byte[] imageData,
        ImageCompressionOptions? options = null);

    /// <summary>
    /// Mengompresi gambar dari stream.
    /// </summary>
    Task<ImageCompressionResult?> CompressAsync(
        Stream imageStream,
        ImageCompressionOptions? options = null);
}
