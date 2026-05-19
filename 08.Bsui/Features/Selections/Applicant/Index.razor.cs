using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Web;
using Psmp.Shared.Common.Extensions;
using Psmp.Bsui.Common.Extensions;
using Psmp.Shared.Scholarships.ScholarshipPostingApplicant.Queries;

namespace Psmp.Bsui.Features.Selections.Applicant;

public partial class Index
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

    protected ClaimsPrincipal _user = default!;

    protected string _greetings = default!;
    protected bool _isLoading = false;
    protected bool _isVisiblePopup = false;
    protected bool _isLoadingData = false;
    protected List<GetScholarshipPostingApplicantByUserResponse> _applicantByUserResponses = default!;

    protected override async Task OnInitializedAsync()
    {
        _greetings = $"Good {DateTimeOffset.Now.ToFriendlyTimeDisplayText()}";
        _user = (await AuthenticationStateTask).User;
        _isLoadingData = true;

        var scholashipsPostingApplicant = await _scholarshipPostingApplicantService.GetScholarshipPostingApplicantByUserAsync(_user.GetEmployeeId());
        if (scholashipsPostingApplicant.Error is null)
        {
            _applicantByUserResponses = scholashipsPostingApplicant.Result.Items.ToList();
        }
    }

    protected void OnClickScheduleDetail(MouseEventArgs args)
    {
        _isVisiblePopup = true;
    }

    protected void DialogOpen(object args)
    {
        _isVisiblePopup = true;
    }

    protected void DialogClose(object args)
    {
        _isVisiblePopup = false;
    }

    protected void ClickedNo()
    {
        _isVisiblePopup = false;
    }

    protected async void ClickedSave()
    {
        await Task.Delay(500);
        _isVisiblePopup = false;
    }
}
