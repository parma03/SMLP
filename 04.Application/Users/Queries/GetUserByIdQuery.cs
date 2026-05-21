using MediatR;
using Microsoft.EntityFrameworkCore;
using Smlp.Application.Services.Persistence;
using Smlp.Application.Common.Exceptions;
using Smlp.Shared.Master.Users.Queries;
using Smlp.Domain.Entities.Master;

namespace Smlp.Application.Users.Queries;

public class GetUserByIdQuery : IRequest<GetUserResponse>
{
    public Guid Id { get; set; }
}

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, GetUserResponse>
{
    private readonly ISmlpDbContext _context;

    public GetUserByIdQueryHandler(ISmlpDbContext context) => _context = context;

    public async Task<GetUserResponse> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.ProfilePicture)
            .Where(u => u.Id == request.Id && !u.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.Id);

        return MapToResponse(user);
    }

    public static GetUserResponse MapToResponse(User u) => new()
    {
        Id = u.Id,
        Username = u.Username,
        DisplayName = u.DisplayName,
        Role = u.Role,
        PhoneNumber = u.PhoneNumber,
        EmailAddress = u.EmailAddress,
        IsActive = u.IsActive,
        ProfilePicture = u.ProfilePicture is null ? null : new ProfilePictureDto
        {
            Id = u.ProfilePicture.Id,
            FileName = u.ProfilePicture.FileName,
            FileContentType = u.ProfilePicture.FileContentType,
            FileSize = u.ProfilePicture.FileSize
        }
    };
}
