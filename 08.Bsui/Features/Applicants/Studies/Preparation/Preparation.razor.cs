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
using Psmp.Shared.Studies.Constants;

namespace Psmp.Bsui.Features.Applicants.Studies.Preparation;

public partial class Preparation
{
    [Parameter]
    public int TabIndex { get; set; }

    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

    protected ClaimsPrincipal _user = default!;
    public bool IsShowPreparationDocument { get; set; } = true;
    public bool IsShowBuddyList { get; set; } = true;

    private int _activeTabIndex = 0;

    private readonly List<BreadcrumbItem> _breadcrumbItems = new();
    public ErrorResponse? Error { get; set; }
    protected bool _isCanView = false;
    protected string _displayName = string.Empty;
    protected string _employeeId = string.Empty;
    protected string _username = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        _activeTabIndex = TabIndex;
        SetupBreadcrumb();
        _user = (await AuthenticationStateTask).User;
        _employeeId = _user.GetEmployeeId() ?? string.Empty;

        _isCanView = false;

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

        // check if exist data scholarship
        var applicant = await _applicantService.GetApplicantByEmployeeIdAsync(_employeeId);
        if (applicant.Error != null)
        {
            Error = applicant.Error;
        }
        else
        {
            var lastApplicant = applicant.Result!.Items.OrderByDescending(x => x.Created).FirstOrDefault();
            if (lastApplicant is not null && lastApplicant!.Id != Guid.Empty)
            {
                var selection = await _selectionOverviewService.GetSelectionOverviewByApplicantIdAsync(lastApplicant!.Id);
                if ((selection.Result!.Stage == SelectionStage.TalentReview || selection.Result.Stage == SelectionStage.TalentReview_2) && selection.Result.Status == ApplicantStatus.Passed)
                {
                    _isCanView = true;
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
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.StudyPreparation));
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.PreparationDocument));
                break;
            case 1:
                _breadcrumbItems.Clear();
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.StudyPreparation));
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.BuddyList));
                break;
        }
    }
    private void OnTabChanged(int index)
    {
        _activeTabIndex = index;
        SetupBreadcrumb();
    }
}
