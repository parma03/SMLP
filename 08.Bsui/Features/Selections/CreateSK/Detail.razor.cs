using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using Smlp.Base.Selections.Enums;
using Smlp.Bsui.Common.Constants;
using Smlp.Bsui.Common.Extensions;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Selections.BeritaAcara.Commands.Update;
using Smlp.Shared.Selections.Constants;
using Smlp.Shared.Selections.CreateSK.Commands.Update;
using Smlp.Shared.Selections.CreateSK.Queries;
using Smlp.Shared.Selections.SelectionOverviews.Commands.Update;
using Smlp.Shared.Selections.SelectionTalentReviews.Commands.Update;
using Syncfusion.Blazor.Inputs;
using RouteFor = Smlp.Bsui.Features.Selections.CreateSK.Constants.RouteFor;

namespace Smlp.Bsui.Features.Selections.CreateSK;

public partial class Detail
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

    protected ClaimsPrincipal _user = default!;

    [Parameter]
    public Guid Id { get; set; }

    protected ErrorResponse? _error;
    private List<BreadcrumbItem> _breadcrumbItems = new();
    protected GetSelectionCreateSKByIdQueryResponse _response = new();
    protected bool _isLoading = false;
    protected bool _isLoadingData = false;
    protected string? IsLoadingDataTitle { get; set; }
    protected bool _isDisabledButtonFailed = true;
    protected bool _isDisabledButtonApprove = true;
    protected SfTextBox? _textBoxNopek;
    protected string? DRHSContent { get; set; }
    protected List<ListScopeResponse> _listScope = new();
    protected DecreeStage SelectionDecreStage { get; set; }
    protected ApplicantStatus SelectionApplicantStatus { get; set; }

    protected override async Task OnInitializedAsync()
    {
        SetupBreadcrumb();
        _user = (await AuthenticationStateTask).User;
        if (_user.Identity!.IsAuthenticated)
        {
            _isLoading = true;
            var scope = _user.Claims.Where(p => p.Type.ToLower().Trim() == "permission").ToList();
            if (scope.Count > 0)
            {
                foreach (var item in scope)
                {
                    _listScope.Add(new ListScopeResponse
                    {
                        ScopeName = item.Value
                    });
                }
            }

            await GetData();

            _isLoading = false;
        }
        else
        {
            _navigationManager.NavigateTo(Common.Pages.Errors.Constants.RouteFor.Forbidden, true);
            _isLoading = false;
        }
    }

    private void SetupBreadcrumb()
    {
        _breadcrumbItems = new()
        {
            CommonBreadcrumbFor.Active(DisplayTextFor.CompanySelection),
            CommonBreadcrumbFor.Active(DisplayTextFor.OpenRegistration),
            CommonBreadcrumbFor.Active(DisplayTextFor.CreateSK),
            CommonBreadcrumbFor.Active(DisplayTextFor.CreateSKDetails)
        };
    }

    private async Task GetData()
    {
        var response = await _selectionDecreesService.GetSelectionDecreeDetailAsync(Id);

        if (response.Error is null)
        {
            _response = response.Result!;
            if (_response.Status == ApplicantStatus.Failed)
            {
                _isDisabledButtonFailed = true;
                _isDisabledButtonApprove = false;
            }
            else if (_response.Status == ApplicantStatus.Passed)
            {
                _isDisabledButtonFailed = false;
                _isDisabledButtonApprove = true;
            }
            else if (_response.Status == ApplicantStatus.SKSend)
            {
                _isDisabledButtonFailed = true;
                _isDisabledButtonApprove = true;
            }
            else
            {
                _isDisabledButtonFailed = false;
                _isDisabledButtonApprove = false;
            }

            //if (_listScope.Where(x => x.ScopeName!.ToLower().Contains("admin") || x.ScopeName.ToLower().Contains("psmp.selection.trm.crud")).Count() > 0)
            //{
            //    _isDisabledButtonFailed = false;
            //    _isDisabledButtonApprove = false;
            //}
            //else
            //{
            //    _isDisabledButtonFailed = true;
            //    _isDisabledButtonApprove = true;
            //}
        }
        else
        {
            _error = response.Error;
        }
    }

    protected void OnClickBack()
    {
        _navigationManager.NavigateTo(RouteFor.Selections());
    }

    protected async void OnClickReject()
    {
        _isLoadingData = true;
        IsLoadingDataTitle = "Loading...";
        var decreeRequest = new UpdateSelectionDecreeRequest
        {
            Id = Id,
            Status = ApplicantStatus.Failed,
            DecreeStage = _response.Stage,
        };

        var decree = await _selectionDecreesService.UpdateSelectionDecreeAsync(decreeRequest);
        if (decree.Error is not null)
        {
            var result = string.Empty;
            for (var i = 0; i < decree.Error.Details.Count; i++)
            {
                result = result + decree.Error.Details[i] + ",";
            }

            _snackbar.AddError("Decree - Error : " + result);
            _isLoadingData = false;
            IsLoadingDataTitle = "";
            StateHasChanged();
            return;
        }

        // update applicant
        UpdateApplicantStatus(ApplicantStatus.Failed);

        _snackbar.AddSuccess("Success fail applicant");
        _isLoadingData = false;
        IsLoadingDataTitle = "";
        await GetData();
        StateHasChanged();
    }

    protected async void OnClickApprove()
    {
        _isLoadingData = true;
        IsLoadingDataTitle = "Loading...";
        var decreeRequest = new UpdateSelectionDecreeRequest
        {
            Id = Id,
            Status = ApplicantStatus.Passed,
            DecreeStage = DecreeStage.Manager,
        };

        var decree = await _selectionDecreesService.UpdateSelectionDecreeAsync(decreeRequest);
        if (decree.Error is not null)
        {
            var result = string.Empty;
            for (var i = 0; i < decree.Error.Details.Count; i++)
            {
                result = result + decree.Error.Details[i] + ",";
            }

            _snackbar.AddError("Decree - Error : " + result);
            _isLoadingData = false;
            IsLoadingDataTitle = "";
            StateHasChanged();
            return;
        }

        // update applicant
        UpdateApplicantStatus(ApplicantStatus.InProgressSK);

        _snackbar.AddSuccess("Success pass applicant");
        _isLoadingData = false;
        IsLoadingDataTitle = "";
        await GetData();
        StateHasChanged();
    }

    protected async void OnClickGetDRHS()
    {
        _isLoadingData = true;
        IsLoadingDataTitle = "Retrieving data form HRIS Service ...";
        var dataDRHS = await _iamService.GetDrhs(_response.EmployeeNumber);
        if (dataDRHS is not null)
        {
            DRHSContent = dataDRHS;
            _isLoadingData = false;
            StateHasChanged();
        }
        else
        {
            _snackbar.AddWarning("Data DRHS not found !");
            _isLoadingData = false;
            IsLoadingDataTitle = "";
            StateHasChanged();
        }
    }

    private async void UpdateApplicantStatus(ApplicantStatus status)
    {
        var reqBa = new UpdateSelectionBeritaAcaraApplicantRequest
        {
            ScholarshipPostingApplicantId = _response.ApplicationId,
            Status = status,
        };

        await _selectionBeritaAcaraService.UpdateSelectionApplicantBeritAcaraApplicantAsync(reqBa);

        var request = new UpdateSelectionTalentReviewRequest
        {
            Id = _response.TalentReviewId,
            ApplicantId = _response.ApplicationId,
            Status = status,
        };

        await _selectionTalentReviewService.UpdateSelectionTalentReviewAsync(request);

        var updateOverviewRequest = new UpdateSelectionOverviewByApplicantIdRequest
        {
            ApplicantId = _response.ApplicationId,
            Stage = SelectionStage.Decree,
            Status = status,
        };

        await _selectionOverviewService.UpdateSelectionOverviewByApplicantIdAsync(updateOverviewRequest);
    }
}
