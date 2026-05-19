using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Smlp.Base.Selections.Enums;
using Smlp.Bsui.Common.Extensions;
using Smlp.Shared.Common.Constants;

namespace Smlp.Bsui.Layouts;

public partial class NavMenu
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

    protected ClaimsPrincipal _user = default!;
    protected string _employeeId = string.Empty;
    protected bool _isAwardee;
    protected bool _isMentor = false;

    protected override async Task OnInitializedAsync()
    {
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
            }
        }

        #endregion bypass user applicant

        //get scholarship existing
        var applicant = await _applicantService.GetApplicantByEmployeeIdAsync(_employeeId);
        if (applicant.Error is null)
        {
            _isAwardee = false;
            var lastApplicant = applicant.Result!.Items.OrderByDescending(x => x.Created).FirstOrDefault();
            if (lastApplicant != null)
            {
                var applicantId = lastApplicant!.Id;

                var selection = await _selectionOverviewService.GetSelectionOverviewByApplicantIdAsync(applicantId);
                if (selection.Error is null)
                {
                    if (selection.Result!.Stage == SelectionStage.TalentReview && selection.Result.Status == ApplicantStatus.Passed)
                    {
                        _isAwardee = true;
                    }
                    else if (selection.Result.Stage == SelectionStage.TalentReview_2 && selection.Result.Status == ApplicantStatus.Passed)
                    {
                        _isAwardee = true;
                    }
                }
            }
        }

        var mentor = await _applicantService.GetMentorByEmployeeIdAsync(_employeeId);
        if (mentor.Error is null)
        {
            if (mentor.Result is not null)
            {
                _isMentor = true;
            }
        }
    }
}
