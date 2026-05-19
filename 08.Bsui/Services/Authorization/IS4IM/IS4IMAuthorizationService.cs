using Microsoft.Extensions.Options;
using Smlp.Shared.Services.Authorization.Constants;
using Smlp.Shared.Services.Authorization.Models.GetAuthorizationInfo;
using Smlp.Shared.Services.Authorization.Models.GetPositions;
using RestSharp;

namespace Smlp.Bsui.Services.Authorization.IS4IM;

public class IS4IMAuthorizationService : IAuthorizationService
{
    private readonly IS4IMAuthorizationOptions _is4imAuthorizationOptions;
    private readonly RestClient _restClient;

    public IS4IMAuthorizationService(IOptions<IS4IMAuthorizationOptions> is4imAuthorizationOptions)
    {
        _is4imAuthorizationOptions = is4imAuthorizationOptions.Value;
        _restClient = new RestClient(is4imAuthorizationOptions.Value.BaseUrl);
    }

    public async Task<GetPositionsResponse> GetPositionsAsync(string username, string accessToken, CancellationToken cancellationToken)
    {
        var restRequest = new RestRequest($"{_is4imAuthorizationOptions.Endpoints.Positions}/{username}", Method.Get);

        try
        {
            var restResponse = await _restClient.ExecuteAsync<GetPositionsResponse>(restRequest, cancellationToken);

            if (!restResponse.IsSuccessful)
            {
                if (restResponse.ErrorException is null)
                {
                    throw new Exception($"Failed to retreive {AuthorizationDisplayTextFor.Positions} from {_restClient.BuildUri(restRequest)}");
                }

                throw restResponse.ErrorException;
            }

            if (restResponse.Data is null)
            {
                throw new Exception($"Failed to deserialize JSON content {restResponse.Content} into {nameof(GetPositionsResponse)}.");
            }

            return restResponse.Data;
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<GetAuthorizationInfoResponse> GetAuthorizationInfoAsync(string positionId, string accessToken, CancellationToken cancellationToken)
    {
        var restRequest = new RestRequest($"{_is4imAuthorizationOptions.Endpoints.AuthorizationInfo}/{positionId}", Method.Get);

        try
        {
            var restResponse = await _restClient.ExecuteAsync<GetAuthorizationInfoResponse>(restRequest, cancellationToken);

            if (!restResponse.IsSuccessful)
            {
                if (restResponse.ErrorException is null)
                {
                    throw new Exception($"Failed to retreive {AuthorizationDisplayTextFor.AuthorizationInfo} info from {_restClient.BuildUri(restRequest)}");
                }

                throw restResponse.ErrorException;
            }

            if (restResponse.Data is null)
            {
                throw new Exception($"Failed to deserialize JSON content {restResponse.Content} into {nameof(GetAuthorizationInfoResponse)}.");
            }

            return restResponse.Data;
        }
        catch (Exception)
        {
            throw;
        }
    }
}
