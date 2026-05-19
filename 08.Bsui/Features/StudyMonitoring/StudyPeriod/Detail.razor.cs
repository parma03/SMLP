using Microsoft.AspNetCore.Components;
using MudBlazor;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.StudyMonitorings.StudyPeriod.Queries;
using Syncfusion.Blazor.Grids;
using Smlp.Bsui.Common.Constants;
using Smlp.Shared.StudyMonitorings.Constants;
using Smlp.Bsui.Common.Extensions;

namespace Smlp.Bsui.Features.StudyMonitoring.StudyPeriod;

public partial class Detail
{
    #region Parameter
    [Parameter]
    public Guid Id { get; set; }
    #endregion

    #region Protected
    protected ErrorResponse? _error;
    protected bool _isLoading = false;
    protected bool _isLoadingDialog = false;
    private readonly List<BreadcrumbItem> _breadcrumbItems = new();
    protected List<GetStudyPeriodByAdmissionIdQueryResponse> _dataGrid = new();
    protected GetStudyPeriodByAdmissionIdQueryResponse _data = new();
    #endregion

    #region Card
    private float _highest = 0;
    private float _lowest = 0;
    private float _average = 0;
    #endregion

    protected override async Task OnInitializedAsync()
    {
        _isLoading = true;

        await OnLoadPage();
        await SetDataOnLoadAsync();

        _isLoading = false;
        await Task.CompletedTask;
    }

    private async Task OnLoadPage()
    {
        _breadcrumbItems.Clear();
        _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.StudyMonitoring));
        _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.StudyPeriod));
        _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.Detail));

        await Task.CompletedTask;
    }

    #region DataGrid
    private async Task SetDataOnLoadAsync()
    {
        var data = await _studyMonitoringStudyPeriodService.GetStudyPeriodByAdmissionIdAsync(Id);
        if (data.Result == null || data.Result!.Items.Count == 0)
        {
            _snackbar.AddError("Data not found!!");
            _navigationManager.NavigateTo(Smlp.Bsui.Features.StudyMonitoring.Constants.RouteFor.StudyMonitoringTab(0), true);
            StateHasChanged();
            return;
        }

        if (data.Error is null)
        {
            _dataGrid = data.Result!.Items.ToList();
            _data = _dataGrid.FirstOrDefault() ?? new();

            _highest = 0;
            _lowest = 0;
            _average = 0;
            if (_dataGrid.Count() > 0)
            {
                _highest = _dataGrid.Max(x => x.GpaScore);
                _lowest = _dataGrid.Min(x => x.GpaScore);
                _average = _dataGrid.Average(x => x.GpaScore);
            }
        }
        else
        {
            _error = data.Error;
        }

        await Task.CompletedTask;
    }
    #endregion

    #region Command Event
    public async Task OnCommandClicked(CommandClickEventArgs<GetStudyPeriodByAdmissionIdQueryResponse> args)
    {
        _navigationManager.NavigateTo(Smlp.Bsui.Features.StudyMonitoring.Constants.RouteFor.StudyMonitoringDetailItem("StudyPeriod", Id, args.RowData.Id), true);
        await Task.CompletedTask;
    }
    #endregion

    #region OnClick
    public async Task OnClickBack()
    {
        _navigationManager.NavigateTo(Smlp.Bsui.Features.StudyMonitoring.Constants.RouteFor.StudyMonitoringTab(0), true);
        await Task.CompletedTask;
    }
    #endregion
}
