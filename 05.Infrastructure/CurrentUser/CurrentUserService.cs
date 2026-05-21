using System.Security.Claims;
using IdentityModel;
using Microsoft.AspNetCore.Http;
using Smlp.Application.Services.CurrentUser;
using Smlp.Base.ValueObjects;
using Smlp.Shared.Common.Constants;
using Smlp.Shared.Services.Authorization.Constants;

namespace Smlp.InfrastructureCurrentUser;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            if (_httpContextAccessor.HttpContext is null) return null;
            var subject = _httpContextAccessor.HttpContext.User.FindFirstValue(JwtClaimTypes.Subject)
                ?? _httpContextAccessor.HttpContext.User.FindFirst(x => x.Type.Contains(AuthorizationClaimTypes.NameIdentifier))?.Value;
            return string.IsNullOrWhiteSpace(subject) ? null : new Guid(subject);
        }
    }

    public string Username
    {
        get
        {
            if (_httpContextAccessor.HttpContext is null) return DefaultTextFor.SystemBackgroundJob;
            return _httpContextAccessor.HttpContext.User.FindFirstValue(JwtClaimTypes.Email)
                ?? _httpContextAccessor.HttpContext.User.FindFirst(x => x.Type.Contains(JwtClaimTypes.Email))?.Value
                ?? DefaultTextFor.Unknown;
        }
    }

    public string? PositionId =>
        _httpContextAccessor.HttpContext?.Request.Headers[HttpHeaderName.PositionId].FirstOrDefault();

    public string? Role
    {
        get
        {
            if (_httpContextAccessor.HttpContext is null) return null;
            return _httpContextAccessor.HttpContext.User.FindFirstValue(AuthorizationClaimTypes.Role)
                ?? _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Role);
        }
    }

    public string ClientId
    {
        get
        {
            if (_httpContextAccessor.HttpContext is null) return DefaultTextFor.SystemBackgroundJob;
            return _httpContextAccessor.HttpContext.User.FindFirstValue(JwtClaimTypes.ClientId) ?? DefaultTextFor.Unknown;
        }
    }

    public string IpAddress
    {
        get
        {
            if (_httpContextAccessor.HttpContext is null) return DefaultTextFor.SystemBackgroundJob;
            var ipAddress = _httpContextAccessor.HttpContext.Request.Headers[HttpHeaderName.IpAddress].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(ipAddress)) return ipAddress;
            var remoteIp = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress;
            return remoteIp?.ToString() ?? DefaultTextFor.Unknown;
        }
    }

    public Geolocation? Geolocation
    {
        get
        {
            var geolocationText = _httpContextAccessor.HttpContext?.Request.Headers[HttpHeaderName.Geolocation].FirstOrDefault();
            return string.IsNullOrWhiteSpace(geolocationText) ? null : Geolocation.From(geolocationText);
        }
    }

    public IList<string> Permissions
    {
        get
        {
            if (_httpContextAccessor.HttpContext is null) return new List<string>();
            return _httpContextAccessor.HttpContext.User.FindAll(AuthorizationClaimTypes.Permission)
                .Select(x => x.Value).ToList();
        }
    }
}
