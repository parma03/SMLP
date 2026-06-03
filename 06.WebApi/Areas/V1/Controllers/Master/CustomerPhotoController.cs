/*
* Sistem Manajemen Loyalitas Pelanggan (SMLP)
* Upload/Get/Delete foto avatar pelanggan — dengan image compression
*/

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Smlp.Application.Services.Image;
using Smlp.Application.Services.Persistence;
using Smlp.Application.Services.Storage;
using Smlp.Base.Selections.Enums;

namespace Smlp.WebApi.Areas.V1.Controllers.Master;

[ApiVersion(ApiVersioning.V1.Number)]
[Route("api/v{version:apiVersion}/customers/{customerId:guid}/photo")]
[Authorize]
public class CustomerPhotoController : ControllerBase
{
    private readonly ISmlpDbContext _context;
    private readonly IStorageService _storageService;
    private readonly IImageCompressionService _imageCompression;
    private readonly ILogger<CustomerPhotoController> _logger;

    public CustomerPhotoController(
        ISmlpDbContext context,
        IStorageService storageService,
        IImageCompressionService imageCompression,
        ILogger<CustomerPhotoController> logger)
    {
        _context = context;
        _storageService = storageService;
        _imageCompression = imageCompression;
        _logger = logger;
    }

    /// <summary>Upload foto avatar pelanggan (auto-compress & resize menjadi max 256x256px).</summary>
    [HttpPost]
    [Authorize(Policy = "AdminOrKasir")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadPhoto(
        [FromRoute] Guid customerId,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "File tidak boleh kosong." });

        // Validasi MIME type
        if (!_imageCompression.IsImageContentType(file.ContentType))
            return BadRequest(new { message = "Hanya file gambar (JPEG, PNG, WebP) yang diizinkan." });

        // Validasi ukuran maksimum (2MB sebelum kompresi)
        if (file.Length > 5 * 1024 * 1024)
            return BadRequest(new { message = "Ukuran file maksimal 5MB." });

        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == customerId && !c.IsDeleted, cancellationToken);

        if (customer is null)
            return NotFound(new { message = "Pelanggan tidak ditemukan." });

        // Baca file ke bytes
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms, cancellationToken);
        var originalBytes = ms.ToArray();

        // Validasi mendalam
        var (isValid, errorMessage) = await _imageCompression.ValidateImageAsync(
            originalBytes, file.ContentType, maxSizeBytes: 5 * 1024 * 1024);

        if (!isValid)
            return BadRequest(new { message = errorMessage });

        // Kompresi & resize ke avatar (max 256x256, JPEG quality 85)
        var compressionResult = await _imageCompression.CompressAsync(originalBytes, ImageCompressionOptions.Avatar);

        byte[] bytesToStore;
        string contentTypeToStore;
        string fileNameToStore;

        if (compressionResult is not null)
        {
            bytesToStore = compressionResult.CompressedData;
            contentTypeToStore = compressionResult.ContentType;
            fileNameToStore = Path.GetFileNameWithoutExtension(file.FileName) + ".jpg";
            _logger.LogInformation(
                "Foto pelanggan {CustomerId} dikompres: {OrigKb}KB→{CompKb}KB ({Ratio}% lebih kecil), {OrigW}x{OrigH}→{OutW}x{OutH}px",
                customerId,
                compressionResult.OriginalSizeBytes / 1024,
                compressionResult.CompressedSizeBytes / 1024,
                compressionResult.CompressionRatio,
                compressionResult.OriginalWidthPx, compressionResult.OriginalHeightPx,
                compressionResult.OutputWidthPx, compressionResult.OutputHeightPx);
        }
        else
        {
            // Fallback: simpan tanpa kompresi
            bytesToStore = originalBytes;
            contentTypeToStore = file.ContentType;
            fileNameToStore = file.FileName;
        }

        // Simpan ke storage
        var storedId = await _storageService.CreateAsync(bytesToStore, DocumentType.Nota, fileNameToStore);

        await _context.SaveChangesAsync(this, cancellationToken);

        return Ok(new
        {
            message = "Foto berhasil diupload dan dikompres.",
            storageFileId = storedId,
            fileName = fileNameToStore,
            originalSizeKb = originalBytes.Length / 1024,
            compressedSizeKb = bytesToStore.Length / 1024,
            compressionRatioPercent = compressionResult?.CompressionRatio ?? 0,
            outputWidth = compressionResult?.OutputWidthPx,
            outputHeight = compressionResult?.OutputHeightPx,
            customerId = customerId
        });
    }

    /// <summary>Ambil foto avatar pelanggan.</summary>
    [HttpGet("{storageFileId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPhoto([FromRoute] string storageFileId)
    {
        try
        {
            var bytes = await _storageService.ReadAsync(storageFileId, DocumentType.Nota);
            return File(bytes, "image/jpeg");
        }
        catch
        {
            return NotFound(new { message = "Foto tidak ditemukan." });
        }
    }
}
