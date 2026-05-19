using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Smlp.Bsui.Services.Authorization.IdAMan.Models.GetAuhtorizationInfo;
using Smlp.Bsui.Services.Authorization.IdAMan.Models.GetPositions;
using Smlp.Shared.Common.Constants;
using Smlp.Shared.Services.Authorization.Constants;
using Smlp.Shared.Services.Authorization.Models.GetAuthorizationInfo;
using Smlp.Shared.Services.Authorization.Models.GetPositions;
using RestSharp;

namespace Smlp.Bsui.Services.Authorization.IdAMan;

public class IdAManAuthorizationService : IAuthorizationService
{
    private readonly IdAManAuthorizationOptions _idAManAuthorizationOptions;
    private readonly RestClient _restClient;

    public IdAManAuthorizationService(IOptions<IdAManAuthorizationOptions> idAManAuthorizationOptions)
    {
        _idAManAuthorizationOptions = idAManAuthorizationOptions.Value;
        _restClient = new RestClient(_idAManAuthorizationOptions.BaseUrl);
    }

    public async Task<GetPositionsResponse> GetPositionsAsync(string username, string accessToken, CancellationToken cancellationToken)
    {
        var restRequest = new RestRequest($"{_idAManAuthorizationOptions.Endpoints.Positions}/{username}", Method.Get);

        restRequest.AddHeader(HttpHeaderName.Authorization, $"{JwtBearerDefaults.AuthenticationScheme} {accessToken}");

        try
        {
            var restResponse = await _restClient.ExecuteAsync<GetPositionsIdAManPersona[]>(restRequest, cancellationToken);

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
                throw new Exception($"Failed to deserialize JSON content {restResponse.Content} into {nameof(GetPositionsIdAManPersona)}.");
            }

            var getPositionsResponse = new GetPositionsResponse();

            foreach (var persona in restResponse.Data)
            {
                foreach (var position in persona.Positions)
                {
                    if (!getPositionsResponse.Positions.Any(x => x.Id == position.Id))
                    {
                        getPositionsResponse.Positions.Add(new GetPositionsPosition
                        {
                            Id = position.Id,
                            Name = position.Name,
                            PersonaType = persona.Type
                        });
                    }
                }
            }

            return getPositionsResponse;
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<GetAuthorizationInfoResponse> GetAuthorizationInfoAsync(string positionId, string accessToken, CancellationToken cancellationToken)
    {
        var restRequest = new RestRequest($"{_idAManAuthorizationOptions.Endpoints.AuthorizationInfo}/{positionId}?applicationId={_idAManAuthorizationOptions.ObjectId}", Method.Get);

        restRequest.AddHeader(HttpHeaderName.Authorization, $"{JwtBearerDefaults.AuthenticationScheme} {accessToken}");

        try
        {
            var restResponse = await _restClient.ExecuteAsync<GetAuhtorizationInfoIdAMan[]>(restRequest, cancellationToken);

            if (!restResponse.IsSuccessful)
            {
                if (restResponse.ErrorException is null)
                {
                    throw new Exception($"Failed to retreive {AuthorizationDisplayTextFor.AuthorizationInfo} from {_restClient.BuildUri(restRequest)}");
                }

                throw restResponse.ErrorException;
            }

            if (restResponse.Data is null)
            {
                throw new Exception($"Failed to deserialize JSON content {restResponse.Content} into {nameof(GetAuhtorizationInfoIdAMan)}.");
            }

            var authorizationInfo = new GetAuthorizationInfoResponse();

            if (restResponse.Data.Any())
            {
                var idAManApplication = restResponse.Data.First();

                foreach (var role in idAManApplication.Roles)
                {
                    var authorizationInfoRole = new GetAuthorizationInfoRole
                    {
                        Name = role.Name
                    };

                    if (role.Permissions is not null)
                    {
                        foreach (var permission in role.Permissions)
                        {
                            authorizationInfoRole.Permissions.Add(permission);
                        }
                    }

                    authorizationInfo.Roles.Add(authorizationInfoRole);
                }

                foreach (var customParameter in idAManApplication.CustomParameters.SelectMany(x => x).ToList())
                {
                    authorizationInfo.CustomParameters.Add(new GetAuthorizationInfoCustomParameter
                    {
                        Key = customParameter.Key,
                        Value = customParameter.Value
                    });
                }
            }

            return authorizationInfo;
        }
        catch (Exception)
        {
            throw;
        }
    }
}
