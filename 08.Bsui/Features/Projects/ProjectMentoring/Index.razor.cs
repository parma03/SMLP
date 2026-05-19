using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Smlp.Bsui.Common.Extensions;
using Smlp.Bsui.Features.Projects.Constants;
using Smlp.Shared.Common.Constants;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Projects.ProjectMonitorings.Queries;
using Syncfusion.Blazor.Grids;

namespace Smlp.Bsui.Features.Projects.ProjectMentoring;

public partial class Index
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;
    protected ClaimsPrincipal _user = default!;
    protected ErrorResponse? _error;
    protected bool _isLoading = false;
    protected bool _isLoadingData = false;
    protected bool _isAdmin = false;
    protected string _employeeId = string.Empty;
    protected string _displayName = default!;
    protected int _countAll = 0;
    protected int _countStudyPeriod = 0;
    protected int _countUniversitasAdmission = 0;
    protected int _countStudyPreparation = 0;
    protected string _displayPosition = default!;
    private SfGrid<GetProjectMonitoringMentorResponse> GridProjectMonitoringMentor { get; set; } = new();
    protected List<GetProjectMonitoringMentorResponse> _projectMonitoringMentor = new();
    protected override async Task OnInitializedAsync()
    {
        _user = (await AuthenticationStateTask).User;
        if (_user.GetPermissions().Any())
        {
            _isAdmin = true;
        }

        _employeeId = _user.GetEmployeeId() ?? string.Empty;
        _displayName = _user.GetDisplayName() ?? string.Empty;
        _displayPosition = _user.GetPositionName() ?? string.Empty;
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
        await LoadDataProjectMonitoring();
    }
    private async Task LoadDataProjectMonitoring()
    {
        var request = new GetProjectMonitoringRequest()
        {
            MentorId = _employeeId,
            IsAdmin = _isAdmin
        };
        var response = await _projectMonitoringService.GetProjectMonitoringByMentorAsync(request);
        if (response.Error is null)
        {
            _projectMonitoringMentor = response.Result!.Items
                .Where(x => x.StatusDecree == "SKSend")
                .ToList();
            _countAll = _projectMonitoringMentor.Count();
            _countStudyPreparation = _projectMonitoringMentor.Where(x => x.Status == "Study Preparation").Count();
            _countStudyPeriod = _projectMonitoringMentor.Where(x => x.Status == "Study Period").Count();
            _countUniversitasAdmission = _projectMonitoringMentor.Where(x => x.Status == "Universitas Admission").Count();
        }
    }
    public void OnCommandClicked(CommandClickEventArgs<GetProjectMonitoringMentorResponse> args)
    {
        if (args.CommandColumn.Type == CommandButtonType.None)
        {
            _navigationManager.NavigateTo(RouteFor.DetailProjectMentoringed(args.RowData.ApplicantId.ToString()));
        }
    }
}

