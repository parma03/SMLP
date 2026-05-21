/*
* Sistem Manajemen Loyalitas Pelanggan (SMLP)
*/

using Microsoft.Extensions.Options;
using RestSharp;
using Smlp.Client.Common.Extensions;
using Smlp.Client.Services.UserInfo;
using Smlp.Shared.Emails.Commands.SendEmail;
using static Smlp.Shared.Emails.Constants.ApiEndpoint.V1;

namespace Smlp.Client.Services.BackEnd;

public class EmailService : IDisposable
{
    private readonly RestClient _restClient;

    public EmailService(IOptions<BackEndOptions> backEndServiceOptions, UserInfoService userInfo)
    {
        var options = new RestClientOptions(new Uri($"{backEndServiceOptions.Value.BaseUrl}/{Emails.Segment}"))
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

    /// <summary>Kirim email notifikasi (poin masuk, reward tersedia, dll.)</summary>
    public async Task<SendEmailResponse?> SendEmailAsync(SendEmailRequest request)
    {
        var restRequest = new RestRequest(Emails.RouteTemplateFor.SendEmail, Method.Post);
        restRequest.AddBody(request);
        var restResponse = await _restClient.ExecuteAsync<SendEmailResponse>(restRequest);
        return restResponse.Data;
    }
}
