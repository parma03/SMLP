/*
* Sistem Manajemen Loyalitas Pelanggan (SMLP)
*/

using Microsoft.Extensions.Options;
using RestSharp;
using Smlp.Client.Common.Extensions;
using Smlp.Client.Services.UserInfo;
using Smlp.Shared.AppSettings.Commands.UpsertAppSettings;
using Smlp.Shared.AppSettings.Queries.GetAppSettings;
using Smlp.Shared.Common.Responses;

namespace Smlp.Client.Services.BackEnd.Masters;

public class AppSettingService : IDisposable
{
    private readonly RestClient _restClient = default!;

    public AppSettingService(IOptions<BackEndOptions> backEndServiceOptions, UserInfoService userInfo)
    {
        if (backEndServiceOptions is null) return;

        var options = new RestClientOptions(new Uri($"{backEndServiceOptions!.Value.BaseUrl}/V1/AppSettings"))
        {
            RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true
        };
        _restClient = new RestClient(options);
        _restClient.AddUserInfo(userInfo);
    }

    /// <summary>
    /// Mengambil seluruh pengaturan aplikasi dari server.
    /// </summary>
    public async Task<ResponseResult<GetAppSettingsResponse>> GetAppSettingsAsync()
    {
        var request = new RestRequest(string.Empty, Method.Get);
        return (await _restClient.ExecuteAsync(request)).ToResponseResult<GetAppSettingsResponse>();
    }

    /// <summary>
    /// Menyimpan pengaturan aplikasi ke server (upsert).
    /// </summary>
    public async Task<ResponseResult<SuccessResponse>> UpsertAppSettingsAsync(UpsertAppSettingsRequest payload)
    {
        var request = new RestRequest(string.Empty, Method.Put);
        request.AddJsonBody(payload);
        return (await _restClient.ExecuteAsync(request)).ToResponseResult<SuccessResponse>();
    }

    public void Dispose()
    {
        _restClient?.Dispose();
        GC.SuppressFinalize(this);
    }
}
