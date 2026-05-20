/*
* Sistem Manajemen Loyalitas Pelanggan (SMLP)
*/

using Smlp.Shared.Common.Constants;
using Smlp.Shared.Services.Authorization.Models.GetAuthorizationInfo;
using Smlp.Shared.Services.Authorization.Models.GetPositions;

namespace Smlp.Bsui.Services.Authorization.None;

/// <summary>
/// SMLP menggunakan JWT Role-based authorization langsung.
/// Service ini berfungsi sebagai no-op untuk kompatibilitas framework.
/// </summary>
public class NoneAuthorizationService : IAuthorizationService
{
    private readonly ILogger<NoneAuthorizationService> _logger;

    public NoneAuthorizationService(ILogger<NoneAuthorizationService> logger)
    {
        _logger = logger;
    }

    public Task<GetAuthorizationInfoResponse> GetAuthorizationInfoAsync(string positionId, string accessToken, CancellationToken cancellationToken = default)
    {
        // SMLP: Auth info diambil langsung dari JWT claims (role-based)
        return Task.FromResult(new GetAuthorizationInfoResponse());
    }

    public Task<GetPositionsResponse> GetPositionsAsync(string username, string accessToken, CancellationToken cancellationToken = default)
    {
        // SMLP: Tidak menggunakan position/jabatan — menggunakan UserRole enum
        return Task.FromResult(new GetPositionsResponse());
    }
}
