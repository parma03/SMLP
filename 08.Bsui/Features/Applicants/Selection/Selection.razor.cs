/*
*/

using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using Smlp.Base.Selections.Enums;
using Smlp.Bsui.Common.Constants;
using Smlp.Bsui.Common.Extensions;
using Smlp.Bsui.Features.Applicants.Selection.Components;
using Smlp.Bsui.Features.Applicants.Selection.Constants;
using Smlp.Bsui.Features.Applicants.Selection.Models;
using Smlp.Shared.Applicants.Queries.GetApplicantSchedule;
using Smlp.Shared.Common.Constants;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Scholarships.ScholarshipPosting.Queries;
using Smlp.Shared.Selections.CreateSK.Queries;
using Smlp.Shared.Selections.SelectionOverviews.Queries;
using DialogOptions = MudBlazor.DialogOptions;

namespace Smlp.Bsui.Features.Applicants.Selection;

public partial class Selection
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

    protected ClaimsPrincipal _user = default!;

    protected bool IsLoadingTimeline { get; set; } = false;
    public ErrorResponse? Error { get; set; }
    public Guid ScholarshipId { get; set; }
    public Guid ApplicantId { get; set; }
    private bool Visibility { get; set; }
    private bool ShowButton { get; set; } = false;
    //private bool _showSfDialog = true;
    [Parameter]
    public string? Condition { get; set; }

    public GetScholarshipPostingResponse Scholarship { get; set; } = default!;
    private IList<GetScholarshipPostingResponse> Scholarships { get; set; } = default!;
    public GetSelectionOverviewQueryResponse SelectionOverview { get; set; } = default!;
    private IList<SelectionTimelineModel> SelectionTimelineModels { get; set; } = default!;
    public SelectionType SelectionType { get; set; } = default!;
    public bool ApplicatIsInSelection { get; set; } = false;

    protected bool _isDisableCancel = false;
    protected bool _isLoading = false;
    protected string _displayName = default!;
    protected string _displayTitle = default!;
    protected string _displayDueDate = default!;
    protected string _employeeId = string.Empty;
    protected string _username = string.Empty;
    protected string _postingText = string.Empty;

    private bool VisiblePopup { get; set; }
    private Guid IdScholarshipPlan { get; set; }
    private Guid SPlanId { get; set; }
    private string? ActionType { get; set; }
    private bool IsEnabled { get; set; }
    private string TitlePopup { get; set; } = "View";

    protected override async Task OnInitializedAsync()
    {
        _user = (await AuthenticationStateTask).User;
        _displayName = _user?.GetDisplayName();
        _employeeId = _user?.GetEmployeeId();
        _isLoading = true;

        if (!string.IsNullOrEmpty(Condition))
        {
            Visibility = true;
        }

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
            _isLoading = false;
            return;
        }

        #endregion bypass user applicant

        var request = new GetScholarshipPostingRequest()
        {
            CompanyCode = _user.GetCompanyCode(),
        };

        //get scholarship existing
        var applicant = await _applicantService.GetApplicantByEmployeeIdAsync(_employeeId);
        if (applicant.Error != null)
        {
            Error = applicant.Error;
        }
        else if (applicant.Result!.Items.Count() == 0)
        {
            _snackbar.AddError("Not Found data Applicant");
        }
        else
        {
            var lastApplicant = applicant.Result!.Items.OrderByDescending(x => x.Created).FirstOrDefault();
            ApplicantId = lastApplicant!.Id;
            ScholarshipId = lastApplicant.ScholarshipPostingId;

            // get data posting
            if (ScholarshipId != Guid.Empty)
            {
                var posting = await _postingService.GetScholarshipPostingByIdAsync(ScholarshipId);
                if (posting.Error is null && posting.Result is not null)
                {
                    var item = posting.Result;
                    SPlanId = item.ScholarshipPlanId;
                    _postingText = string.Format(@"{0} {1} {2} {3} {4} {5} - Batch {6}"
                                            , item.CompanyGroupName, item.PostingYear, item.DegreeName,
                                            item.Faculty, item.Major, item.Code, item.Batch);
                }
            }

            SelectionType = lastApplicant.SelectionType;
            if (ApplicantId != Guid.Empty)
            {
                var selection = await _selectionOverviewService.GetSelectionOverviewByApplicantIdAsync(ApplicantId);

                if (selection.Result != null &&
                    (selection.Result.Status == ApplicantStatus.Passed ||
                    selection.Result.Status == ApplicantStatus.InProgress ||
                    selection.Result.Status == ApplicantStatus.InProgressTRM ||
                    selection.Result.Status == ApplicantStatus.InProgressSK ||
                    selection.Result.Status == ApplicantStatus.Approved ||
                    selection.Result.Status == ApplicantStatus.ApprovedDirector ||
                    selection.Result.Status == ApplicantStatus.SKCreated ||
                    selection.Result.Status == ApplicantStatus.SKSend ||
                    selection.Result.Status == ApplicantStatus.InProgressBA ||
                    selection.Result.Status == ApplicantStatus.InProgressPoffice ||
                    selection.Result.Status == ApplicantStatus.InProgressPsign ||
                    (selection.Result.Status == ApplicantStatus.Failed && selection.Result.Stage != SelectionStage.Administration)))
                {
                    ApplicatIsInSelection = true;
                    SelectionOverview = selection.Result!;
                    _isDisableCancel = false;
                    var param = new GetSelectionDecreeFilterRequest();
                    var listSelectionDegree = await _selectionDecreesService.GetSelectionDecreeListAsync(param);
                    var skSendExists = listSelectionDegree.Result.Items.Any(x =>
                        x.Status == ApplicantStatus.SKSend &&
                        x.ScholarshipPostingId == SelectionOverview.PostingId);

                    switch (SelectionOverview.Stage)
                    {
                        case SelectionStage.Administration:
                            SelectionTimelineModels = SelectionTimelineItem.Administration.InProgress();
                            break;

                        case SelectionStage.TalentReview_1:
                            SelectionTimelineModels = SelectionTimelineItem.TalentReview1st.InProgress();
                            break;

                        case SelectionStage.TOEFL:
                            if (selection.Result.Status == ApplicantStatus.Failed)
                            {
                                SelectionTimelineModels = SelectionTimelineItem.Toefl.Failed(SelectionType);
                                break;
                            }
                            else
                            {
                                SelectionTimelineModels = SelectionTimelineItem.Toefl.InProgress(SelectionType);
                                break;
                            }

                        case SelectionStage.Psychotest:
                            if (selection.Result.Status == ApplicantStatus.Failed)
                            {
                                SelectionTimelineModels = SelectionTimelineItem.Psychotest.Failed(SelectionType);
                                break;
                            }
                            else
                            {
                                SelectionTimelineModels = SelectionTimelineItem.Psychotest.InProgress(SelectionType);
                                break;
                            }

                        case SelectionStage.TalentReview:
                            if (skSendExists)
                            {
                                if (selection.Result.Status == ApplicantStatus.Passed)
                                {
                                    _isDisableCancel = true;
                                    SelectionTimelineModels = SelectionTimelineItem.TalentReview.Passed();
                                    break;
                                }
                                else if (selection.Result.Status == ApplicantStatus.Failed)
                                {
                                    SelectionTimelineModels = SelectionTimelineItem.TalentReview.Failed();
                                    break;
                                }
                                else
                                {
                                    SelectionTimelineModels = SelectionTimelineItem.TalentReview.InProgress();
                                    break;
                                }
                            }
                            else
                            {
                                SelectionTimelineModels = SelectionTimelineItem.TalentReview.InProgress();
                                break;
                            }

                        case SelectionStage.Decree or SelectionStage.BeritaAcara:
                            if (skSendExists)
                            {
                                if (selection.Result.Status == ApplicantStatus.Failed)
                                {
                                    SelectionTimelineModels = SelectionTimelineItem.TalentReview.Failed();
                                    break;
                                }

                                break;
                            }
                            else
                            {
                                SelectionTimelineModels = SelectionTimelineItem.TalentReview.InProgress();
                                break;
                            }

                        case SelectionStage.TalentReview_2:
                            if (selection.Result.Status == ApplicantStatus.Passed)
                            {
                                _isDisableCancel = true;
                                SelectionTimelineModels = SelectionTimelineItem.TalentReview2nd.Passed();
                                break;
                            }
                            else
                            {
                                SelectionTimelineModels = SelectionTimelineItem.TalentReview2nd.InProgress();
                                break;
                            }

                        default:
                            break;
                    }

                    _isLoading = false;
                    return;
                }
            }
        }

        _isLoading = false;
    }

    protected async void OnClickScheduleAsync(MouseEventArgs args)
    {
        if (SelectionOverview.Stage == SelectionStage.TOEFL && SelectionOverview.Status == ApplicantStatus.InProgress)
        {
            var scheduleRequest = new GetApplicantScheduleRequest
            {
                ApplicantId = ApplicantId,
                ScholarshipId = ScholarshipId,
            };

            var applicantToelf = await _scheduleService.GetApplicantToeflScheduleAsync(scheduleRequest);
            if (applicantToelf.Error == null && applicantToelf.Result!.Id != Guid.Empty)
            {
                var scheduleSet = $"Your TOEFL Schedule Test already exists on {applicantToelf.Result.ScheduleStart}";
                var options = new DialogOptions { CloseOnEscapeKey = false, DisableBackdropClick = true };
                var parameters = new DialogParameters<DialogExistSchedule> { { x => x.ScheduleSet, scheduleSet } };

                //show dialog apply
                var dialog = await _dialogService.ShowAsync<DialogExistSchedule>("TOEFL Schedule", parameters, options);
                var result = await dialog.Result;
            }
            else
            {
                var options = new DialogOptions { CloseOnEscapeKey = false, DisableBackdropClick = true };
                var parameters = new DialogParameters<DialogToeflSchedule> { { x => x.ScholarshipPostingId, ScholarshipId }, { y => y.ApplicantId, ApplicantId } };

                //show dialog apply
                var dialog = await _dialogService.ShowAsync<DialogToeflSchedule>("TOEFL Schedule", parameters, options);
                var result = await dialog.Result;
            }
        }
        else if (SelectionOverview.Stage == SelectionStage.Psychotest && SelectionOverview.Status == ApplicantStatus.InProgress)
        {
            var scheduleRequest = new GetApplicantScheduleRequest
            {
                ApplicantId = ApplicantId,
                ScholarshipId = ScholarshipId,
            };

            var applicantPsychotest = await _scheduleService.GetApplicantPsychotestScheduleAsync(scheduleRequest);
            if (applicantPsychotest.Error == null && applicantPsychotest.Result!.Id != Guid.Empty)
            {
                var scheduleSet = $"Your Psychotest Schedule already exists on {applicantPsychotest.Result.ScheduleStart}";
                var options = new DialogOptions { CloseOnEscapeKey = false, DisableBackdropClick = true };
                var parameters = new DialogParameters<DialogExistSchedule> { { x => x.ScheduleSet, scheduleSet } };

                //show dialog apply
                var dialog = await _dialogService.ShowAsync<DialogExistSchedule>("Psychotest Schedule", parameters, options);
                var result = await dialog.Result;
            }
            else
            {
                var options = new DialogOptions { CloseOnEscapeKey = false, DisableBackdropClick = true };
                var parameters = new DialogParameters<DialogPsychotestSchedule> { { x => x.ScholarshipPostingId, ScholarshipId }, { y => y.ApplicantId, ApplicantId } };

                //show dialog apply
                var dialog = await _dialogService.ShowAsync<DialogPsychotestSchedule>("Psychotest Schedule", parameters, options);
                var result = await dialog.Result;
            }
        }
        else
        {
            _snackbar.AddWarning(NotificationMessageFor.NotAllowScheduleSet);
        }
    }

    protected async void OnClickCancelAsync(MouseEventArgs args)
    {
        if (!_isDisableCancel)
        {
            var options = new DialogOptions { CloseOnEscapeKey = false, DisableBackdropClick = true };
            var parameters = new DialogParameters<DialogCancelApplicant> { { x => x.ApplicantId, ApplicantId } };

            //show dialog apply
            var dialog = await _dialogService.ShowAsync<DialogCancelApplicant>("Cancel Applicant", parameters, options);
            var result = await dialog.Result;
        }
    }

    //Dialog direct to Selection
    //private void DialogOpen(object args)
    //{
    //    ShowButton = false;
    //    Console.WriteLine("Dialog Opened");
    //}
    //private void DialogClose(object args)
    //{
    //    ShowButton = true;
    //    Console.WriteLine("Dialog Closed");
    //}
    //private void OnButtonClick()
    //{
    //    Visibility = true;
    //}
    //private void DialogButtonClick()
    //{
    //    Visibility = false;
    //    _showSfDialog = true;
    //}

    //Dialog Specific Requirement
    private async Task OnCommandClicked()
    {
        ActionType = "view";
        TitlePopup = "View Specific Requirement";
        IdScholarshipPlan = SPlanId;
        IsEnabled = true;
        VisiblePopup = true;
        DialogOpen();
    }
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
