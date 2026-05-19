using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Smlp.Bsui.Common.Extensions;
using Smlp.Bsui.Features.Projects.Constants;
using Smlp.Shared.Common.Constants;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Projects.ProjectCharterPlans.Queries;
using Syncfusion.Blazor.Grids;

namespace Smlp.Bsui.Features.Projects.ProjectCharterPlan;

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
    private SfGrid<GetProjectCharterPlanResponse> GridProjectCharterPlanResponse { get; set; } = new();
    protected List<GetProjectCharterPlanResponse> _projectCharterPlanResponse = new();
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
        await LoadDataProjectCahrter();
    }
    private async Task LoadDataProjectCahrter()
    {
        var response = await _projectCharterPlansService.GetProjectCharterPlanByMentorAsync(_employeeId, _isAdmin);
        if (response.Error is null)
        {
            _projectCharterPlanResponse = response.Result.Items.ToList();
            _countAll = _projectCharterPlanResponse.Count();
            _countWaitingForApproval = _projectCharterPlanResponse.Where(x => x.Status == Base.Project.Enums.ProjectCharterPlanStatus.WaitingApproval).Count();
            _countApproved = _projectCharterPlanResponse.Where(x => x.Status == Base.Project.Enums.ProjectCharterPlanStatus.Approved).Count();
            _countRejected = _projectCharterPlanResponse.Where(x => x.Status == Base.Project.Enums.ProjectCharterPlanStatus.Rejected).Count();
        }
    }
    public void OnCommandClicked(CommandClickEventArgs<GetProjectCharterPlanResponse> args)
    {
        if (args.CommandColumn.Type == CommandButtonType.None)
        {
            _navigationManager.NavigateTo(RouteFor.EditProjectPlaned(args.RowData.ApplicantId.ToString()));
        }
    }
}
