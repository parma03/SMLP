/*
* Sistem Manajemen Loyalitas Pelanggan (SMLP)
*/

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Smlp.Application.Services.Persistence;
using Smlp.Application.Services.Storage;

namespace Smlp.WebApi.Areas.V1.Controllers.Master;

[ApiVersion(ApiVersioning.V1.Number)]
[Route("api/v{version:apiVersion}/users/{userId:guid}/profile-picture")]
public class ProfilePictureController : ControllerBase
{
    private readonly ISmlpDbContext _context;
    private readonly IStorageService _storageService;

    public ProfilePictureController(ISmlpDbContext context, IStorageService storageService)
    {
        _context = context;
        _storageService = storageService;
    }

    /// <summary>Mengambil file foto profil user.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProfilePicture([FromRoute] Guid userId, CancellationToken cancellationToken)
    {
        var profilePicture = await _context.UserProfilePictures
            .Where(p => p.UserId == userId && !p.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (profilePicture is null)
            return NotFound(new { message = "Foto profil tidak ditemukan." });

        var fileBytes = await _storageService.ReadAsync(profilePicture.StorageFileId);
        return File(fileBytes, profilePicture.FileContentType, profilePicture.FileName);
    }

    /// <summary>Menghapus foto profil user.</summary>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProfilePicture([FromRoute] Guid userId, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.ProfilePicture)
            .Where(u => u.Id == userId && !u.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
            return NotFound(new { message = "User tidak ditemukan." });

        if (user.ProfilePicture is null)
            return NotFound(new { message = "Foto profil tidak ditemukan." });

        await _storageService.DeleteAsync(user.ProfilePicture.StorageFileId);
        _context.UserProfilePictures.Remove(user.ProfilePicture);
        user.ProfilePictureId = null;

        await _context.SaveChangesAsync(this, cancellationToken);
        return Ok(new { message = "Foto profil berhasil dihapus." });
    }
}
