using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using Smlp.Base.Selections.Enums;
using Smlp.Bsui.Common.Constants;
using Smlp.Bsui.Common.Extensions;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Projects.ProjectCharterPlans.Commands;
using Smlp.Shared.Projects.ProjectCharterPlans.Queries;
using Smlp.Shared.Projects.ProjectCharterRealisations.Commands;
using Smlp.Shared.Projects.ProjectCharterRealisations.Queries;
using Smlp.Shared.Projects.ProjectPlans.Commands;
using Smlp.Shared.Selections.BeritaAcara.Commands.Create;
using Smlp.Shared.Selections.Constants;
using Smlp.Shared.Selections.CreateSK.Commands.Update;
using Smlp.Shared.Selections.CreateSK.Queries;
using Smlp.Shared.Selections.SelectionOverviews.Commands.Update;
using Smlp.Shared.Selections.SelectionTalentReviews.Commands.Update;
using Smlp.Shared.Selections.SelectionTalentReviews.Queries;
using Syncfusion.Blazor.Inputs;
using RouteFor = Smlp.Bsui.Features.Selections.TalentReview.Constants.RouteFor;

namespace Smlp.Bsui.Features.Selections.TalentReview;

public partial class Detail
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

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
            CommonBreadcrumbFor.Active(DisplayTextFor.TalentReviewMeeting),
            CommonBreadcrumbFor.Active(DisplayTextFor.TalentReviewMeetingDetails)
        };
    }
    private async Task GetData()
    {
        var response = await _selectionTalentReviewService.GetSelectionTalentReviewDetailAsync(Id);

        if (response.Error is null)
        {
            _response = response.Result!;

            //if (_response.Status == ApplicantStatus.InProgressTRM)
            //{
            //    _contentButtonSubmit = "Pass & Continue to Create BA";
            //    if (_listScope.Where(x => x.ScopeName!.ToLower().Contains("admin") || x.ScopeName.ToLower().Contains("psmp.selection.trm.crud")).Count() > 0)
            //    {
            //        _isDisabledButtonFailed = false;
            //        _isDisabledButtonApprove = false;
            //    }
            //    else
            //    {
            //        _isDisabledButtonFailed = true;
            //        _isDisabledButtonApprove = true;
            //    }
            //}
            //else
            //{
            //    if (_response.Status != ApplicantStatus.Failed)
            //    {
            //        _contentButtonSubmit = "Update";
            //        _isDisabledButtonFailed = true;
            //        _isDisabledButtonApprove = false;
            //    }
            //    else if (_response.Status != ApplicantStatus.Passed)
            //    {
            //        _contentButtonSubmit = "Update";
            //        _isDisabledButtonFailed = true;
            //        _isDisabledButtonApprove = true;
            //    }
            //    else
            //    {
            //        _contentButtonSubmit = "Update";
            //        _isDisabledButtonFailed = true;
            //        _isDisabledButtonApprove = true;
            //    }
            //}

            var skData = await _selectionDecreesService.GetSelectionDecreeByApplicantIdAsync(response.Result.ApplicationId);

            if (_response.Status == ApplicantStatus.Failed)
            {
                _contentButtonSubmit = "Pass & Continue to Create BA";
                _isDisabledButtonFailed = true;
                _isDisabledButtonApprove = false;
            }
            else if (_response.Status == ApplicantStatus.InProgressBA)
            {
                _contentButtonSubmit = "Update";
                _isDisabledButtonFailed = true;
                _isDisabledButtonApprove = false;
            }
            else if (_response.Status == ApplicantStatus.Passed && skData.Result.Status == ApplicantStatus.SKSend)
            {
                _contentButtonSubmit = "Update";
                _isDisabledButtonFailed = true;
                _isDisabledButtonApprove = false;
            }
            else
            {
                _contentButtonSubmit = "Pass & Continue to Create BA";
                _isDisabledButtonFailed = false;
                _isDisabledButtonApprove = false;
            }

            if (_response.InnovationProject == null)
            {
                _response.InnovationProject = _response.InnovationProjectSCPlant;
            }
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
    protected async void OnClickFailed()
    {
        _isLoadingData = true;
        var request = new UpdateSelectionTalentReviewRequest
        {
            Id = Id,
            ApplicantId = _response.ApplicationId,
            Status = Base.Selections.Enums.ApplicantStatus.Failed,
        };

        var response = await _selectionTalentReviewService.UpdateSelectionTalentReviewAsync(request);
        if (response.Error is null)
        {
            var updateOverviewRequest = new UpdateSelectionOverviewByApplicantIdRequest
            {
                ApplicantId = _response.ApplicationId,
                Stage = SelectionStage.TalentReview,
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
    private bool ValidateMentorRequirement()
    {
        // check
        var hasMentor1 = !string.IsNullOrEmpty(_response?.MentorId) &&
                          !string.IsNullOrEmpty(_response?.MentorName);

        var hasMentor2 = !string.IsNullOrEmpty(_response?.MentorId2) &&
                          !string.IsNullOrEmpty(_response?.MentorName2);

        if (!hasMentor1 && !hasMentor2)
        {
            _snackbar.AddError("Minimum 1 mentor must be filled!");
            return false;
        }

        return true;
    }
    protected async void OnClickPassed()
    {
        if (!ValidateMentorRequirement())
        {
            return;
        }

        _isLoadingData = true;
        var request = new UpdateSelectionTalentReviewRequest
        {
            Id = Id,
            ApplicantId = _response.ApplicationId,
            InnovationProject = _response.InnovationProject,
            MentorId = _response.MentorId,
            MentorName = _response.MentorName,
            MentorEmail = _response.MentorEmail,
            MentorPosition = _response.MentorPosition,
            MentorId2 = _response.MentorId2,
            MentorName2 = _response.MentorName2,
            MentorEmail2 = _response.MentorEmail2,
            MentorPosition2 = _response.MentorPosition2,
        };

        var skData = await _selectionDecreesService.GetSelectionDecreeByApplicantIdAsync(_response.ApplicationId);

        if (_response.Status == ApplicantStatus.InProgressTRM)
        {
            request.Status = ApplicantStatus.InProgressBA;
        }
        else if (_response.Status == ApplicantStatus.Failed)
        {
            request.Status = ApplicantStatus.InProgressBA;
        }
        else if (skData.Result.Status == ApplicantStatus.SKCreated)
        {
            request.Status = _response.Status;
        }
        else if (skData.Result.Status == ApplicantStatus.SKSend)
        {
            request.Status = _response.Status;
        }

        var response = await _selectionTalentReviewService.UpdateSelectionTalentReviewAsync(request);
        if (response.Error is null)
        {
            if (_response.Status == ApplicantStatus.InProgressTRM)
            {
                var updateOverviewRequest = new UpdateSelectionOverviewByApplicantIdRequest
                {
                    ApplicantId = _response.ApplicationId,
                    Stage = SelectionStage.BeritaAcara,
                    Status = ApplicantStatus.InProgressBA,
                };

                await _selectionOverviewService.UpdateSelectionOverviewByApplicantIdAsync(updateOverviewRequest);
            }
            else if (_response.Status == ApplicantStatus.Failed)
            {
                var updateOverviewRequest = new UpdateSelectionOverviewByApplicantIdRequest
                {
                    ApplicantId = _response.ApplicationId,
                    Stage = SelectionStage.BeritaAcara,
                    Status = ApplicantStatus.InProgressBA,
                };

                await _selectionOverviewService.UpdateSelectionOverviewByApplicantIdAsync(updateOverviewRequest);
            }
            else if (skData.Result.Status == ApplicantStatus.SKCreated)
            {
                var updateOverviewRequest = new UpdateSelectionOverviewByApplicantIdRequest
                {
                    ApplicantId = _response.ApplicationId,
                    Stage = SelectionStage.TalentReview,
                    Status = _response.Status,
                };

                await _selectionOverviewService.UpdateSelectionOverviewByApplicantIdAsync(updateOverviewRequest);
            }
            else if (skData.Result.Status == ApplicantStatus.SKSend)
            {
                var updateOverviewRequest = new UpdateSelectionOverviewByApplicantIdRequest
                {
                    ApplicantId = _response.ApplicationId,
                    Stage = SelectionStage.TalentReview,
                    Status = _response.Status,
                };

                await _selectionOverviewService.UpdateSelectionOverviewByApplicantIdAsync(updateOverviewRequest);
            }

            #region update ProjectPlan
            var getProjectPlan = await _projectPlansService.GetProjectPlanByApplicantIdAsync(request.ApplicantId);
            if (getProjectPlan.Result != null)
            {
                if (getProjectPlan.Result.Status != Base.Project.Enums.ProjectPlanStatus.Approved)
                {
                    var requestProjectPlan = new ProjectPlanRequest()
                    {
                        Id = getProjectPlan.Result.Id,
                        ApplicantId = getProjectPlan.Result.ApplicantId,
                        ProjectTitle = getProjectPlan.Result.Title,
                        Abstract = getProjectPlan.Result.Abstract,
                        Status = (int)getProjectPlan.Result.Status,
                        InnovationProject = getProjectPlan.Result.InnovationProject,
                        DocumentProposalId = getProjectPlan.Result.DocumentProposalId,
                        MentorId = _response.MentorId,
                        MentorName = _response.MentorName,
                        MentorEmail = _response.MentorEmail,
                        MentorPosition = _response.MentorPosition,
                        MentorId2 = _response.MentorId2,
                        MentorName2 = _response.MentorName2,
                        MentorEmail2 = _response.MentorEmail2,
                        MentorPosition2 = _response.MentorPosition2
                    };

                    await _projectPlansService.UpdateProjectPlanAsync(requestProjectPlan);
                }
            }
            #endregion

            #region update ProjectCharterPlan
            var requestCharterPlanId = new GetProjectCharterPlanRequest()
            {
                ApplicantId = request.ApplicantId,
            };

            var getProjectCharterPlan = await _projectCharterPlansService.GetProjectCharterPlanByApplicantIdAsync(requestCharterPlanId);
            if (getProjectCharterPlan.Result != null)
            {
                if (getProjectCharterPlan.Result.Status != Base.Project.Enums.ProjectCharterPlanStatus.Approved)
                {
                    var requestProjectCharterPlan = new ProjectCharterPlanRequest()
                    {
                        Id = getProjectCharterPlan.Result.Id,
                        ApplicantId = getProjectCharterPlan.Result.ApplicantId,
                        ProjectTitle = getProjectCharterPlan.Result.ProjectTitle,
                        Abstract = getProjectCharterPlan.Result.Abstract,
                        Status = (int)getProjectCharterPlan.Result.Status,
                        InnovationProject = getProjectCharterPlan.Result.InnovationProject,
                        DocumentProjectCharterId = getProjectCharterPlan.Result.DocumentProjectCharterId,
                        MentorId = _response.MentorId,
                        MentorName = _response.MentorName,
                        MentorEmail = _response.MentorEmail,
                        MentorPosition = _response.MentorPosition,
                        MentorId2 = _response.MentorId2,
                        MentorName2 = _response.MentorName2,
                        MentorEmail2 = _response.MentorEmail2,
                        MentorPosition2 = _response.MentorPosition2
                    };

                    await _projectCharterPlansService.UpdateProjectCharterPlanAsync(requestProjectCharterPlan);
                }
            }
            #endregion

            #region update ProjectCharterRealization
            var requestProjectCharterRealizationId = new GetProjectCharterRealizationRequest()
            {
                ApplicantId = request.ApplicantId,
            };
            var getProjectCharterRealization = await _projectCharterRealisationService.GetProjectCharterRealisationsByApplicantIdAsync(requestProjectCharterRealizationId);
            if (getProjectCharterRealization.Result != null)
            {
                if (getProjectCharterRealization.Result.Status != Base.Project.Enums.ProjectCharterRealizationStatus.Approved)
                {
                    var requestProjectCharterRealization = new ProjectCharterRealizationRequest()
                    {
                        Id = getProjectCharterRealization.Result.Id,
                        ApplicantId = getProjectCharterRealization.Result.ApplicantId,
                        ProjectTitle = getProjectCharterRealization.Result.ProjectTitle,
                        Abstract = getProjectCharterRealization.Result.Abstract,
                        Status = (int)getProjectCharterRealization.Result.Status,
                        InnovationProject = getProjectCharterRealization.Result.InnovationProject,
                        DocumentRealizationId = getProjectCharterRealization.Result.DocumentRealizationId,
                        MentorId = _response.MentorId,
                        MentorName = _response.MentorName,
                        MentorEmail = _response.MentorEmail,
                        MentorPosition = _response.MentorPosition,
                        MentorId2 = _response.MentorId2,
                        MentorName2 = _response.MentorName2,
                        MentorEmail2 = _response.MentorEmail2,
                        MentorPosition2 = _response.MentorPosition2
                    };

                    await _projectCharterRealisationService.UpdateProjectCharterRealisationAsync(requestProjectCharterRealization);
                }
            }
            #endregion

            var decreeRequest = new CreateSelectionBeritaAcaraApplicantRequest
            {
                ScholarshipPostingApplicantId = _response.ApplicationId,
                Status = ApplicantStatus.InProgress
            };

            var createSelectionBeritaAcaraApolicant = await _selectionBeritaAcaraService.CreateSelectionBeritAcaraApplicantAsync(decreeRequest);
            if (createSelectionBeritaAcaraApolicant.Error is not null)
            {
                _snackbar.AddError("Berita Acara Applicant - Error : " + createSelectionBeritaAcaraApolicant.Error.ToDetail());
                StateHasChanged();
                return;
            }

            #region update sk status jika sudah ada sk
            var param = new GetSelectionDecreeFilterRequest();
            var checkdecrees = await _selectionDecreesService.GetSelectionDecreeListAsync(param);
            var decree = checkdecrees.Result?.Items.FirstOrDefault(x =>
                x.ApplicantId == _response.ApplicationId &&
                x.ScholarshipPostingId == skData.Result.ScholarshipPostingId);

            if (decree is not null)
            {
                await _selectionDecreesService.UpdateSelectionDecreeAsync(new UpdateSelectionDecreeRequest
                {
                    Id = decree.Id,
                    DecreeStage = decree.Stage,
                    Status = decree.Status,
                    MentorId = request.MentorId,
                    MentorEmail = request.MentorEmail,
                    MentorName = request.MentorName,
                    MentorPosition = request.MentorPosition,
                    MentorId2 = request.MentorId2,
                    MentorEmail2 = request.MentorEmail2,
                    MentorName2 = request.MentorName2,
                    MentorPosition2 = request.MentorPosition2
                });
            }
            #endregion

            _snackbar.AddSuccess("Update data success");
            await GetData();
        }
        else
        {
            _snackbar.AddError(response.Error.ToDetail());
            StateHasChanged();
            return;
        }

        _isLoadingData = false;
        StateHasChanged();
    }
    //private async void CreateTextboxNopex()
    //{
    //    var click = EventCallback.Factory.Create<MouseEventArgs>(this, OnClickSearch);
    //    await _textBoxNopek!.AddIconAsync("append", "e-icons e-search", new Dictionary<string, object>() { { "onclick", click } });
    //}
    //public async void OnClickSearch()
    //{
    //    if (_response!.MentorId is null)
    //    {
    //        _snackbar.AddWarning("Please input employee number");
    //        return;
    //    }

    //    _isLoadingData = true;
    //    var dataHRIS = await _hrisService.GetEmployeeByIdAsync(_response.MentorId);
    //    if (dataHRIS is not null)
    //    {
    //        _response.MentorName = dataHRIS.EmployeeName;
    //        _response.MentorEmail = dataHRIS.Email;
    //        _response.MentorPosition = dataHRIS.PositionName;
    //        _snackbar.AddInfo("Success retrive data from HRIS");
    //        _isLoadingData = false;
    //        StateHasChanged();
    //    }
    //    else
    //    {
    //        _snackbar.AddError("Employee not found in HRIS !");
    //        _isLoadingData = false;
    //        StateHasChanged();
    //    }
    //}

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
