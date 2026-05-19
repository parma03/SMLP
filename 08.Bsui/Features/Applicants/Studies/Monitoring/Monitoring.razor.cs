/*
* Pertamina Scholarship Management Portal
* Copyright (c) 2024-2025 PT. Pertamina Persero
*/

using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using Psmp.Base.Selections.Enums;
using Psmp.Bsui.Common.Constants;
using Psmp.Bsui.Common.Extensions;
using Psmp.Shared.Common.Constants;
using Psmp.Shared.Common.Responses;
using Psmp.Shared.StudyMonitorings.Constants;

namespace Psmp.Bsui.Features.Applicants.Studies.Monitoring;

public partial class Monitoring
{
    [Parameter]
    public int TabIndex { get; set; }

    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

    protected ClaimsPrincipal _user = default!;

    public ErrorResponse? Error { get; set; }
    protected Guid ApplicantId { get; set; }
    protected Guid AdmissionId { get; set; }

    protected bool _canView = false;
    protected string _employeeId = string.Empty;
    protected string _username = string.Empty;
    protected string _displayName = default!;
    protected string _universityName = default!;
    private int _activeTabIndex = 0;
    private readonly List<BreadcrumbItem> _breadcrumbItems = new();

    protected override async Task OnInitializedAsync()
    {
        _activeTabIndex = TabIndex;
        SetupBreadcrumb();
        _user = (await AuthenticationStateTask).User;
        _employeeId = _user.GetEmployeeId() ?? string.Empty;

        #region bypass user applicant

        var isProductionEnvironment = _webHostEnvironment.IsInEnvironment(EnvironmentNames.Production);
        if (!isProductionEnvironment && _hrisOptions.Value.ByPass)
        {
            var asResponse = await _applicantByPassService.GetApplicantByPassEmployeeIdAsync(_user.GetEmployeeId() ?? string.Empty);
            if (asResponse.Result != null && asResponse.Result.EmployeeIdAlias != null)
            {
                _employeeId = asResponse.Result.EmployeeIdAlias;
                _username = asResponse.Result.EmailAlias;
            }
        }

        if (string.IsNullOrEmpty(_employeeId))
        {
            _snackbar.AddError("Not Found Employee ID");
            return;
        }

        #endregion bypass user applicant

        //get scholarship existing
        var applicant = await _applicantService.GetApplicantByEmployeeIdAsync(_employeeId);
        if (applicant.Error != null)
        {
            Error = applicant.Error;
        }
        else
        {
            var lastApplicant = applicant.Result!.Items.OrderByDescending(x => x.Created).FirstOrDefault();
            if (lastApplicant is not null)
            {
                ApplicantId = lastApplicant!.Id;
                var selection = await _selectionOverviewService.GetSelectionOverviewByApplicantIdAsync(lastApplicant!.Id);
                if ((selection.Result!.Stage == SelectionStage.TalentReview || selection.Result.Stage == SelectionStage.TalentReview_2) && selection.Result.Status == ApplicantStatus.Passed)
                {
                    _canView = true;
                }

                var admissions = await _universityAdmissionService.GetUniversityAdmissionAsync();
                if (admissions.Error != null)
                {
                    Error = admissions.Error;
                }
            }
        }
    }

    private void SetupBreadcrumb()
    {
        switch (_activeTabIndex)
        {
            case 0:
                _breadcrumbItems.Clear();
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.StudyMonitoring));
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.StudyPeriod));
                break;

            case 1:
                _breadcrumbItems.Clear();
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.StudyMonitoring));
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.Graduate));
                break;

            case 2:
                _breadcrumbItems.Clear();
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.StudyMonitoring));
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.AfterProgram));
                break;
        }
    }

    private void OnTabChanged(int index)
    {
        _activeTabIndex = index;
        SetupBreadcrumb();
    }
}
