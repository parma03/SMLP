using Smlp.Shared.Auth.Requests;
using Smlp.Shared.Auth.Responses;
using Smlp.Shared.Common.Responses;

namespace Smlp.Client.Services.BackEnd;

public interface IAuthService
{
    Task<ResponseResult<LoginResponse>> LoginAsync(LoginRequest request);
    Task<ResponseResult<SuccessResponse>> LogoutAsync();
    Task<ResponseResult<RefreshTokenResponse>> RefreshTokenAsync(RefreshTokenRequest request);
}
