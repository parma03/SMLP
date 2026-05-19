using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using Psmp.Base.Selections.Enums;
using Psmp.Bsui.Common.Constants;
using Psmp.Bsui.Common.Extensions;
using Psmp.Shared.Common.Responses;
using Psmp.Shared.Selections.BeritaAcara.Commands.Update;
using Psmp.Shared.Selections.BeritaAcara.Queries;
using Psmp.Shared.Selections.Constants;
using Psmp.Shared.Selections.SelectionOverviews.Commands.Update;
using Psmp.Shared.Selections.SelectionTalentReviews.Queries;
using Syncfusion.Blazor.Inputs;
using RouteFor = Psmp.Bsui.Features.Selections.BeritaAcara.Constants.RouteFor;

namespace Psmp.Bsui.Features.Selections.BeritaAcara;

public partial class Detail
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;
    [Parameter]
    public int Type { get; set; }
    protected ClaimsPrincipal _user = default!;
    protected List<ListScopeResponse> _listScope = new();
    [Parameter]
    public Guid Id { get; set; }
    protected ErrorResponse? _error;
    private List<BreadcrumbItem> _breadcrumbItems = new();
    protected GetSelectionTalentReviewByIdQueryResponse _response = new();
    protected bool _isLoading = false;
    protected bool _isLoadingData = false;
    protected bool _isDisabledButtonFailed = false;
    protected bool _isDisabledButtonApprove = false;
    protected string _contentButtonSubmit = string.Empty;
    protected SfTextBox? _textBoxNopek;
    protected SfTextBox? _textBoxNopek2;

    protected string? DRHSContent { get; set; }

    protected override async Task OnInitializedAsync()
    {
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

            SetupBreadcrumb();
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
            CommonBreadcrumbFor.Active(DisplayTextFor.BeritaAcara),
            CommonBreadcrumbFor.Active(DisplayTextFor.BeritaAcaraDetail)
        };
    }
    private async Task GetData()
    {
        var response = await _selectionTalentReviewService.GetSelectionTalentReviewDetailAsync(Id);

        if (response.Error is null)
        {
            _response = response.Result!;
            var skData = await _selectionDecreesService.GetSelectionDecreeByApplicantIdAsync(_response.ApplicationId);

            if (_response.Status == ApplicantStatus.InProgressBA)
            {
                var param = new GetSelectionBeritaAcaraFilterRequest
                {
                    Code = _response.ScholarshipCode,
                    SelectionType = (SelectionType)Type
                };

                var responseBA = await _selectionBeritaAcaraService.GetSelectionApplicantBeritaListAsync(param);
                var matchingApplicant = responseBA.Result.Items
                    .FirstOrDefault(x => x.ApplicantId == _response.ApplicationId);

                var hasScope =
                    _listScope.Any(x =>
                        x.ScopeName != null &&
                        (x.ScopeName.Contains("admin", StringComparison.OrdinalIgnoreCase) ||
                         x.ScopeName.Contains("psmp.selection.trm.crud", StringComparison.OrdinalIgnoreCase)));

                if (matchingApplicant is not null)
                {
                    if (matchingApplicant.Status == ApplicantStatus.Failed)
                    {
                        _contentButtonSubmit = "Pass";
                        _isDisabledButtonFailed = true;
                        _isDisabledButtonApprove = false;
                    }
                    else if (matchingApplicant.Status == ApplicantStatus.Passed)
                    {
                        _contentButtonSubmit = "Pass";
                        _isDisabledButtonFailed = false;
                        _isDisabledButtonApprove = true;
                    }
                    else
                    {
                        _contentButtonSubmit = "Pass";
                        _isDisabledButtonFailed = false;
                        _isDisabledButtonApprove = false;
                    }
                }
            }
            else if (_response.Status == ApplicantStatus.InProgressSK)
            {
                _contentButtonSubmit = "Pass";
                _isDisabledButtonFailed = true;
                _isDisabledButtonApprove = true;
            }
            else if (skData.Result.Status == ApplicantStatus.SKSend)
            {
                _contentButtonSubmit = "Pass";
                _isDisabledButtonFailed = true;
                _isDisabledButtonApprove = true;
            }
        }
        else
        {
            _error = response.Error;
        }
    }
    protected void OnClickBack()
    {
        if (Type == 0)
        {
            _navigationManager.NavigateTo(RouteFor.SelectionsClose(), true);
        }
        else
        {
            _navigationManager.NavigateTo(RouteFor.Selections(), true);
        }
    }
    protected async void OnClickFailed()
    {
        _isLoadingData = true;
        var request = new UpdateSelectionBeritaAcaraApplicantRequest
        {
            ScholarshipPostingApplicantId = _response.ApplicationId,
            Status = Base.Selections.Enums.ApplicantStatus.Failed,
        };

        var response = await _selectionBeritaAcaraService.UpdateSelectionApplicantBeritAcaraApplicantAsync(request);
        if (response.Error is null)
        {
            var updateOverviewRequest = new UpdateSelectionOverviewByApplicantIdRequest
            {
                ApplicantId = _response.ApplicationId,
                Stage = SelectionStage.BeritaAcara,
                Status = ApplicantStatus.Failed,
            };

            await _selectionOverviewService.UpdateSelectionOverviewByApplicantIdAsync(updateOverviewRequest);
            _snackbar.AddSuccess("Update data success");
            await GetData();
        }
        else
        {
            var result = string.Empty;
            for (var i = 0; i < response.Error.Details.Count; i++)
            {
                result = result + response.Error.Details[i] + ",";
            }

            _snackbar.AddError(result);
        }

        _isLoadingData = false;
        await GetData();
        StateHasChanged();
    }
    protected async void OnClickPassed()
    {
        _isLoadingData = true;
        var request = new UpdateSelectionBeritaAcaraApplicantRequest
        {
            ScholarshipPostingApplicantId = _response.ApplicationId,
            Status = Base.Selections.Enums.ApplicantStatus.Passed,
        };

        var response = await _selectionBeritaAcaraService.UpdateSelectionApplicantBeritAcaraApplicantAsync(request);
        if (response.Error is null)
        {
            var updateOverviewRequest = new UpdateSelectionOverviewByApplicantIdRequest
            {
                ApplicantId = _response.ApplicationId,
                Stage = SelectionStage.BeritaAcara,
                Status = ApplicantStatus.InProgressBA,
            };

            await _selectionOverviewService.UpdateSelectionOverviewByApplicantIdAsync(updateOverviewRequest);
            _snackbar.AddSuccess("Update data success");
            await GetData();
        }
        else
        {
            var result = string.Empty;
            for (var i = 0; i < response.Error.Details.Count; i++)
            {
                result = result + response.Error.Details[i] + ",";
            }

            _snackbar.AddError(result);
        }

        _isLoadingData = false;
        StateHasChanged();
    }
    private async void CreateTextboxNopex()
    {
        var click = EventCallback.Factory.Create<MouseEventArgs>(this, async () =>
        {
            var employeeId = $"00000000{_response.MentorId}";
            _response.MentorId = employeeId[^8..];

            var mentorIdForSearch = _response?.MentorId is not null ? _response.MentorId : null;
            await OnClickSearch(mentorIdForSearch);
        });

        await _textBoxNopek!.AddIconAsync("append", "e-icons e-search", new Dictionary<string, object>() { { "onclick", click } });
    }

    private async void CreateTextboxNopex2()
    {
        var click = EventCallback.Factory.Create<MouseEventArgs>(this, async () =>
        {
            var employeeId = $"00000000{_response.MentorId2}";
            _response.MentorId2 = employeeId[^8..];

            var mentorIdForSearch = _response?.MentorId2 is not null ? _response.MentorId2 : null;
            await OnClickSearch2(mentorIdForSearch);
        });

        await _textBoxNopek2!.AddIconAsync("append", "e-icons e-search", new Dictionary<string, object>() { { "onclick", click } });
    }
    public async Task OnClickSearch(string? mentorIdForSearch)
    {
        if (string.IsNullOrEmpty(mentorIdForSearch))
        {
            _snackbar.AddWarning("Please input employee number");
            return;
        }

        _isLoadingData = true;

        var dataHRIS = await _hrisService.GetEmployeeByIdAsync(mentorIdForSearch);
        if (dataHRIS is not null)
        {
            _response!.MentorName = dataHRIS.EmployeeName;
            _response.MentorEmail = dataHRIS.Email;
            _response.MentorPosition = dataHRIS.PositionName;
            _snackbar.AddInfo("Success retrieve data from HRIS");
        }
        else
        {
            _snackbar.AddError("Employee not found in HRIS!");
        }

        _isLoadingData = false;
        StateHasChanged();
    }

    public async Task OnClickSearch2(string? mentorIdForSearch)
    {
        if (string.IsNullOrEmpty(mentorIdForSearch))
        {
            _snackbar.AddWarning("Please input employee number");
            return;
        }

        _isLoadingData = true;

        var dataHRIS = await _hrisService.GetEmployeeByIdAsync(mentorIdForSearch);
        if (dataHRIS is not null)
        {
            _response!.MentorName2 = dataHRIS.EmployeeName;
            _response.MentorEmail2 = dataHRIS.Email;
            _response.MentorPosition2 = dataHRIS.PositionName;
            _snackbar.AddInfo("Success retrieve data from HRIS");
        }
        else
        {
            _snackbar.AddError("Employee not found in HRIS!");
        }

        _isLoadingData = false;
        StateHasChanged();
    }
    protected async void OnClickGetDRHS()
    {
        _isLoadingData = true;
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
        }
    }
}
