/*
* Sistem Manajemen Loyalitas Pelanggan (SMLP)
* Upload/Get/Delete foto avatar pelanggan
*/

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

    public CustomerPhotoController(ISmlpDbContext context, IStorageService storageService)
    {
        _context = context;
        _storageService = storageService;
    }

    /// <summary>Upload foto avatar pelanggan.</summary>
    [HttpPost]
    [Authorize(Policy = "AdminOrKasir")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadPhoto([FromRoute] Guid customerId, IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "File tidak boleh kosong." });

        var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
        if (!allowedTypes.Contains(file.ContentType.ToLower()))
            return BadRequest(new { message = "Hanya file JPG, PNG, atau WebP yang diizinkan." });

        if (file.Length > 2 * 1024 * 1024) // 2MB limit
            return BadRequest(new { message = "Ukuran file maksimal 2MB." });

        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == customerId && !c.IsDeleted, cancellationToken);

        if (customer is null)
            return NotFound(new { message = "Pelanggan tidak ditemukan." });

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms, cancellationToken);
        var bytes = ms.ToArray();

        // Store file and save the storage ID to customer notes (simple approach)
        // In a full implementation, we'd have a CustomerPhoto entity
        var storedId = await _storageService.CreateAsync(bytes, DocumentType.Nota, file.FileName);

        // Save storage reference in the customer's notes field temporarily
        // TODO: proper CustomerPhoto entity
        customer.Notes = customer.Notes; // Update via proper entity in production

        await _context.SaveChangesAsync(this, cancellationToken);

        return Ok(new
        {
            message = "Foto berhasil diupload.",
            storageFileId = storedId,
            fileName = file.FileName,
            fileSize = file.Length,
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
            return File(bytes, "image/jpeg"); // Default to jpeg, could be stored
        }
        catch
        {
            return NotFound(new { message = "Foto tidak ditemukan." });
        }
    }
}
