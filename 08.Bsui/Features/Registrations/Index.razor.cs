using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Psmp.Base.Selections.Enums;
using Psmp.Bsui.Common.Extensions;
using Psmp.Shared.Common.Responses;
using Psmp.Shared.Scholarships.ScholarshipPostingApplicant.Commands.Create;
using Psmp.Shared.Scholarships.ScholarshipPostingApplicant.Queries;
using Psmp.Shared.Selections.SelectionAdministrations.Commands.CreateSelectionAdministration;
using Psmp.Shared.Selections.SelectionOverviews.Commands.Create;

namespace Psmp.Bsui.Features.Registrations;

public partial class Index
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

    protected ClaimsPrincipal _user = default!;

    protected List<ExtendedScholarshipPostingResponse> _scholarships = new();
    //protected List<ExtendedScholarshipPostingResponse> _scholarshipExtends = new();

    protected ErrorResponse? _error;
    protected bool _isLoading = false;
    protected bool _isLoadingData = false;
    protected Guid _scholarshipPostingId;
    protected bool _isExpandDetail = false;
    protected bool _isVisiblePopup = false;
    protected bool _isValidRegistered = false;
    protected string _contentPopup = default!;
    protected string _scholarsipTitleExpanded = default!;
    protected Guid _scholarsipPlanIdExpanded = Guid.Empty;
    protected Guid _scholarsipPostingIdExpanded = Guid.Empty;

    protected override async Task OnInitializedAsync()
    {
        _user = (await AuthenticationStateTask).User;

        _isLoading = true;
        await SetDataOnLoadAsync();
        _isLoading = false;
    }

    private async Task SetDataOnLoadAsync()
    {
        _isLoadingData = true;

        var scholaships = await _scholarshipPostingApplicantService.GetScholarshipPostingApplicantListAsync();
        if (scholaships.Error is null)
        {
            _scholarships = scholaships.Result!.Items.ToList()
               .Select(p => new ExtendedScholarshipPostingResponse
               {
                   Expanded = false,
                   ScholashipPlanId = p.ScholashipPlanId,
                   ScholashipPostingId = p.ScholashipPostingId,
                   Title = p.Title,
                   TitleBatch = p.TitleBatch,
                   Batch = p.Batch,
                   SelectionType = p.SelectionType,
                   SelectionCategory = p.SelectionCategory,
                   DueDate = p.DueDate,
                   Description = p.Description,
                   Prl = p.Prl,
                   MaximumAge = p.MaximumAge,
                   MinimumWorkPeriod = p.MinimumWorkPeriod,
                   PerformanceRating = p.PerformanceRating
               })
               .Where(x => x.ScholashipPostingId != Guid.Empty)
               .Where(x => x.DueDate >= DateTime.Now)
               .ToList() ?? new List<ExtendedScholarshipPostingResponse>();
        }
        else
        {
            _error = scholaships.Error;
        }

        _isLoadingData = false;
    }

    private void OnClickDetail(ExtendedScholarshipPostingResponse item)
    {
        item.Expanded = !item.Expanded;

        _scholarsipPlanIdExpanded = item.ScholashipPlanId;
        _scholarsipPostingIdExpanded = item.ScholashipPostingId;
        _scholarsipTitleExpanded = item.TitleBatch;

        StateHasChanged();
    }

    private void OnClickSubmit()
    {
        _isVisiblePopup = true;
        _contentPopup = $"Hi <b>{_user.GetDisplayName()}</b><br />Are you sure want to register <b><i>{_scholarsipTitleExpanded}</i></b>?";
    }

    private void DialogOpen(object args)
    {
        _isVisiblePopup = true;
    }

    private void DialogClose(object args)
    {
        _isVisiblePopup = false;
    }

    private void ClickedNo()
    {
        _isVisiblePopup = false;
    }

    private async void ClickedYes()
    {
        var requirements = await _scholarshipPlanService.GetScholarshipPlanSpecialRequirementAsync(_scholarsipPlanIdExpanded);

        //validate company code
        if (requirements.Error is null)
        {
            if (requirements.Result is not null)
            {
                var a = _user.GetCompanyCode();
                if (requirements.Result.Items.Where(x => x.CompanyCode == _user.GetCompanyCode()).Any())
                {
                    _isValidRegistered = true;

                    var applicantData = new CreateScholarshipPostingApplicantRequest()
                    {
                        CompanyId = _user.GetCompanyCode() ?? "",
                        Email = _user.GetUsername() ?? "",
                        EmployeeNumber = _user.GetEmployeeId() ?? "",
                        Name = _user.GetDisplayName() ?? "",
                        Position = _user.GetPositionName() ?? "",
                        ScholarshipPostingId = _scholarsipPostingIdExpanded
                    };

                    var responseapplicant = await _scholarshipPostingApplicantService.CreateScholarshipPostingApplicantAsync(applicantData);
                    if (responseapplicant.Error is not null)
                    {
                        _isValidRegistered = false;
                        _snackbar.AddError(responseapplicant.Error.Detail);
                        return;
                    }

                    var selectionOverviewData = new CreateSelectionOverviewRequest()
                    {
                        //CompanyCode = _user.GetCompanyCode(),
                        //Email = _user.GetUsername(),
                        //EmployeeNumber = _user.GetEmployeeId(),
                        //Name = _user.GetDisplayName(),
                        Status = ApplicantStatus.Awaiting,
                        Stage = SelectionStage.Administration,
                        //ScholarshipPostingApplicantId = responseapplicant.Result.Id
                    };

                    var responseOverview = await _selectionOverviewService.CreateSelectionOverviewAsync(selectionOverviewData);
                    if (responseOverview.Error is not null)
                    {
                        _isValidRegistered = false;
                        _snackbar.AddError(responseOverview.Error.Detail);
                        return;
                    }

                    var administrasiData = new CreateAdministrationApplicantRequest()
                    {
                        Company = _user.GetCompanyCode() ?? "",
                        Email = _user.GetUsername() ?? "",
                        EmployeeNumber = _user.GetEmployeeId() ?? "",
                        Name = _user.GetDisplayName() ?? "",
                        Status = 1,
                        ScholarshipPostingApplicantId = responseapplicant.Result!.Id
                    };

                    //var response = await _selectionAdministrationService.CreateAdministrationApplicantAsync(administrasiData);
                    //if (response.Error is not null)
                    //{
                    //    _isValidRegistered = false;
                    //    _snackbar.AddError(responseapplicant.Error.Detail);
                    //    return;
                    //}
                }
                else
                {
                    _isValidRegistered = false;
                    _snackbar.AddError("Your company code profile not allowed to register this scholarship.");
                    return;
                }
            }
        }
        else
        {
            _error = requirements.Error;
        }

        var hris = await _hrisService.GetEmployeeByIdAsync(_user.GetEmployeeId() ?? string.Empty);

        _isVisiblePopup = false;
        StateHasChanged();
        //_navigationManager.NavigateTo(RouteFor.Index, true);
    }
}

public class ExtendedScholarshipPostingResponse : GetScholarshipPostingApplicantListQueryResponse
{
    public bool Expanded { get; set; } = false;
}
