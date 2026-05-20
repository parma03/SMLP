using RestSharp;
using Smlp.Client.Common.Extensions;
using Smlp.Shared.Auth.Requests;
using Smlp.Shared.Auth.Responses;
using Smlp.Shared.Common.Responses;

namespace Smlp.Client.Services.BackEnd;

public class AuthService : IAuthService
{
    private readonly RestClient _client;

    public AuthService(RestClient client)
    {
        _client = client;
    }

    public async Task<Response<LoginResponse>> LoginAsync(LoginRequest request)
    {
        var restRequest = new RestRequest("api/v1/auth/login", Method.Post);
        restRequest.AddJsonBody(request);
        var response = await _client.ExecuteAsync<LoginResponse>(restRequest);
        return response.ToResponse<LoginResponse>();
    }

    public async Task<Response<object>> LogoutAsync(string accessToken)
    {
        var restRequest = new RestRequest("api/v1/auth/logout", Method.Post);
        restRequest.AddHeader("Authorization", $"Bearer {accessToken}");
        var response = await _client.ExecuteAsync(restRequest);
        return response.ToResponse<object>();
    }

    public async Task<Response<object>> RefreshTokenAsync(string refreshToken)
    {
        var restRequest = new RestRequest("api/v1/auth/refresh", Method.Post);
        restRequest.AddJsonBody(new RefreshTokenRequest { RefreshToken = refreshToken });
        var response = await _client.ExecuteAsync(restRequest);
        return response.ToResponse<object>();
    }
}
