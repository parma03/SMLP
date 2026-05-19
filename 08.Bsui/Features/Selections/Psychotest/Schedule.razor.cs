using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using Psmp.Base.Selections.Enums;
using Psmp.Bsui.Common.Constants;
using Psmp.Bsui.Common.Extensions;
using Psmp.Shared.Common.Responses;
using Psmp.Shared.Emails.Commands.SendEmailSchedule;
using Psmp.Shared.Selections.Constants;
using Psmp.Shared.Selections.SelectionPsychotests.Commands.Create;
using Psmp.Shared.Selections.SelectionPsychotests.Queries;
using Syncfusion.Blazor.Calendars;
using Syncfusion.Blazor.Grids;
using RouteFor = Psmp.Bsui.Features.Selections.Psychotest.Constants.RouteFor;

namespace Psmp.Bsui.Features.Selections.Psychotest;

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
    protected List<GetSelectionPsychotestScheduleListResponse> _getSelectionPsychotestScheduleListResponse = new();
    public GetSelectionPsychotestScheduleListResponse _psychotestScheduleSelected = new();
    protected List<GetSelectionPsychotestChartResponse>? _getSelectionPsychotestChartResponse = new();
    protected CreateSelectionPsychotestScheduleRequest _createSelectionPsychotestScheduleRequest = new();
    protected CreateSelectionPsychotestScheduleRequest _createSelectionPsychotestScheduleSession2Request = new();

    private bool VisiblePopup { get; set; }
    private bool VisiblePopupDelete { get; set; } = false;
    private string TitlePopup { get; set; } = "Add Schedule";
    protected string _scheduleSummary = "0";
    protected string _doneSummary = "0";
    protected string _notStartedSummary = "0";
    protected override async Task OnInitializedAsync()
    {
        _isLoading = true;
        SetupBreadcrumb();
        //await GetPsychotestScheduleChartAsync();
        await SetDataOnLoadAsync();
        _isLoading = false;
    }
    private void SetupBreadcrumb()
    {
        _breadcrumbItems = new()
        {
            CommonBreadcrumbFor.Active(DisplayTextFor.CompanySelection),
            CommonBreadcrumbFor.Active(DisplayTextFor.OpenRegistration),
            CommonBreadcrumbFor.Active(DisplayTextFor.Psychotest),
            CommonBreadcrumbFor.Active(DisplayTextFor.ScheduleTest)
        };
    }
    private async Task GetPsychotestScheduleChartAsync()
    {
        var response = await _selectionPsychotestService.GetSelectionPsychotestScheduleChartAsync(Id);

        if (response.Error is null)
        {
            _getSelectionPsychotestChartResponse = response.Result!.Items.ToList();
        }
    }
    private async Task SetDataOnLoadAsync()
    {
        _isLoadingData = true;
        var response = await _selectionPsychotestService.GetSelectionPsychotestSchedulelListAsync(Id);

        if (response.Error is null)
        {
            _getSelectionPsychotestScheduleListResponse = response.Result!.Items.ToList();
            _scheduleSummary = _getSelectionPsychotestScheduleListResponse.Count().ToString();
            _doneSummary = _getSelectionPsychotestScheduleListResponse.Where(x => x.Status == ScheduleStatus.Done).ToList().Count().ToString() + "/" + _scheduleSummary;
            _notStartedSummary = _getSelectionPsychotestScheduleListResponse.Where(x => x.Status == ScheduleStatus.NotStarted).ToList().Count().ToString() + "/" + _scheduleSummary;
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
        _psychotestScheduleSelected.Session = 2;
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
        _psychotestScheduleSelected = new GetSelectionPsychotestScheduleListResponse();
    }

    private void ClickedNo()
    {
        VisiblePopup = false;
        _psychotestScheduleSelected = new GetSelectionPsychotestScheduleListResponse();
    }

    private async void ClickedYes()
    {
        _isLoading = true;
        StateHasChanged();
        // validation schedule
        if (_psychotestScheduleSelected.Session1Start >= _psychotestScheduleSelected.Session1End)
        {
            _snackbar.AddWarning($"Session 1 : Session end must greater than Session start!!");
            _isLoading = false;
            StateHasChanged();
            return;
        }

        if (_psychotestScheduleSelected.Session1End >= _psychotestScheduleSelected.Session2Start)
        {
            _snackbar.AddWarning($"Session 2 : Session start must greater than Session end on Session 1!!");
            _isLoading = false;
            StateHasChanged();
            return;
        }

        if (_psychotestScheduleSelected.Session2Start >= _psychotestScheduleSelected.Session2End)
        {
            _snackbar.AddWarning($"Session 2 : Session end must greater than Session start!!");
            _isLoading = false;
            StateHasChanged();
            return;
        }

        var itemSchedule = new GetSelectionPsychotestScheduleListResponse
        {
            Id = _psychotestScheduleSelected.Id,
            ScholarshipPostingId = _psychotestScheduleSelected.ScholarshipPostingId,
            ScholarshipPostingCode = _psychotestScheduleSelected.ScholarshipPostingCode,
            ScheduleStartDate = _psychotestScheduleSelected.ScheduleStartDate,
            ScheduleEndDate = _psychotestScheduleSelected.ScheduleEndDate,
            Session1Start = _psychotestScheduleSelected.Session1Start,
            Session1End = _psychotestScheduleSelected.Session1End,
            Session2Start = _psychotestScheduleSelected.Session2Start,
            Session2End = _psychotestScheduleSelected.Session2End,
            Session = _psychotestScheduleSelected.Session,
            Applicants = _psychotestScheduleSelected.Applicants,
            Status = _psychotestScheduleSelected.Status,
            Location = _psychotestScheduleSelected.Location
        };

        _createSelectionPsychotestScheduleRequest.ScholarshipPostingId = Id;
        _createSelectionPsychotestScheduleRequest.Location = "";
        _createSelectionPsychotestScheduleRequest.Applicants = Convert.ToInt16(itemSchedule.Applicants);
        _createSelectionPsychotestScheduleRequest.Session1Start = itemSchedule.Session1Start;
        _createSelectionPsychotestScheduleRequest.Session1End = itemSchedule.Session1End;
        _createSelectionPsychotestScheduleRequest.ScheduleStart = itemSchedule.ScheduleStartDate;
        _createSelectionPsychotestScheduleRequest.ScheduleEnd = itemSchedule.ScheduleEndDate;
        _createSelectionPsychotestScheduleSession2Request.ScheduleStart = itemSchedule.ScheduleStartDate;
        _createSelectionPsychotestScheduleSession2Request.ScheduleEnd = itemSchedule.ScheduleEndDate;
        var substractday1 = _createSelectionPsychotestScheduleRequest.ScheduleEnd!.Value.Subtract(_createSelectionPsychotestScheduleRequest.ScheduleStart!.Value);
        var daySession1 = _createSelectionPsychotestScheduleRequest.ScheduleStart!.Value;

        // create session 1
        var j = 0;
        while (j <= substractday1.TotalDays)
        {
            _createSelectionPsychotestScheduleRequest.Session = 1;
            _createSelectionPsychotestScheduleRequest.ScheduleStart = daySession1;
            var creatPsychotestSchedule = await _selectionPsychotestService.CreateSelectionPsychotestScheduleAsync(_createSelectionPsychotestScheduleRequest);
            if (creatPsychotestSchedule.Error is not null)
            {
                var result = string.Empty;
                for (var i = 0; i < creatPsychotestSchedule.Error.Details.Count; i++)
                {
                    result = result + creatPsychotestSchedule.Error.Details[i] + ",";
                }

                _snackbar.AddError(result);
                _isLoading = false;
                StateHasChanged();
                return;
            }

            daySession1 = daySession1.AddDays(1);
            j++;
        }

        _createSelectionPsychotestScheduleSession2Request.ScholarshipPostingId = Id;
        _createSelectionPsychotestScheduleSession2Request.Location = "";
        _createSelectionPsychotestScheduleSession2Request.Applicants = Convert.ToInt16(itemSchedule.Applicants);
        _createSelectionPsychotestScheduleSession2Request.Session1Start = itemSchedule.Session2Start;
        _createSelectionPsychotestScheduleSession2Request.Session1End = itemSchedule.Session2End;
        var substractday2 = _createSelectionPsychotestScheduleSession2Request.ScheduleEnd!.Value.Subtract(_createSelectionPsychotestScheduleSession2Request.ScheduleStart!.Value);
        var daySession2 = _createSelectionPsychotestScheduleSession2Request.ScheduleStart!.Value;

        //create session 2
        var k = 0;
        while (k <= substractday2.TotalDays)
        {
            _createSelectionPsychotestScheduleSession2Request.Session = 2;
            _createSelectionPsychotestScheduleSession2Request.ScheduleStart = daySession2;
            var createSesion2 = await _selectionPsychotestService.CreateSelectionPsychotestScheduleAsync(_createSelectionPsychotestScheduleSession2Request);
            if (createSesion2.Error is not null)
            {
                var result = string.Empty;
                for (var i = 0; i < createSesion2.Error.Details.Count; i++)
                {
                    result = result + createSesion2.Error.Details[i] + ",";
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
        var reqEmail = new SendEmailSchedulePsychoTestRequest
        {
            PostingId = Id,
            StartDate = itemSchedule.ScheduleStartDate,
            EndDate = itemSchedule.ScheduleEndDate,
            Session1Start = itemSchedule.Session1Start,
            Session1End = itemSchedule.Session1End,
            Session2Start = itemSchedule.Session2Start,
            Session2End = itemSchedule.Session2End,
            Quota = itemSchedule.Applicants ?? 0
        };

        await _emailService.SendMailSchedulePsychoTestAsync(reqEmail);

        VisiblePopup = false;
        _isLoading = false;
        _psychotestScheduleSelected = new GetSelectionPsychotestScheduleListResponse();
        _snackbar.AddSuccess("Seccess create schedule");
        await SetDataOnLoadAsync();
        StateHasChanged();
    }

    #endregion Popup Event
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
        var response = await _selectionPsychotestService.DeleteSelectionPsychotestScheduleAsync(_psychotestScheduleSelected!.Id);
        if (response.Error is null)
        {
            _snackbar.AddSuccess("Success delete data");
            await GetPsychotestScheduleChartAsync();
            await SetDataOnLoadAsync();
            StateHasChanged();
        }
    }
    #endregion
    public void OnCommandClicked(CommandClickEventArgs<GetSelectionPsychotestScheduleListResponse> args)
    {
        if (args.CommandColumn.Type == CommandButtonType.Edit)
        {
            _psychotestScheduleSelected.Id = args.RowData.Id;
            _navigationManager.NavigateTo(RouteFor.ScheduleDetails(Type, _psychotestScheduleSelected.Id));
        }
        else if (args.CommandColumn.Type == CommandButtonType.Delete)
        {
            VisiblePopupDelete = true;
            _psychotestScheduleSelected.Id = args.RowData.Id;
        }
    }
    private void OnChangeValueScheduleDate(RangePickerEventArgs<DateTimeOffset?> args)
    {
        _psychotestScheduleSelected.ScheduleStartDate = args.StartDate;
        _psychotestScheduleSelected.ScheduleEndDate = args.EndDate;
        StateHasChanged();

    }
}
