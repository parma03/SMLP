/*
 * Sistem Manajemen Loyalitas Pelanggan (SMLP)
 * Copyright (c) 2025-2026
 */

using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
                _logger.LogInformation("Seeding user {Username} ({Role})...", user.Username, user.Role);
                await _context.Users.AddAsync(user);
            }
            else if (string.IsNullOrWhiteSpace(existing.PasswordHash))
            {
                existing.PasswordHash = user.PasswordHash;
                _logger.LogInformation("Updating PasswordHash for user {Username}...", user.Username);
            }
        }

        await _context.SaveChangesAsync(nameof(UserSeeder));
    }
}

public static class InitialUser
{
    // Default password untuk semua akun seed: Admin@123
    // SHA256 hash dari "Admin@123"
    private static readonly string _defaultPasswordHash = HashPassword("Admin@123");

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
        PhoneNumber = "081000000000",
        EmailAddress = "admin@smlp.app",
        IsActive = true,
        PasswordHash = _defaultPasswordHash
    };

    public static readonly User Owner = new()
    {
        Id = new Guid("b2c3d4e5-f6a7-8901-bcde-f12345678901"),
        Username = "owner",
        DisplayName = "Owner Coffee Shop",
        Role = UserRole.Owner,
        PhoneNumber = "081000000001",
        EmailAddress = "owner@smlp.app",
        IsActive = true,
        PasswordHash = _defaultPasswordHash
    };

    public static readonly User Kasir = new()
    {
        Id = new Guid("c3d4e5f6-a7b8-9012-cdef-123456789012"),
        Username = "kasir",
        DisplayName = "Kasir",
        Role = UserRole.Kasir,
        PhoneNumber = "081000000002",
        EmailAddress = "kasir@smlp.app",
        IsActive = true,
        PasswordHash = _defaultPasswordHash
    };

    public static readonly User[] All = [Admin, Owner, Kasir];
}
