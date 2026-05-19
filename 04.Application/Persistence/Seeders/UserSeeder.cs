using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Smlp.Application.Services.Persistence;
using static Smlp.Shared.Master.Users.Constants.ApiEndpoint.V1;

namespace Smlp.Application.Services.Persistence.Seeders;
public class UserSeeder
{
    private readonly ILogger<UserSeeder> _logger;
    private readonly ISmlpDbContext _context;

    public UserSeeder(ILogger<UserSeeder> logger, ISmlpDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task SeedUsersAsync()
    {
        foreach (var user in InitialUser.All)
        {
            if (!await _context.Users.AnyAsync(x => x.Id == user.Id))
            {
                _logger.LogInformation("Seeding data {EntityType} {EntityName}...", nameof(Users), user.Name);

                _ = await _context.Users.AddAsync(user);
            }
        }

        _ = await _context.SaveChangesAsync(nameof(UserSeeder));
    }
}

public static class InitialUser
{
    public static readonly Users Admin = new()
    {
        Id = new.guid,
        Name = "tes1",
        Username = "tes1",
        DisplayName = "tes1",
        Role = Smlp.Base.User.Enums.UserRole.Admin,
        PhoneNumber = "081234567890",
        EmailAddress = "",
        IsActive = true
    };
    public static readonly Users[] All = new Users[]
    {

    };
}

