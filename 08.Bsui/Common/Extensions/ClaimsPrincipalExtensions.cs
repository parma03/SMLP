/*
* Sistem Manajemen Loyalitas Pelanggan (SMLP)
*/

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Smlp.Bsui.Services.Authentication.Constants;
using Smlp.Shared.Services.Authorization.Constants;

namespace Smlp.Bsui.Common.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid? GetUserId(this ClaimsPrincipal user)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (string.IsNullOrWhiteSpace(userId))
            return null;

        return Guid.TryParse(userId, out var id) ? id : null;
    }

    public static string? GetUsername(this ClaimsPrincipal user)
    {
        return user.FindFirst("username")?.Value
            ?? user.FindFirst(JwtRegisteredClaimNames.Email)?.Value
            ?? user.FindFirst(ClaimTypes.Name)?.Value;
    }

    public static string? GetEmail(this ClaimsPrincipal user)
    {
        return user.FindFirst(JwtRegisteredClaimNames.Email)?.Value
            ?? user.FindFirst(ClaimTypes.Email)?.Value;
    }

    public static string? GetDisplayName(this ClaimsPrincipal user)
    {
        return user.FindFirst(CustomClaimTypes.DisplayName)?.Value
            ?? user.FindFirst(JwtRegisteredClaimNames.Name)?.Value
            ?? user.FindFirst(ClaimTypes.Name)?.Value;
    }

    public static string? GetRole(this ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.Role)?.Value
            ?? user.FindFirst("role")?.Value;
    }

    public static IEnumerable<string> GetRoles(this ClaimsPrincipal user)
    {
        return user.Claims
            .Where(x => x.Type == ClaimTypes.Role || x.Type == "role" || x.Type == AuthorizationClaimTypes.Role)
            .Select(x => x.Value);
    }

    public static IEnumerable<string> GetPermissions(this ClaimsPrincipal user)
    {
        return user.Claims
            .Where(x => x.Type == AuthorizationClaimTypes.Permission)
            .Select(x => x.Value);
    }

    public static bool HasPermission(this ClaimsPrincipal user, string permission)
    {
        return user.Claims.Any(x => x.Type == AuthorizationClaimTypes.Permission && x.Value == permission);
    }

    public static bool HasPermission(this ClaimsPrincipal user, string[] permissions)
    {
        return user.Claims.Any(x => x.Type == AuthorizationClaimTypes.Permission && permissions.Contains(x.Value));
    }

    public static bool IsAdmin(this ClaimsPrincipal user)
        => user.IsInRole("Admin");

    public static bool IsOwner(this ClaimsPrincipal user)
        => user.IsInRole("Owner");

    public static bool IsKasir(this ClaimsPrincipal user)
        => user.IsInRole("Kasir") || user.IsInRole("Admin");

    public static bool IsCustomer(this ClaimsPrincipal user)
        => user.IsInRole("Customer");

    public static DateTimeOffset? GetAuthenticationTime(this ClaimsPrincipal user)
    {
        var authTime = user.FindFirst(JwtRegisteredClaimNames.Iat)?.Value;
        if (string.IsNullOrWhiteSpace(authTime) || !long.TryParse(authTime, out var unixTime))
            return null;
        return DateTimeOffset.FromUnixTimeSeconds(unixTime);
    }
}
