using Microsoft.Extensions.Options;
using RestSharp;
using Smlp.Client.Common.Extensions;
using Smlp.Client.Services.UserInfo;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Scheduler.Queries.GetJobStats;
using Smlp.Shared.Scheduler.Queries.GetRecurringJobs;
using static Smlp.Shared.Scheduler.Constants.ApiEndpoint.V1;

namespace Smlp.Client.Services.BackEnd;

public class SchedulerService : IDisposable
{
    private readonly RestClient _restClient;

    public SchedulerService(IOptions<BackEndOptions> options, UserInfoService userInfo)
    {
        _restClient = new RestClient($"{options.Value.BaseUrl}/{Scheduler.Segment}");
        _restClient.AddUserInfo(userInfo);
    }

    public async Task<ResponseResult<GetRecurringJobsResponse>> GetRecurringJobsAsync()
    {
        var request = new RestRequest(Scheduler.RouteTemplateFor.RecurringJobs, Method.Get);
        return (await _restClient.ExecuteAsync(request)).ToResponseResult<GetRecurringJobsResponse>();
    }

    public async Task<ResponseResult<GetJobStatsResponse>> GetJobStatsAsync()
    {
        var request = new RestRequest(Scheduler.RouteTemplateFor.JobStats, Method.Get);
        return (await _restClient.ExecuteAsync(request)).ToResponseResult<GetJobStatsResponse>();
    }

    public async Task<ResponseResult<SuccessResponse>> TriggerJobAsync(string jobId)
    {
        var request = new RestRequest(
            Scheduler.RouteTemplateFor.TriggerJob.Replace("{jobId}", jobId), Method.Post);
        return (await _restClient.ExecuteAsync(request)).ToResponseResult<SuccessResponse>();
    }

    public void Dispose()
    {
        _restClient.Dispose();
        GC.SuppressFinalize(this);
    }
}
