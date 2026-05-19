using Microsoft.AspNetCore.Components;
using MudBlazor;
using Smlp.Bsui.Common.Constants;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Masters.Queries;
using Smlp.Shared.Selections.SelectionPsychotests.Queries;
using Syncfusion.Blazor.Grids;
using Smlp.Shared.Selections.Constants;
using Smlp.Bsui.Common.Extensions;
using Microsoft.AspNetCore.Components.Web;
using RouteFor = Smlp.Bsui.Features.Selections.Psychotest.Constants.RouteFor;
using Smlp.Shared.Selections.SelectionPsychotests.Commands.Update;

namespace Smlp.Bsui.Features.Selections.Psychotest;

public partial class ScheduleDetail
{
    [Parameter]
    public Guid Id { get; set; }
    [Parameter]
    public int Type { get; set; }
    protected ErrorResponse? _error;
    protected bool _isLoading = false;
    protected bool _isLoadingData = false;
    private List<BreadcrumbItem> _breadcrumbItems = new();
    protected GetSelectionPsychotestScheduleListResponse _response = new();
    protected List<GetSelectionPsychotestApplicantsQueryResponse> _listApplicants = new();
    protected List<GetSelectionPsychotestScheduleListResponse> _getSelectionPsychotestScheduleListResponse = new();
    private string _companyCode = "0000";
    private bool VisiblePopupDelete { get; set; } = false;
    private Guid PsychotestScheduleApplicantId { get; set; }

    protected override async Task OnInitializedAsync()
    {
        _isLoading = true;
        SetupBreadcrumb();
        await GetScheduledDetail();
        await GetApplicants();
        _isLoading = false;
    }
    private void SetupBreadcrumb()
    {
        _breadcrumbItems = new()
        {
            CommonBreadcrumbFor.Active(DisplayTextFor.CompanySelection),
            CommonBreadcrumbFor.Active(DisplayTextFor.OpenRegistration),
            CommonBreadcrumbFor.Active(DisplayTextFor.Psychotest),
            CommonBreadcrumbFor.Active(DisplayTextFor.ScheduleTest),
            CommonBreadcrumbFor.Active(DisplayTextFor.ScheduleTestDetails)
        };
    }
    private async Task GetScheduledDetail()
    {
        var response = await _selectionPsychotestService.GetSelectionPsychotestScheduleDetailAsync(Id);

        if (response.Error is null)
        {
            _response = response.Result!;

            var listSchedule = await _selectionPsychotestService.GetSelectionPsychotestSchedulelListAsync(_response.ScholarshipPostingId);
            if (listSchedule.Error is null)
            {
                _getSelectionPsychotestScheduleListResponse = listSchedule.Result!.Items.ToList();
            }
        }
        else
        {
            _error = response.Error;
        }

    }
    private async Task GetApplicants()
    {
        var request = new GetSelectionPsychotestScheduleRequest
        {
            SelectionPsychotestScheduleId = Id,
            CompanyCode = _companyCode,
        };

        var response = await _selectionPsychotestService.GetSelectionPsychotestApplicantsAsync(request);

        if (response.Error is null)
        {
            _listApplicants = response.Result!.Items.ToList();
        }
        else
        {
            _error = response.Error;
        }

    }
    public async Task OnChangeCompanyAsync(Syncfusion.Blazor.DropDowns.ChangeEventArgs<string, GetCompanyQueryResponse> args)
    {
        _companyCode = args.ItemData.Code;

        await GetApplicants();
        StateHasChanged();
    }
    public void OnCommandClicked(CommandClickEventArgs<GetSelectionPsychotestApplicantsQueryResponse> args)
    {
        if (args.CommandColumn.Type == CommandButtonType.Delete)
        {
            PsychotestScheduleApplicantId = args.RowData.Id;
            VisiblePopupDelete = true;
        }
    }
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
        var response = await _selectionPsychotestService.DeleteSelectionPsychotesApplicantAsync(PsychotestScheduleApplicantId);
        if (response.Error is null)
        {
            _snackbar.AddSuccess("Success delete data");
            await GetApplicants();
            StateHasChanged();
        }
    }
    #endregion
    protected void OnClickCancel(MouseEventArgs e)
    {
        _navigationManager.NavigateTo(RouteFor.Schedule(Type, _response.ScholarshipPostingId), true);
    }
    private async void OnClickSave()
    {
        var scheduleOnDate = _getSelectionPsychotestScheduleListResponse.Where(x => x.ScheduleStartDate == _response.ScheduleStartDate).ToList();
        if (scheduleOnDate.Count() > 0)
        {
            if (_response.Session1Start >= _response.Session1End)
            {
                _snackbar.AddWarning($"Session end must greater than Session start!!");
                StateHasChanged();
                return;
            }

            if (_response.Session == 1)
            {
                var scheduleSession2 = scheduleOnDate.Where(x => x.Session == 2).FirstOrDefault();
                if (scheduleSession2 != null)
                {
                    if (scheduleSession2.Session1Start <= _response.Session1End)
                    {
                        _snackbar.AddWarning($"Please change Session 2 for greater than Session end!");
                        StateHasChanged();
                        return;
                    }
                }
            }

            if (_response.Session == 2)
            {
                var scheduleSession1 = scheduleOnDate.Where(x => x.Session == 1).FirstOrDefault();
                if (scheduleSession1 != null)
                {
                    if (scheduleSession1.Session1End >= _response.Session1Start)
                    {
                        _snackbar.AddWarning($"Session start must greater than Session end on Session 1!!");
                        StateHasChanged();
                        return;
                    }
                }
            }
        }

        var request = new UpdateSelectionPsychotestScheduleRequest
        {
            Id = Id,
            ScheduleStart = _response.ScheduleStartDate,
            ScheduleEnd = _response.ScheduleEndDate,
            Applicants = Convert.ToInt16(_response.Applicants),
            Session = _response.Session,
            Session1Start = _response.Session1Start,
            Session1End = _response.Session1End,
            Session2Start = _response.Session2Start,
            Session2End = _response.Session2End,
            Location = _response.Location,
        };
        var response = await _selectionPsychotestService.UpdateSelectionPsychotestScheduleAsync(request);
        if (response.Error is null)
        {
            _snackbar.AddSuccess("Success update data");
            await GetScheduledDetail();
            StateHasChanged();
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
    }
}
