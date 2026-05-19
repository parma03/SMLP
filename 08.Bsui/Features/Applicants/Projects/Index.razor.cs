using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using Psmp.Bsui.Common.Constants;
using Psmp.Bsui.Common.Extensions;
using Psmp.Shared.Common.Constants;
using Psmp.Shared.Projects.Constants;
using Psmp.Shared.Projects.ProjectCharterPlans.Queries;

namespace Psmp.Bsui.Features.Applicants.Projects;

public partial class Index
{
    [Parameter]
    public int TabIndex { get; set; }

    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

    protected ClaimsPrincipal _user = default!;
    public bool IsShowScholarshipPlan { get; set; } = false;
    public bool IsShowScholarshipPosting { get; set; } = false;
    private int _activeTabIndex = 0;
    protected string _employeeId = string.Empty;
    private Guid _applicantId = default!;
    private readonly List<BreadcrumbItem> _breadcrumbItems = new();

    private bool _statusProjectPlan = false;
    private bool _statusProjectCharterPlan = false;
    protected override async Task OnInitializedAsync()
    {
        _activeTabIndex = TabIndex;
        SetupBreadcrumb();
        _user = (await AuthenticationStateTask).User;
        if (_user.Identity.IsAuthenticated)
        {
            if (_user.Claims.Where(x => x.Type.ToLower().Trim() == "permission" && x.Value.ToLower().Trim().Contains("scholarship.plan.view")).ToList().Count > 0)
            {
                IsShowScholarshipPlan = true;
            }

            if (_user.Claims.Where(x => x.Type.ToLower().Trim() == "permission" && x.Value.ToLower().Trim().Contains("scholarship.posting.view")).ToList().Count > 0)
            {
                IsShowScholarshipPosting = true;
            }
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
            return;
        }

        var applicat = await _applicantService.GetApplicantByEmployeeIdAsync(_employeeId);
        if (applicat.Error is null)
        {
            var data = applicat.Result!.Items.FirstOrDefault();
            if (data is not null)
            {
                _applicantId = data!.Id;
            }
        }

        #endregion bypass user applicant

        await LoadDataProject();
    }

    private void SetupBreadcrumb()
    {
        switch (_activeTabIndex)
        {
            case 0:
                _breadcrumbItems.Clear();
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.InnovationProject));
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.ProjectPlan));
                break;

            case 1:
                _breadcrumbItems.Clear();
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.InnovationProject));
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.ProjectMonitoring));
                break;

            case 2:
                _breadcrumbItems.Clear();
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.InnovationProject));
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.ProjectCharterPlan));
                break;
            case 3:
                _breadcrumbItems.Clear();
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.InnovationProject));
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.ProjectCharterReal));
                break;
        }
    }

    private void OnTabChanged(int index)
    {
        _activeTabIndex = index;
        SetupBreadcrumb();
    }

    private async Task LoadDataProject()
    {
        var dataStatusProjectPlan = await _projectPlansService.GetProjectPlanByApplicantIdAsync(_applicantId);
        if (dataStatusProjectPlan.Error is null)
        {
            try
            {
                _statusProjectPlan = dataStatusProjectPlan.Result.Status == Base.Project.Enums.ProjectPlanStatus.Approved;
            }
            catch
            {

            }
        }

        var request = new GetProjectCharterPlanRequest()
        {
            ApplicantId = _applicantId,
        };

        var dataStatusProjectCharter = await _projectCharterPlansService.GetProjectCharterPlanByApplicantIdAsync(new GetProjectCharterPlanRequest
        {
            ApplicantId = _applicantId,
        });
        if (dataStatusProjectCharter.Error is null)
        {
            try
            {
                _statusProjectCharterPlan = dataStatusProjectCharter.Result.Status == Base.Project.Enums.ProjectCharterPlanStatus.Approved;
            }
            catch
            {

            }
        }
    }
}
