using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using Smlp.Bsui.Common.Constants;
using Smlp.Bsui.Common.Extensions;
using Smlp.Shared.Common.Extensions;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Projects.ProjectComments.Commands;
using Smlp.Shared.Projects.ProjectComments.Queries;
using Smlp.Shared.Projects.ProjectMonitorings.Commands;
using Smlp.Shared.Projects.ProjectMonitorings.Queries;
using Syncfusion.Blazor.Grids;

namespace Smlp.Bsui.Features.Projects.ProjectMentoring;

public partial class Detail
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

    protected ClaimsPrincipal _user = default!;
    [Parameter]
    public string Id { get; set; } = default!;
    private List<BreadcrumbItem> _breadcrumbItems = new();

    protected ErrorResponse? _error;
    protected bool _isLoading = false;
    protected bool _isLoadingData = false;
    protected bool _dataExsist = false;
    protected string _displayName = default!;
    private List<GetProjectCommentResponse> _comments = new();
    private string _message = default!;
    private bool _visibility = false;
    private bool _isRead = false;
    protected string _employeeId = string.Empty;
    protected ProjectMonitoringRequest _request = new();
    private SfGrid<GetProjectMonitoringResponse> GridProjectMonitoring { get; set; } = new();
    protected List<GetProjectMonitoringResponse> _projectMonitoring = new();

    protected string _presentationStatus = default!;
    protected string _presentationLink = default!;
    protected DateOnly? _presentationDate = default!;
    protected TimeSpan? _presentationTime = default!;
    protected string? _university = string.Empty;
    protected override async Task OnInitializedAsync()
    {
        SetupBreadcrumb();

        _user = (await AuthenticationStateTask).User;
        _displayName = _user.GetDisplayName() ?? string.Empty;
        _employeeId = _user.GetEmployeeId() ?? string.Empty;

        await LoadDataProjectMentoringPlan(new Guid(Id));
    }
    private void SetupBreadcrumb()
    {
        _breadcrumbItems = new()
        {
            CommonBreadcrumbFor.Active("Detail Project Mentoring")
        };
    }
    private async Task LoadDataProjectMentoringPlan(Guid id)
    {
        var request = new GetProjectMonitoringRequest()
        {
            ApplicantId = id,
        };
        var response = await _projectMonitoringService.GetProjectMonitoringAsync(request);
        if (response.Error is null)
        {
            _projectMonitoring = response.Result!.Items
                .OrderByDescending(x => x.MentorngSchedule)
                .ToList();

            var now = DateTime.Now;
            var today = DateOnly.FromDateTime(now);

            var earliestScheduling = response.Result.Items
                .Where(x =>
                    x.Status == Base.Project.Enums.ProjectMonitoringStatus.Scheduling &&
                    x.MentorngSchedule.HasValue &&
                    x.SessionStart.HasValue &&
                    (
                        x.MentorngSchedule.Value > today ||

                        (
                            x.MentorngSchedule.Value == today &&
                            x.MentorngSchedule.Value.ToDateTime(TimeOnly.FromTimeSpan(x.SessionStart.Value)) > now
                        )
                    )
                )
                .OrderBy(x => x.MentorngSchedule)
                .ThenBy(x => x.SessionStart)
                .FirstOrDefault();

            if (earliestScheduling is not null)
            {
                _presentationStatus = earliestScheduling.Status.GetDescription();
                _presentationLink = earliestScheduling.PresentationLink;
                _presentationDate = earliestScheduling.MentorngSchedule;
                _presentationTime = earliestScheduling.SessionStart;
            }
            else
            {
                _presentationStatus = null;
                _presentationLink = null;
                _presentationDate = null;
                _presentationTime = null;
            }

            await LoadComment(id);
            await LoadDataApplicant();
        }
    }

    private async Task LoadDataApplicant()
    {
        var applicat = await _applicantService.GetApplicantByIdAsync(new Guid(Id));
        if (applicat.Error is null)
        {
            var data = applicat.Result!.Items.FirstOrDefault();
            if (data is not null)
            {
                _university = data!.UniversityApplicant;

            }
        }
    }
    private async Task LoadComment(Guid projectId)
    {
        var request = new GetProjectCommentRequest()
        {
            ProjectId = projectId,
            ProjectType = 2,
        };

        var response = await _projectPlansService.GetProjectPlanCommentAsync(request);
        if (response.Error is null)
        {
            _comments = response.Result!.Items.ToList();
        }
    }
    private async Task SubmitComment()
    {
        var request = new ProjectCommentRequest()
        {
            ProjectId = new Guid(Id),
            Comment = _message,
            User = _displayName,
            ProjectType = 2
        };

        var response = await _projectPlansService.CreateProjectPlanCommentAsync(request);
        if (response.Error is null)
        {
            _message = default!;
            await LoadComment(new Guid(Id));
        }
    }
    public void OnCommandClicked(CommandClickEventArgs<GetProjectMonitoringResponse> args)
    {
        _request.PresentationLink = args.RowData.PresentationLink;
        _request.MentorngSchedule = args.RowData.MentorngSchedule;
        _request.MateriLink = args.RowData.MateriLink;
        _request.SessionStart = args.RowData.SessionStart;
        _request.Title = args.RowData.Title;
        _request.Id = args.RowData.Id;
        _request.ApplicantId = args.RowData.ApplicantId;
        _request.MentorId = args.RowData.MentorId;
        _request.Status = args.RowData.Status.GetIntValue();
        _isRead = false;
        _visibility = true;
    }

    private void DialogOpen()
    {
        _visibility = true;
    }
    private void DialogClose()
    {
        _visibility = false;
    }

    private async Task DialogCancel()
    {
        _request.Status = 0;
        var response = await _projectMonitoringService.ProjectMonitoringUpdateStatusAsync(_request);
        if (response.Error is null)
        {
            _snackbar.AddSuccess($"Project Monitoring is Cancel");
            _visibility = false;
            await LoadDataProjectMentoringPlan(new Guid(Id));
        }
        else
        {
            _snackbar.AddSuccess($"{response.Error.Detail}");
            _visibility = false;
        }

        _visibility = false;
    }
    private async Task DialogDone()
    {
        _request.Status = 2;
        var response = await _projectMonitoringService.ProjectMonitoringUpdateStatusAsync(_request);
        if (response.Error is null)
        {
            _snackbar.AddSuccess($"Project Monitoring is Done");
            _visibility = false;
            await LoadDataProjectMentoringPlan(new Guid(Id));
        }
        else
        {
            _snackbar.AddSuccess($"{response.Error.Detail}");
            _visibility = false;
        }

    }

    private string FormatUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return "#";
        }

        if (url.StartsWith("http://") || url.StartsWith("https://"))
        {
            return url;
        }

        return "https://" + url;
    }
}
