
using Microsoft.Extensions.Options;
//using Newtonsoft.Json;
using Smlp.Client.Common.Extensions;
using Smlp.Client.Services.UserInfo;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Emails.Commands.SendEmail;
using Smlp.Shared.Emails.Commands.SendEmailSchedule;
using Smlp.Shared.Emails.Commands.SendEmailStudyPreparation;

//using Smlp.Shared.Emails.Queries.GetEmail;
using RestSharp;
using static Smlp.Shared.Emails.Constants.ApiEndpoint.V1;

namespace Smlp.Client.Services.BackEnd;
public class EmailService
{
    private readonly RestClient _restClient;

    public EmailService(IOptions<BackEndOptions> backEndServiceOptions, UserInfoService userInfo)
    {
        _restClient = new RestClient($"{backEndServiceOptions.Value.BaseUrl}/{Emails.Segment}");
        _restClient.AddUserInfo(userInfo);
    }
    public async Task<ResponseResult<ListResponse<SendEmailResponse>>> SendMailAsync(SendEmailRequest request)
    {
        //var restRequest = new RestRequest(nameof(Emails.RouteTemplateFor.SendEmail), Method.Post);
        var restRequest = new RestRequest(string.Empty, Method.Post);
        restRequest.AddParameters(request);

        var restResponse = await _restClient.ExecuteAsync(restRequest);
        //var listEmail = new List<GetEmailResponse>();
        //foreach (var item in request.ListEmail)
        //{
        //    listEmail.Add(new GetEmailResponse { Email = item.Email });
        //}

        //var jsonSrting = JsonConvert.SerializeObject(request);
        //restRequest.AddParameter("application/json", jsonSrting, ParameterType.RequestBody);
        //var restResponse = await _restClient.ExecuteAsync(restRequest);

        return restResponse.ToResponseResult<ListResponse<SendEmailResponse>>();
    }

    public async Task<ResponseResult<ListResponse<SendEmailResponse>>> SendMailScheduleToeflAsync(SendEmailScheduleToeflRequest request)
    {
        var restRequest = new RestRequest(nameof(Emails.RouteTemplateFor.SendEmailScheduleToefl), Method.Post);
        restRequest.AddParameters(request);

        var restResponse = await _restClient.ExecuteAsync(restRequest);
        return restResponse.ToResponseResult<ListResponse<SendEmailResponse>>();
    }

    public async Task<ResponseResult<ListResponse<SendEmailResponse>>> SendMailSchedulePsychoTestAsync(SendEmailSchedulePsychoTestRequest request)
    {
        var restRequest = new RestRequest(nameof(Emails.RouteTemplateFor.SendEmailSchedulePsychoTest), Method.Post);
        restRequest.AddParameters(request);

        var restResponse = await _restClient.ExecuteAsync(restRequest);
        return restResponse.ToResponseResult<ListResponse<SendEmailResponse>>();
    }

    public async Task<ResponseResult<ListResponse<SendEmailResponse>>> SendMailSponsorshipLetterAsync(SendEmailSponsorshipLetterRequest request)
    {
        var restRequest = new RestRequest(nameof(Emails.RouteTemplateFor.SendEmailSponsorshipLetter), Method.Post);
        restRequest.AddParameters(request);

        var restResponse = await _restClient.ExecuteAsync(restRequest);
        return restResponse.ToResponseResult<ListResponse<SendEmailResponse>>();
    }

    public async Task<ResponseResult<ListResponse<SendEmailResponse>>> SendMailVisaGuaranteeAsync(SendEmailVisaGuaranteeRequest request)
    {
        var restRequest = new RestRequest(nameof(Emails.RouteTemplateFor.SendEmailVisaGuarantee), Method.Post);
        restRequest.AddParameters(request);

        var restResponse = await _restClient.ExecuteAsync(restRequest);
        return restResponse.ToResponseResult<ListResponse<SendEmailResponse>>();
    }
}
