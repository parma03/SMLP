using System.Security.Cryptography;
using System.Text;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Smlp.Application.Common.Exceptions;
using Smlp.Application.Services.Persistence;
using Smlp.Domain.Entities.Master;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Master.Users.Commands.Create;

namespace Smlp.Application.Users.Commands.Create;

public class CreateUserCommand : IRequest<CommonCreateResponse>
{
    public CreateUserRequest Request { get; set; } = default!;
}

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, CommonCreateResponse>
{
    private readonly ISmlpDbContext _context;

    public CreateUserCommandHandler(ISmlpDbContext context)
    {
        _context = context;
    }

    public async Task<CommonCreateResponse> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var req = request.Request;

        var exists = await _context.Users.AnyAsync(u => u.Username == req.Username && !u.IsDeleted, cancellationToken);
        if (exists)
            throw new AlreadyExistsExceptions(nameof(User), req.Username);

        var user = new User
        {
            Username = req.Username,
            DisplayName = req.DisplayName,
            Role = req.Role,
            PhoneNumber = req.PhoneNumber,
            EmailAddress = req.EmailAddress,
            IsActive = req.IsActive,
            PasswordHash = HashPassword("Admin@123")
        };

        await _context.Users.AddAsync(user, cancellationToken);
        await _context.SaveChangesAsync(this, cancellationToken);

        return new CommonCreateResponse { Id = user.Id };
    }

    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes).ToLower();
    }
}
