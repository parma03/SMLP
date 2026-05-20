using Microsoft.Extensions.Options;
using Smlp.Client.Common.Extensions;
using Smlp.Client.Services.UserInfo;
using Smlp.Shared.Auth.Requests;
using Smlp.Shared.Auth.Responses;
using Smlp.Shared.Common.Responses;
using RestSharp;
using static Smlp.Shared.Auth.Constants.ApiEndpoint.V1;

namespace Smlp.Client.Services.BackEnd.Auth;

public class AuthService
{
    private readonly RestClient _restClient;

    public AuthService(IOptions<BackEndOptions> backEndServiceOptions, UserInfoService userInfo)
    {
        var options = new RestClientOptions(new Uri($"{backEndServiceOptions.Value.BaseUrl}/{Auths.Segment}"))
        {
            RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true
        };
        _restClient = new RestClient(options);
        _restClient.AddUserInfo(userInfo);
    }

    public async Task<ResponseResult<LoginResponse>> LoginAsync(LoginRequest request)
    {
        var restRequest = new RestRequest($"{nameof(Auths.RouteTemplateFor.Login)}", Method.Post);
        restRequest.AddParameters(request);

        var restResponse = await _restClient.ExecuteAsync(restRequest);

        return restResponse.ToResponseResult<LoginResponse>();
    }

    public async Task<ResponseResult<RefreshTokenResponse>> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var restRequest = new RestRequest($"{nameof(Auths.RouteTemplateFor.RefreshToken)}", Method.Post);
        restRequest.AddParameters(request);

        var restResponse = await _restClient.ExecuteAsync(restRequest);

        return restResponse.ToResponseResult<RefreshTokenResponse>();
    }

    public async Task<ResponseResult<SuccessResponse>> LogoutAsync()
    {
        var restRequest = new RestRequest($"{nameof(Auths.RouteTemplateFor.Logout)}", Method.Post);

        var restResponse = await _restClient.ExecuteAsync(restRequest);

        return restResponse.ToResponseResult<SuccessResponse>();
    }
}
