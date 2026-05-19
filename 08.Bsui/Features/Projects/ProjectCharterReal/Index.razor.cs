using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Psmp.Bsui.Common.Extensions;
using Psmp.Bsui.Features.Projects.Constants;
using Psmp.Shared.Common.Constants;
using Psmp.Shared.Common.Responses;
using Psmp.Shared.Projects.ProjectCharterRealisations.Queries;
using Syncfusion.Blazor.Grids;

namespace Psmp.Bsui.Features.Projects.ProjectCharterReal;

public partial class Index
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;
    protected ClaimsPrincipal _user = default!;
    protected ErrorResponse? _error;
    protected bool _isLoading = false;
    protected bool _isLoadingData = false;
    protected string _employeeId = string.Empty;
    protected string _displayName = default!;
    protected int _countAll = 0;
    protected int _countWaitingForApproval = 0;
    protected int _countApproved = 0;
    protected int _countRejected = 0;
    protected bool _isAdmin = false;
    protected string _displayPosition = default!;
    private SfGrid<GetProjectCharterRealizationResponse> GridGetProjectCharterRealizationResponse { get; set; } = new();
    protected List<GetProjectCharterRealizationResponse> _projectCharterRealizationResponse = new();
    protected override async Task OnInitializedAsync()
    {
        _user = (await AuthenticationStateTask).User;
        _employeeId = _user.GetEmployeeId() ?? string.Empty;
        _displayName = _user.GetDisplayName() ?? string.Empty;
        _displayPosition = _user.GetPositionName() ?? string.Empty;
        if (_user.GetPermissions().Any())
        {
            _isAdmin = true;
        }
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
        await LoadDataProjectCahrterReal();
    }
    private async Task LoadDataProjectCahrterReal()
    {
        var response = await _projectCharterRealisationService.GetProjectCharterRealisationsByMentorAsync(_employeeId, _isAdmin);
        if (response.Error is null)
        {
            _projectCharterRealizationResponse = response.Result.Items.ToList();
            _countAll = _projectCharterRealizationResponse.Count();
            _countWaitingForApproval = _projectCharterRealizationResponse.Where(x => x.Status == Base.Project.Enums.ProjectCharterRealizationStatus.WaitingApproval).Count();
            _countApproved = _projectCharterRealizationResponse.Where(x => x.Status == Base.Project.Enums.ProjectCharterRealizationStatus.Approved).Count();
            _countRejected = _projectCharterRealizationResponse.Where(x => x.Status == Base.Project.Enums.ProjectCharterRealizationStatus.Rejected).Count();
        }
    }
    public void OnCommandClicked(CommandClickEventArgs<GetProjectCharterRealizationResponse> args)
    {
        if (args.CommandColumn.Type == CommandButtonType.None)
        {
            _navigationManager.NavigateTo(RouteFor.EditProjectRealed(args.RowData.ApplicantId.ToString()));
        }
    }
}
