using Smlp.Shared.Auth.Requests;
using Smlp.Shared.Auth.Responses;
using Smlp.Shared.Common.Responses;

namespace Smlp.Client.Services.BackEnd;

public interface IAuthService
{
    Task<Response<LoginResponse>> LoginAsync(LoginRequest request);
    Task<Response<object>> LogoutAsync(string accessToken);
    Task<Response<object>> RefreshTokenAsync(string refreshToken);
}
