using MediatR;
using Microsoft.EntityFrameworkCore;
using Smlp.Application.Services.Persistence;
using Smlp.Application.Services.Storage;
using Smlp.Application.Common.Exceptions;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Master.Users.Commands.Update;
using Smlp.Domain.Entities.Master;

namespace Smlp.Application.Users.Commands.Update;

public class UpdateUserCommand : IRequest<CommonUpdateResponse>
{
    public Guid Id { get; set; }
    public UpdateUserRequest Request { get; set; } = default!;
}

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, CommonUpdateResponse>
{
    private readonly ISmlpDbContext _context;
    private readonly IStorageService _storageService;

    public UpdateUserCommandHandler(ISmlpDbContext context, IStorageService storageService)
    {
        _context = context;
        _storageService = storageService;
    }

    public async Task<CommonUpdateResponse> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.ProfilePicture)
            .FirstOrDefaultAsync(u => u.Id == request.Id && !u.IsDeleted, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.Id);

        var req = request.Request;
        user.DisplayName = req.DisplayName;
        user.Username = req.Username;
        user.Role = req.Role;
        user.PhoneNumber = req.PhoneNumber;
        user.EmailAddress = req.EmailAddress;
        user.IsActive = req.IsActive;

        // Handle profile picture upload
        if (req.ProfilePictureFile is not null && req.ProfilePictureFile.Length > 0)
        {
            using var ms = new MemoryStream();
            await req.ProfilePictureFile.CopyToAsync(ms, cancellationToken);
            var fileBytes = ms.ToArray();

            var extension = Path.GetExtension(req.ProfilePictureFile.FileName);
            var filename = $"PROFILE_{user.Id}_{DateTime.Now:ddMMyyyyHHmmss}{extension}";

            if (user.ProfilePicture is not null)
            {
                // Hapus foto lama, simpan yang baru
                await _storageService.DeleteAsync(user.ProfilePicture.StorageFileId);
                user.ProfilePicture.FileName = filename;
                user.ProfilePicture.FileContentType = req.ProfilePictureFile.ContentType;
                user.ProfilePicture.FileSize = req.ProfilePictureFile.Length;
                user.ProfilePicture.StorageFileId = await _storageService.CreateAsync(fileBytes);
            }
            else
            {
                // Buat record foto profil baru
                var profilePicture = new UserProfilePicture
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    FileName = filename,
                    FileContentType = req.ProfilePictureFile.ContentType,
                    FileSize = req.ProfilePictureFile.Length,
                    StorageFileId = await _storageService.CreateAsync(fileBytes)
                };
                await _context.UserProfilePictures.AddAsync(profilePicture, cancellationToken);
                user.ProfilePictureId = profilePicture.Id;
            }
        }

        await _context.SaveChangesAsync(this, cancellationToken);
        return new CommonUpdateResponse { Id = user.Id };
    }
}
