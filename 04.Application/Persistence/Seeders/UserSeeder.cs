/*
 * Sistem Manajemen Loyalitas Pelanggan (SMLP)
 */

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Smlp.Application.Services.Persistence;

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
        Id = new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
        Name = "Admin SMLP",
        Username = "admin",
        DisplayName = "Administrator",
        Role = Smlp.Base.User.Enums.UserRole.Admin,
        PhoneNumber = "08100000000",
        EmailAddress = "admin@smlp.local",
        IsActive = true
    };

    public static readonly Users[] All =
    {
        Admin
    };
}
