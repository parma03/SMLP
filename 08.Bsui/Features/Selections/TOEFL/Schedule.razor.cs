using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using Smlp.Base.Selections.Enums;
using Smlp.Bsui.Common.Constants;
using Smlp.Bsui.Common.Extensions;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Emails.Commands.SendEmailSchedule;
using Smlp.Shared.Selections.Constants;
using Smlp.Shared.Selections.SelectionTOEFLs.Commands.Create;
using Smlp.Shared.Selections.SelectionTOEFLs.Queries;
using Syncfusion.Blazor.Calendars;
using Syncfusion.Blazor.Grids;
using RouteFor = Smlp.Bsui.Features.Selections.TOEFL.Constants.RouteFor;

namespace Smlp.Bsui.Features.Selections.TOEFL;

public partial class Schedule
{
    [Parameter]
    public Guid Id { get; set; }

    [Parameter]
    public int Type { get; set; }

    protected ErrorResponse? _error;
    protected bool _isLoading = false;
    protected bool _isLoadingData = false;
    private List<BreadcrumbItem> _breadcrumbItems = new();
    protected List<GetSelectionTOEFLScheduleListResponse> _getSelectionTOEFLScheduleListResponses = new();
    public CreateSelectionTOEFLScheduleRequest _createSelectionTOEFLScheduleRequest = new();
    public CreateSelectionTOEFLScheduleRequest _createSelectionTOEFLScheduleSession2Request = new();
    protected List<GetSelectionTOEFLScheduleChartResponse>? _getSelectionTOEFLScheduleChartResponse = new();
    //protected CreateSelectionTOEFLScheduleRequest _createRequest = new();

    private bool VisiblePopup { get; set; }
    private bool VisiblePopupDelete { get; set; } = false;
    private string TitlePopup { get; set; } = "Add Schedule";
    protected string _scheduleSummary = "0";
    protected string _doneSummary = "0";
    protected string _notStartedSummary = "0";

    #region Confirm
    private string TitlePopupConfirm { get; set; } = "";
    private bool VisiblePopupConfirm { get; set; } = false;
    private string TypeConfirm { get; set; } = "";
    #endregion

    protected override async Task OnInitializedAsync()
    {
        _isLoading = true;
        SetupBreadcrumb();
        //await GetToeflScheduleChartAsync();
        await SetDataOnLoadAsync();
        _isLoading = false;
    }

    private void SetupBreadcrumb()
    {
        _breadcrumbItems = new()
        {
            CommonBreadcrumbFor.Active(DisplayTextFor.CompanySelection),
            CommonBreadcrumbFor.Active(DisplayTextFor.OpenRegistration),
            CommonBreadcrumbFor.Active(DisplayTextFor.TOEFL),
            CommonBreadcrumbFor.Active(DisplayTextFor.ScheduleTest)
        };
    }

    private async Task GetToeflScheduleChartAsync()
    {
        var response = await _selectionTOEFLService.GetSelectionTeoflScheduleChartAsync(Id);

        if (response.Error is null)
        {
            _getSelectionTOEFLScheduleChartResponse = response.Result!.Items.ToList();
        }
    }

    private async Task SetDataOnLoadAsync()
    {
        _isLoadingData = true;
        var response = await _selectionTOEFLService.GetSelectionToeflSchedulelListAsync(Id);

        if (response.Error is null)
        {
            _getSelectionTOEFLScheduleListResponses = response.Result!.Items.ToList();
            _scheduleSummary = _getSelectionTOEFLScheduleListResponses.Count().ToString();
            _doneSummary = _getSelectionTOEFLScheduleListResponses.Where(x => x.Status == ScheduleStatus.Done).ToList().Count().ToString() + "/" + _scheduleSummary;
            _notStartedSummary = _getSelectionTOEFLScheduleListResponses.Where(x => x.Status == ScheduleStatus.NotStarted).ToList().Count().ToString() + "/" + _scheduleSummary;
        }
        else
        {
            _error = response.Error;
            _isLoadingData = false;
        }

        _isLoadingData = false;
    }

    protected void OnClickBack(MouseEventArgs e)
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

    private void OnClickAddScheduleAsync(MouseEventArgs e)
    {
        VisiblePopup = true;
    }

    #region Popup Event

    private void DialogOpen()
    {
        VisiblePopup = true;
    }

    private void DialogClose()
    {
        VisiblePopup = false;
        _createSelectionTOEFLScheduleRequest = new CreateSelectionTOEFLScheduleRequest();
    }

    private void ClickedNo()
    {
        VisiblePopup = false;
        _createSelectionTOEFLScheduleRequest = new CreateSelectionTOEFLScheduleRequest();
    }

    private void ClickedYes()
    {
        // validation schedule
        if (_createSelectionTOEFLScheduleRequest.Session1Start >= _createSelectionTOEFLScheduleRequest.Session1End)
        {
            _snackbar.AddWarning($"Session 1 : Session end must greater than Session start!!");
            StateHasChanged();
            return;
        }

        if (_createSelectionTOEFLScheduleRequest.Session1End >= _createSelectionTOEFLScheduleRequest.Session2Start)
        {
            _snackbar.AddWarning($"Session 2 : Session start must greater than Session end on Session 1!!");
            StateHasChanged();
            return;
        }

        if (_createSelectionTOEFLScheduleRequest.Session2Start >= _createSelectionTOEFLScheduleRequest.Session2End)
        {
            _snackbar.AddWarning($"Session 2 : Session end must greater than Session start!!");
            StateHasChanged();
            return;
        }

        VisiblePopupConfirm = true;
        TitlePopupConfirm = "<p style='padding:20px;'>Are you sure want to create schedule test?";
        TypeConfirm = "Created";
        StateHasChanged();
    }

    #endregion Popup Event

    #region Modal Confirm

    private void DialogConfirmOpen()
    {
        VisiblePopupConfirm = true;
    }

    private void DialogConfirmClose()
    {
        VisiblePopupConfirm = false;
    }

    private void ClickedNoConfirm()
    {
        VisiblePopupConfirm = false;
    }

    private async void ClickedYesConfirm()
    {
        VisiblePopupConfirm = false;
        _isLoading = true;
        StateHasChanged();
        if (TypeConfirm == "Created")
        {
            var itemSchedule = new CreateSelectionTOEFLScheduleRequest
            {
                Id = _createSelectionTOEFLScheduleRequest.Id,
                ScheduleStart = _createSelectionTOEFLScheduleRequest.ScheduleStart,
                ScheduleEnd = _createSelectionTOEFLScheduleRequest.ScheduleEnd,
                Session1Start = _createSelectionTOEFLScheduleRequest.Session1Start,
                Session1End = _createSelectionTOEFLScheduleRequest.Session1End,
                Session2Start = _createSelectionTOEFLScheduleRequest.Session2Start,
                Session2End = _createSelectionTOEFLScheduleRequest.Session2End,
                Session = _createSelectionTOEFLScheduleRequest.Session,
                Applicants = _createSelectionTOEFLScheduleRequest.Applicants,
                Status = _createSelectionTOEFLScheduleRequest.Status,
                ScholarshipPostingId = _createSelectionTOEFLScheduleRequest.ScholarshipPostingId,
                Location = _createSelectionTOEFLScheduleRequest.Location
            };

            _createSelectionTOEFLScheduleRequest.ScholarshipPostingId = Id;
            _createSelectionTOEFLScheduleRequest.Location = "";
            _createSelectionTOEFLScheduleRequest.Applicants = itemSchedule.Applicants;
            _createSelectionTOEFLScheduleRequest.ScheduleStart = itemSchedule.ScheduleStart;
            _createSelectionTOEFLScheduleRequest.ScheduleEnd = itemSchedule.ScheduleEnd;
            _createSelectionTOEFLScheduleRequest.Session1Start = itemSchedule.Session1Start;
            _createSelectionTOEFLScheduleRequest.Session1End = itemSchedule.Session1End;
            _createSelectionTOEFLScheduleSession2Request.ScheduleStart = itemSchedule.ScheduleStart;
            _createSelectionTOEFLScheduleSession2Request.ScheduleEnd = itemSchedule.ScheduleEnd;
            var substractday1 = _createSelectionTOEFLScheduleRequest.ScheduleEnd!.Value.Subtract(_createSelectionTOEFLScheduleRequest.ScheduleStart!.Value);
            var daySession1 = _createSelectionTOEFLScheduleRequest.ScheduleStart!.Value;

            //Create Session 1
            var j = 0;
            while (j <= substractday1.TotalDays)
            {
                _createSelectionTOEFLScheduleRequest.Session = 1;
                _createSelectionTOEFLScheduleRequest.ScheduleStart = daySession1;
                var createToeflSchedule = await _selectionTOEFLService.CreateSelectionToeflScheduleAsync(_createSelectionTOEFLScheduleRequest);
                if (createToeflSchedule.Error is not null)
                {
                    var result = string.Empty;
                    for (var i = 0; i < createToeflSchedule.Error.Details.Count; i++)
                    {
                        result = result + createToeflSchedule.Error.Details[i] + ",";
                    }

                    _snackbar.AddError(result);
                    _isLoading = false;
                    StateHasChanged();
                    return;
                }

                daySession1 = daySession1.AddDays(1);
                j++;
            }

            _createSelectionTOEFLScheduleSession2Request.ScholarshipPostingId = Id;
            _createSelectionTOEFLScheduleSession2Request.Location = "";
            _createSelectionTOEFLScheduleSession2Request.Applicants = itemSchedule.Applicants;
            _createSelectionTOEFLScheduleSession2Request.Session1Start = itemSchedule.Session2Start;
            _createSelectionTOEFLScheduleSession2Request.Session1End = itemSchedule.Session2End;
            var substractday2 = _createSelectionTOEFLScheduleSession2Request.ScheduleEnd!.Value.Subtract(_createSelectionTOEFLScheduleSession2Request.ScheduleStart!.Value);
            var daySession2 = _createSelectionTOEFLScheduleSession2Request.ScheduleStart!.Value;

            //Create session 2
            var k = 0;
            while (k <= substractday2.TotalDays)
            {
                _createSelectionTOEFLScheduleSession2Request.Session = 2;
                _createSelectionTOEFLScheduleSession2Request.ScheduleStart = daySession2;
                var createToeflSchedule = await _selectionTOEFLService.CreateSelectionToeflScheduleAsync(_createSelectionTOEFLScheduleSession2Request);
                if (createToeflSchedule.Error is not null)
                {
                    var result = string.Empty;
                    for (var i = 0; i < createToeflSchedule.Error.Details.Count; i++)
                    {
                        result = result + createToeflSchedule.Error.Details[i] + ",";
                    }

                    _snackbar.AddError(result);
                    _isLoading = false;
                    StateHasChanged();
                    return;
                }

                daySession2 = daySession2.AddDays(1);
                k++;
            }

            // send email to applicant
            var reqEmail = new SendEmailScheduleToeflRequest
            {
                PostingId = Id,
                StartDate = itemSchedule.ScheduleStart,
                EndDate = itemSchedule.ScheduleEnd,
                Session1Start = itemSchedule.Session1Start,
                Session1End = itemSchedule.Session1End,
                Session2Start = itemSchedule.Session2Start,
                Session2End = itemSchedule.Session2End,
                Quota = itemSchedule.Applicants
            };

            await _emailService.SendMailScheduleToeflAsync(reqEmail);

            VisiblePopup = false;
            _createSelectionTOEFLScheduleRequest = new CreateSelectionTOEFLScheduleRequest();
            _snackbar.AddSuccess("Success create schedule");
        }

        VisiblePopupConfirm = false;

        await SetDataOnLoadAsync();

        _isLoading = false;
        StateHasChanged();
        await Task.CompletedTask;
    }

    #endregion

    #region Popup Delete

    private void DialogDeleteOpen()
    {
        VisiblePopupDelete = true;
    }

    private void DialogDeleteClose()
    {
        VisiblePopupDelete = false;
    }

    private void ClickedNoDelete()
    {
        VisiblePopupDelete = false;
    }

    private async void ClickedYesDelete()
    {
        VisiblePopupDelete = false;
        var response = await _selectionTOEFLService.DeleteSelectionToeflScheduleAsync(_createSelectionTOEFLScheduleRequest!.Id);
        if (response.Error is null)
        {
            _snackbar.AddSuccess("Success delete data");
            await GetToeflScheduleChartAsync();
            await SetDataOnLoadAsync();
            StateHasChanged();
        }
    }

    #endregion Popup Delete

    public void OnCommandClicked(CommandClickEventArgs<GetSelectionTOEFLScheduleListResponse> args)
    {
        if (args.CommandColumn.Type == CommandButtonType.Edit)
        {
            _createSelectionTOEFLScheduleRequest.Id = args.RowData.Id;
            _navigationManager.NavigateTo(RouteFor.ScheduleDetails(Type, _createSelectionTOEFLScheduleRequest.Id));
        }
        else if (args.CommandColumn.Type == CommandButtonType.Delete)
        {
            VisiblePopupDelete = true;
            _createSelectionTOEFLScheduleRequest.Id = args.RowData.Id;
        }
    }

    private void OnChangeValueScheduleDate(RangePickerEventArgs<DateTimeOffset?> args)
    {
        _createSelectionTOEFLScheduleRequest.ScheduleStart = args.StartDate;
        _createSelectionTOEFLScheduleRequest.ScheduleEnd = args.EndDate;
        StateHasChanged();
    }
}
