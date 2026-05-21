/*
* Sistem Manajemen Loyalitas Pelanggan (SMLP)
*/

using Microsoft.Extensions.Options;
using RestSharp;
using Smlp.Client.Common.Extensions;
using Smlp.Client.Services.UserInfo;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Master.Users.Commands.Create;
using Smlp.Shared.Master.Users.Commands.Update;
using Smlp.Shared.Master.Users.Queries;

namespace Smlp.Client.Services.BackEnd.Masters;

public class UsersService : IDisposable
{
    private readonly RestClient _restClient = default!;

    public UsersService(IOptions<BackEndOptions> backEndServiceOptions, UserInfoService userInfo)
    {
        if (backEndServiceOptions is null) return;

        var options = new RestClientOptions(new Uri($"{backEndServiceOptions!.Value.BaseUrl}/V1/Users"))
        {
            RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true
        };
        _restClient = new RestClient(options);
        _restClient.AddUserInfo(userInfo);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        _restClient?.Dispose();
    }

    public async Task<ResponseResult<PaginatedListResponse<GetUserResponse>>> GetUsersAsync(GetUserRequest request)
    {
        var restRequest = new RestRequest(string.Empty, Method.Get);
        restRequest.AddQueryParameters(request);
        var restResponse = await _restClient.ExecuteAsync(restRequest);
        return restResponse.ToResponseResult<PaginatedListResponse<GetUserResponse>>();
    }

    public async Task<ResponseResult<GetUserResponse>> GetUserByIdAsync(Guid id)
    {
        var restRequest = new RestRequest($"{id}", Method.Get);
        var restResponse = await _restClient.ExecuteAsync(restRequest);
        return restResponse.ToResponseResult<GetUserResponse>();
    }

    public async Task<ResponseResult<CommonCreateResponse>> CreateUserAsync(CreateUserRequest request)
    {
        var restRequest = new RestRequest(string.Empty, Method.Post);
        restRequest.AddBody(request);
        var restResponse = await _restClient.ExecuteAsync(restRequest);
        return restResponse.ToResponseResult<CommonCreateResponse>();
    }

    public async Task<ResponseResult<CommonUpdateResponse>> UpdateUserAsync(Guid id, UpdateUserRequest request)
    {
        var restRequest = new RestRequest($"{id}", Method.Put);
        // Use AddFile for multipart if profile picture is present
        if (request.ProfilePictureFile is not null)
        {
            restRequest.AddParameter("Username", request.Username, ParameterType.RequestBody);
            restRequest.AddParameter("DisplayName", request.DisplayName, ParameterType.RequestBody);
            restRequest.AddParameter("Role", request.Role.ToString(), ParameterType.RequestBody);
            restRequest.AddParameter("PhoneNumber", request.PhoneNumber, ParameterType.RequestBody);
            restRequest.AddParameter("EmailAddress", request.EmailAddress, ParameterType.RequestBody);
            restRequest.AddParameter("IsActive", request.IsActive.ToString(), ParameterType.RequestBody);
            // Note: file upload from Blazor client handled separately via byte array
        }
        else
        {
            restRequest.AddBody(request);
        }
        var restResponse = await _restClient.ExecuteAsync(restRequest);
        return restResponse.ToResponseResult<CommonUpdateResponse>();
    }

    public async Task<ResponseResult<SuccessResponse>> DeleteUserAsync(Guid id)
    {
        var restRequest = new RestRequest($"{id}", Method.Delete);
        var restResponse = await _restClient.ExecuteAsync(restRequest);
        return restResponse.ToResponseResult<SuccessResponse>();
    }
}
