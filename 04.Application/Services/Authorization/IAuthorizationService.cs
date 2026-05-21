using Smlp.Shared.Services.Authorization.Models.GetAuthorizationInfo;

namespace Smlp.Application.Services.Authorization;

public interface IAuthorizationService
{
    Task<GetAuthorizationInfoResponse> GetAuthorizationInfoAsync(string positionId, CancellationToken cancellationToken);
}
