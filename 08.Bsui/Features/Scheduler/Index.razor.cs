using MudBlazor;
using Smlp.Bsui.Common.Constants;
using Smlp.Bsui.Common.Extensions;
using Smlp.Shared.Scheduler.Queries.GetJobStats;
using Smlp.Shared.Scheduler.Queries.GetRecurringJobs;

namespace Smlp.Bsui.Features.Scheduler;

public partial class Index
{
    private List<BreadcrumbItem> _breadcrumbItems = new();
    private bool _isLoading = false;
    private string? _triggeringJobId = null;

    private List<RecurringJobItem> _recurringJobs = new();
    private GetJobStatsResponse? _stats;

    protected override async Task OnInitializedAsync()
    {
        SetupBreadcrumb();
        await LoadDataAsync();
    }

    private void SetupBreadcrumb()
    {
        _breadcrumbItems = new()
        {
            CommonBreadcrumbFor.Home,
            CommonBreadcrumbFor.Active("Scheduler Management"),
        };
    }

    private async Task LoadDataAsync()
    {
        _isLoading = true;
        try
        {
            var jobsTask = _schedulerService.GetRecurringJobsAsync();
            var statsTask = _schedulerService.GetJobStatsAsync();
            await Task.WhenAll(jobsTask, statsTask);

            var jobsResult = await jobsTask;
            if (jobsResult.Error is not null)
                _snackbar.AddError($"Gagal memuat recurring jobs: {jobsResult.Error.ToDetail()}");
            else
                _recurringJobs = jobsResult.Result?.Jobs ?? new();

            var statsResult = await statsTask;
            if (statsResult.Error is not null)
                _snackbar.AddError($"Gagal memuat job stats: {statsResult.Error.ToDetail()}");
            else
                _stats = statsResult.Result;
        }
        catch (Exception ex)
        {
            _snackbar.AddError($"Error memuat data scheduler: {ex.Message}");
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async Task TriggerJobAsync(string jobId)
    {
        _triggeringJobId = jobId;
        try
        {
            var result = await _schedulerService.TriggerJobAsync(jobId);
            if (result.Error is not null)
                _snackbar.AddError($"Gagal trigger job: {result.Error.ToDetail()}");
            else
                _snackbar.AddSuccess($"Job '{jobId}' berhasil di-trigger secara manual.");

            await Task.Delay(1000);
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            _snackbar.AddError($"Error: {ex.Message}");
        }
        finally
        {
            _triggeringJobId = null;
        }
    }

    private static (Color color, string text) GetJobStateChip(string? state) => state?.ToLower() switch
    {
        "succeeded" => (Color.Success, "✓ Sukses"),
        "failed" => (Color.Error, "✗ Gagal"),
        "processing" => (Color.Warning, "⏳ Berjalan"),
        "enqueued" => (Color.Info, "📋 Antrian"),
        "scheduled" => (Color.Default, "🕐 Terjadwal"),
        _ => (Color.Default, state ?? "—")
    };
}
