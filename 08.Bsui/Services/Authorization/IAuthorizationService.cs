/*
* Sistem Manajemen Loyalitas Pelanggan (SMLP)
*/

using Smlp.Shared.Services.Authorization.Models.GetAuthorizationInfo;
using Smlp.Shared.Services.Authorization.Models.GetPositions;

namespace Smlp.Bsui.Services.Authorization;

/// <summary>
/// Authorization service interface for SMLP.
/// Since SMLP uses JWT-based role authorization (no external provider),
/// this interface is kept for compatibility but uses no-op implementation.
/// </summary>
public interface IAuthorizationService
{
    public Task<GetPositionsResponse> GetPositionsAsync(string username, string accessToken, CancellationToken cancellationToken = default);
    public Task<GetAuthorizationInfoResponse> GetAuthorizationInfoAsync(string positionId, string accessToken, CancellationToken cancellationToken = default);
}
