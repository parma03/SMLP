using Smlp.Application.Services.Authorization;
using Smlp.Application.Services.CurrentUser;
using Smlp.Shared.Services.Authorization.Models.GetAuthorizationInfo;

namespace Smlp.Infrastructure.Authorization;

/// <summary>
/// Implementasi lokal IAuthorizationService menggunakan role dari JWT token.
/// Tidak memerlukan server authorization eksternal.
/// </summary>
public class LocalAuthorizationService : IAuthorizationService
{
    private readonly ICurrentUserService _currentUserService;

    public LocalAuthorizationService(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    public Task<GetAuthorizationInfoResponse> GetAuthorizationInfoAsync(string positionId, CancellationToken cancellationToken)
    {
        var role = _currentUserService.Role ?? string.Empty;

        var authInfo = new GetAuthorizationInfoResponse
        {
            Roles = new List<AuthorizationRoleDto>
            {
                new()
                {
                    Name = role,
                    Permissions = GetPermissionsForRole(role)
                }
            }
        };

        return Task.FromResult(authInfo);
    }

    private static IList<string> GetPermissionsForRole(string role)
    {
        return role switch
        {
            "Owner" => new List<string> { "read", "write", "delete", "admin", "report" },
            "Admin" => new List<string> { "read", "write", "delete", "report" },
            "Kasir" => new List<string> { "read", "write" },
            "Customer" => new List<string> { "read" },
            _ => new List<string>()
        };
    }
}
