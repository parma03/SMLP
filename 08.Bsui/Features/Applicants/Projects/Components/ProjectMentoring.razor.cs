using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using Smlp.Shared.Common.Responses;
using System.Security.Claims;
using Smlp.Bsui.Common.Extensions;
using Smlp.Shared.Common.Constants;
using Smlp.Shared.Projects.ProjectComments.Commands;
using Smlp.Shared.Projects.ProjectComments.Queries;
using Smlp.Shared.Projects.ProjectMonitorings.Queries;
using Syncfusion.Blazor.Grids;
using Smlp.Shared.Projects.ProjectMonitorings.Commands;
using Smlp.Shared.Common.Extensions;

namespace Smlp.Bsui.Features.Applicants.Projects.Components;
public partial class ProjectMentoring
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

    protected ClaimsPrincipal _user = default!;

    protected ErrorResponse? _error;
    protected bool _isLoading = false;
    protected bool _isLoadingData = false;
    protected bool _isLoadingDialog = false;
    protected string _employeeId = string.Empty;
    private Guid _applicantId = default!;
    protected string _displayName = default!;
    private List<GetProjectCommentResponse> _comments = new();
    private string _message = default!;
    private readonly bool _isShowComment = false;
    protected string _mentorId = string.Empty;
    protected string _mentorName = string.Empty;
    protected string _mentorEmail = string.Empty;
    protected string _mentorPosition = string.Empty;
    private bool _visibility = false;
    private bool _isRead = false;
    protected string? _university = string.Empty;
    private SfGrid<GetProjectMonitoringResponse> GridProjectMonitoring { get; set; } = new();
    protected List<GetProjectMonitoringResponse> _projectMonitoring = new();
    private ProjectMonitoringRequest _request = new();

    protected string? _presentationStatus = default!;
    protected string? _presentationLink = default!;
    protected DateOnly? _presentationDate = default!;
    protected TimeSpan? _presentationTime = default!;

    public void RowBound(RowDataBoundEventArgs<GetProjectMonitoringResponse> args)
    {
        if (args.Data.Status == Base.Project.Enums.ProjectMonitoringStatus.Done)
        {
            args.Row.AddClass(new string[] { "e-removeEditcommand" });
        }
    }
    protected override async Task OnInitializedAsync()
    {
        _user = (await AuthenticationStateTask).User;
        _displayName = _user.GetDisplayName() ?? string.Empty;
        _employeeId = _user.GetEmployeeId() ?? string.Empty;

        #region bypass user applicant

        var isProductionEnvironment = _webHostEnvironment.IsInEnvironment(EnvironmentNames.Production);
        if (!isProductionEnvironment && _hrisOptions.Value.ByPass)
        {
            var asResponse = await _applicantByPassService.GetApplicantByPassEmployeeIdAsync(_user.GetEmployeeId() ?? string.Empty);
            if (asResponse.Result != null && asResponse.Result.EmployeeIdAlias != null)
            {
                _employeeId = asResponse.Result.EmployeeIdAlias;

            }
        }

        if (string.IsNullOrEmpty(_employeeId))
        {
            _snackbar.AddError("Not Found Employee ID");
            _isLoading = false;
            return;
        }
        #endregion bypass user applicant

        var applicat = await _applicantService.GetApplicantByEmployeeIdAsync(_employeeId);
        if (applicat.Error is null)
        {
            var data = applicat.Result!.Items.FirstOrDefault();
            if (data is not null)
            {
                _applicantId = data!.Id;
                _mentorId = data!.MentorId;
                _mentorName = data!.MentorName;
                _mentorPosition = data!.MentorPosition;
                _mentorEmail = data!.MentorEmail;
                _university = data!.UniversityApplicant;
                //await LoadDataMentor(_mentorPosition);
                await LoadDataProjectMonitoring(_applicantId);
            }
        }
    }
    //public async Task LoadDataMentor(string? mentorPosition)
    //{
    //    if (string.IsNullOrEmpty(mentorPosition))
    //    {
    //        _snackbar.AddWarning("Please input employee number");
    //        return;
    //    }

    //    _isLoadingData = true;

    //    var dataHRIS = await _hrisService.GetEmployeeByPositionAsync(mentorPosition);
    //    if (dataHRIS is not null)
    //    {
    //        _snackbar.AddInfo("Success retrieve data from HRIS");
    //    }
    //    else
    //    {
    //        _snackbar.AddError("Employee not found in HRIS!");
    //    }

    //    _request.MentorId = _mentorId;
    //    _isLoadingData = false;
    //}
    private async Task LoadDataProjectMonitoring(Guid applicantId)
    {
        var request = new GetProjectMonitoringRequest()
        {
            ApplicantId = applicantId,
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

            await LoadComment(applicantId);
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
            ProjectId = _applicantId,
            Comment = _message,
            User = _displayName,
            ProjectType = 2
        };

        var response = await _projectPlansService.CreateProjectPlanCommentAsync(request);
        if (response.Error is null)
        {
            _message = default!;
            await LoadComment(_applicantId);
        }
    }
    public void OnCommandClicked(CommandClickEventArgs<GetProjectMonitoringResponse> args)
    {
        _request = new();
        if (args.CommandColumn.Type == CommandButtonType.Edit)
        {
            _request.Title = args.RowData.Title;
            _request.ApplicantId = args.RowData.ApplicantId;
            _request.Id = args.RowData.Id;
            _request.PresentationLink = args.RowData.PresentationLink;
            _request.MateriLink = args.RowData.MateriLink;
            _request.SessionStart = args.RowData.SessionStart;
            _request.MentorId = _mentorId;
            _request.MentorngSchedule = args.RowData.MentorngSchedule;
            _request.Status = 1;
            _visibility = true;
            _isRead = true;
        }
        else
        {
            _request.PresentationLink = args.RowData.PresentationLink;
            _request.MentorngSchedule = args.RowData.MentorngSchedule;
            _request.Title = args.RowData.Title;
            _request.MateriLink = args.RowData.MateriLink;
            _request.SessionStart = args.RowData.SessionStart;
            _isRead = false;
            _visibility = true;
        }
    }
    private async Task OnClickAddAsync()
    {
        _request = new();
        _isRead = true;
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

    private bool ValidateDialog()
    {
        if (!string.IsNullOrWhiteSpace(_request.PresentationLink) &&
            !Uri.TryCreate(_request.PresentationLink, UriKind.Absolute, out _))
        {
            _snackbar.Add("Please provide a valid presentation link (example: https://...)", MudBlazor.Severity.Error);
            return false;
        }

        if (!string.IsNullOrWhiteSpace(_request.MateriLink) &&
            !Uri.TryCreate(_request.PresentationLink, UriKind.Absolute, out _))
        {
            _snackbar.Add("Please provide a valid Meeting link (example: https://...)", MudBlazor.Severity.Error);
            return false;
        }

        if (_request.MentorngSchedule is not DateOnly)
        {
            _snackbar.Add("Mentoring schedule date is mandatory", MudBlazor.Severity.Error);
            return false;
        }

        if (_request.SessionStart is not TimeSpan)
        {
            _snackbar.Add("Session start time is mandatory", MudBlazor.Severity.Error);
            return false;
        }

        return true;
    }

    private async Task DialogYes()
    {
        if (!ValidateDialog())
        {
            return;
        }

        _isLoadingDialog = true;
        StateHasChanged();

        try
        {
            if (_request.Id is null)
            {
                _request.ApplicantId = _applicantId;
                _request.Status = 1;
                _request.MentorId = _mentorId;
                _request.MentorPosition = _mentorPosition;

                var response = await _projectMonitoringService
                    .CreateProjectMonitoringAsync(_request);

                if (response.Error is null)
                {
                    _snackbar.AddSuccess("Project Mentoring Schedule Saved");
                    _visibility = false;
                    await LoadDataProjectMonitoring(_applicantId);
                }
            }
            else
            {
                _request.MentorId = _mentorId;
                _request.MentorPosition = _mentorPosition;

                var response = await _projectMonitoringService
                    .ProjectMonitoringUpdatesAsync(_request);

                if (response.Error is null)
                {
                    _snackbar.AddSuccess("Project Mentoring Schedule Updated");
                    _visibility = false;
                    await LoadDataProjectMonitoring(_applicantId);
                }
            }
        }
        finally
        {
            _isLoadingDialog = false;
            StateHasChanged();
        }
    }

}
