/*
 * Sistem Manajemen Loyalitas Pelanggan (SMLP)
 */

using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Smlp.Application.Services.Persistence;
using Smlp.Base.User.Enums;
using Smlp.Domain.Entities.Master;

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
            var existing = await _context.Users.FirstOrDefaultAsync(x => x.Username == user.Username);
            if (existing is null)
            {
                _logger.LogInformation("Seeding data User {Username}...", user.Username);
                await _context.Users.AddAsync(user);
            }
            else if (string.IsNullOrWhiteSpace(existing.PasswordHash))
            {
                // Tambahkan password hash jika belum ada (untuk migrasi dari data lama)
                existing.PasswordHash = user.PasswordHash;
                _logger.LogInformation("Updating PasswordHash for User {Username}...", user.Username);
            }
        }

        await _context.SaveChangesAsync(nameof(UserSeeder));
    }

    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes).ToLower();
    }
}

public static class InitialUser
{
    // Default password: Admin@123
    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes).ToLower();
    }

    public static readonly User Admin = new()
    {
        Id = new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
        Username = "admin",
        DisplayName = "Administrator",
        Role = UserRole.Admin,
        PhoneNumber = "08100000000",
        EmailAddress = "admin@smlp.local",
        IsActive = true,
        PasswordHash = "24c9e15e52afc47c225b757e7bee1f9d2a3d35a1a9b2c4f8" // Admin@123
    };

    public static readonly User Owner = new()
    {
        Id = new Guid("b2c3d4e5-f6a7-8901-bcde-f12345678901"),
        Username = "owner",
        DisplayName = "Owner",
        Role = UserRole.Owner,
        PhoneNumber = "08100000001",
        EmailAddress = "owner@smlp.local",
        IsActive = true,
        PasswordHash = "24c9e15e52afc47c225b757e7bee1f9d2a3d35a1a9b2c4f8" // Admin@123
    };

    public static readonly User Kasir = new()
    {
        Id = new Guid("c3d4e5f6-a7b8-9012-cdef-123456789012"),
        Username = "kasir",
        DisplayName = "Kasir",
        Role = UserRole.Admin, // Kasir menggunakan role Admin untuk akses kasir
        PhoneNumber = "08100000002",
        EmailAddress = "kasir@smlp.local",
        IsActive = true,
        PasswordHash = "24c9e15e52afc47c225b757e7bee1f9d2a3d35a1a9b2c4f8" // Admin@123
    };

    public static readonly User[] All = { Admin, Owner, Kasir };
}
