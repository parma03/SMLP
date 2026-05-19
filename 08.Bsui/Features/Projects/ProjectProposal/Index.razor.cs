using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using System.Security.Claims;
using Psmp.Shared.Common.Responses;
using Psmp.Bsui.Common.Extensions;
using Psmp.Shared.Common.Constants;
using Psmp.Shared.Projects.ProjectPlans.Queries;
using Syncfusion.Blazor.Grids;
using Psmp.Bsui.Features.Projects.Constants;
namespace Psmp.Bsui.Features.Projects.ProjectProposal;

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
    protected string _displayPosition = default!;
    protected int _countAll = 0;
    protected int _countWaitingForApproval = 0;
    protected int _countApproved = 0;
    protected int _countRejected = 0;
    protected bool _isAdmin = false;
    private SfGrid<GetProjectPlanResponse> Grid { get; set; } = new();
    protected List<GetProjectPlanResponse> _projectPlanResponses = new();
    private bool VisiblePopup { get; set; }
    private Guid IdScholarshipPlan { get; set; }
    private string? ActionType { get; set; }
    private bool IsEnabled { get; set; }
    private string TitlePopup { get; set; } = "View";
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
        await LoadDataProjectProposal();
    }
    private async Task LoadDataProjectProposal()
    {
        var response = await _projectPlansService.GetAwordeProjectPlanByMentorAsync(_employeeId, _isAdmin);
        if (response.Error is null)
        {
            _projectPlanResponses = response.Result.Items.ToList();
            _countAll = _projectPlanResponses.Count();
            _countWaitingForApproval = _projectPlanResponses.Where(x => x.Status == Base.Project.Enums.ProjectPlanStatus.WaitingApproval).Count();
            _countApproved = _projectPlanResponses.Where(x => x.Status == Base.Project.Enums.ProjectPlanStatus.Approved).Count();
            _countRejected = _projectPlanResponses.Where(x => x.Status == Base.Project.Enums.ProjectPlanStatus.Rejected).Count();

        }
    }
    public void OnCommandClicked(CommandClickEventArgs<GetProjectPlanResponse> args)
    {
        if (args.CommandColumn.Type == CommandButtonType.None)
        {
            _navigationManager.NavigateTo(RouteFor.EditProjectProposaled(args.RowData.ApplicantId.ToString()));
        }
        else if (args.CommandColumn.Type == CommandButtonType.Edit)
        {
            ActionType = "view";
            TitlePopup = "View Specific Requirement";
            IsEnabled = true;
            IdScholarshipPlan = args.RowData.ScholarshipPlandId;
            VisiblePopup = true;
            StateHasChanged();
        }
    }

    //Modal Dialog
    private void DialogOpen()
    {
        VisiblePopup = true;
    }

    private void DialogClose()
    {
        ActionType = "";
        VisiblePopup = false;
        //_createUniversityRecommendationRequest = new CreateUniversityRecommendationRequest { Dateline = DateOnly.FromDateTime(DateTime.Now), ScholarshipSubject = new Domain.Entities.Masters.ScholarshipSubject() };
        //_updateUniversityRecommendationRequest = new UpdateUniversityRecommendationRequest();
    }
    private void ClickedNo()
    {
        DialogClose();
    }
}
