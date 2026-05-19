using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using System.Security.Claims;
using Psmp.Shared.Common.Responses;
using Psmp.Shared.Masters.Queries;
using Psmp.Shared.Scholarships.ScholarshipPosting.Queries;
using Microsoft.AspNetCore.Components.Web;
using Psmp.Bsui.Features.Scholarships.SholarshipPosting.Constants;
using Syncfusion.Blazor.Grids;
using Psmp.Shared.Scholarships.ScholarshipPosting.Commands.Update;
using Psmp.Base.Scholarships.Enums;
using Psmp.Bsui.Common.Extensions;
using Syncfusion.Blazor.Navigations;
using Psmp.Shared.Common.Extensions;

namespace Psmp.Bsui.Features.Scholarships.SholarshipPosting;

public partial class Index
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;
    protected ClaimsPrincipal _user = default!;
    protected List<GetCompanyQueryResponse> _companies = new();
    protected List<GetScholarshipPostingResponse> _scholarshipPosting = new();
    protected ErrorResponse? _error;
    protected bool _isLoading = false;
    protected bool _isLoadingData = false;
    protected bool _validationDataCheck = false;
    private List<GetScholarshipPostingResponse> _tampscholarshipPosting = new();
    private SfGrid<GetScholarshipPostingResponse> Grid { get; set; } = new();
    public bool IsAllowScholarshipPostingCrud { get; set; } = false;
    protected Guid _scholarshipPostingId;
    protected string _companyCode = "0000";
    protected string _countToBeReviewed = "0";
    protected string _countWaitingForApproval = "0";
    protected string _countPublished = "0";
    protected string _countClosed = "0";
    protected string _countCompleted = "0";
    private UpdateScholarshipPostingStatusRequest ScholarshipPostingStatusRequest { get; set; } = new();
    private List<int> SelectIndex { get; set; } = new();
    protected override async Task OnInitializedAsync()
    {
        _user = (await AuthenticationStateTask).User;
        if (_user.Identity.IsAuthenticated)
        {
            if (_user.Claims.Where(x => x.Type.ToLower().Trim() == "permission" && x.Value.ToLower().Trim().Contains("scholarship.posting.crud")).ToList().Count > 0)
            {
                IsAllowScholarshipPostingCrud = true;
            }

            _isLoading = true;
            await SetDataOnLoadAsync();
            _isLoading = false;
        }
    }

    private async Task SetDataOnLoadAsync()
    {
        _isLoadingData = true;
        await Task.Delay(2000);
        _scholarshipPosting = new List<GetScholarshipPostingResponse>();
        var request = new GetScholarshipPostingRequest()
        {
            CompanyCode = _companyCode,
            Type = "all"
        };
        var response = await _scholarshipPostingService.GetScholarshipPostingAsync(request);
        if (response.Error is null)
        {
            _scholarshipPosting = response.Result?.Items.OrderBy(x => x.Status).ToList();

            _countToBeReviewed = _scholarshipPosting.Where(x => x.Status == ScholarshipPostingStatus.ToBeReviewed).ToList().Count.ToString("N0");
            _countWaitingForApproval = _scholarshipPosting.Where(x => x.Status == ScholarshipPostingStatus.WaitingForApproval).ToList().Count.ToString("N0");
            _countPublished = _scholarshipPosting.Where(x => x.Status == ScholarshipPostingStatus.Published).ToList().Count.ToString("N0");
            _countClosed = _scholarshipPosting.Where(x => x.Status == ScholarshipPostingStatus.Closed || x.DueDate < DateTime.Now).ToList().Count.ToString("N0");
            _countCompleted = _scholarshipPosting.Where(x => x.Status == ScholarshipPostingStatus.Completed).ToList().Count.ToString("N0");
        }
        else
        {
            _error = response.Error;
            _isLoadingData = false;
        }

        _isLoadingData = false;
    }

    public async Task OnChangeBatchAsync(Syncfusion.Blazor.DropDowns.ChangeEventArgs<Guid, GetScholarshipPostingResponse> args)
    {
        _scholarshipPostingId = args.ItemData.Id;

        await SetDataOnLoadAsync();
        StateHasChanged();
    }

    public async Task OnChangeCompanyAsync(Syncfusion.Blazor.DropDowns.ChangeEventArgs<string, GetCompanyQueryResponse> args)
    {
        _companyCode = args.ItemData.Code;

        await SetDataOnLoadAsync();
        StateHasChanged();
    }

    protected async Task OnClickPublish(MouseEventArgs e)
    {
        var data = await Grid.GetSelectedRowIndexesAsync();
        var ids = _tampscholarshipPosting.Select(x => x.Id).ToList();
        ScholarshipPostingStatusRequest.Id = ids;
        if (data.Count > 0)
        {
            var response = await _scholarshipPostingService.UpdateScholarshipPostingStatusAsync(ScholarshipPostingStatusRequest);
            if (response.Error != null)
            {
                _snackbar.AddError(response.Error.Detail);
            }
            else
            {
                _snackbar.AddSuccess($"Success Publish Scholarship Posting");
                await SetDataOnLoadAsync();
                StateHasChanged();
            }
        }
        else
        {
            await Grid.ClearSelectionAsync();
        }
    }

    public async Task OnClickEditScholarshipPosting(GetScholarshipPostingResponse data, bool condition)
    {
        _navigationManager.NavigateTo(RouteFor.Registration(condition, data.Id));
    }
    public async Task OnClickDeleteScholarshipPosting(GetScholarshipPostingResponse data)
    {
        var response = await _scholarshipPostingService.DeleteScholarshipPostingAsync(data.Id);
    }
    public async Task GetSelectedRecords(RowSelectEventArgs<GetScholarshipPostingResponse> args)
    {
        if (!_tampscholarshipPosting.Any(x => args.Datas.Select(x => x.Id).Contains(x.Id)))
        {
            _validationDataCheck = true;
            _tampscholarshipPosting = args.Datas;
            _tampscholarshipPosting = _tampscholarshipPosting.Where(x => x.Status == ScholarshipPostingStatus.WaitingForApproval).ToList();
            var source = await Grid.GetCurrentViewRecordsAsync();
            var indexNum = 0;
            SelectIndex = new List<int>();
            foreach (var record in source)
            {
                if (_tampscholarshipPosting.Any(x => x.Id == record.Id))
                {
                    SelectIndex.Add(indexNum);
                }

                indexNum++;
            }

            await Grid.SelectRowsAsync(SelectIndex.ToArray());
        }
    }
    public async Task CelearSelectedRecords(RowDeselectEventArgs<GetScholarshipPostingResponse> args)
    {
        if (!_validationDataCheck)
        {
            var a = _tampscholarshipPosting;
            var datas = args.Datas;
            _tampscholarshipPosting = _tampscholarshipPosting
                .Where(x => !datas.Select(x => x.Id).Contains(x.Id))
                .Where(x => x.Status == ScholarshipPostingStatus.WaitingForApproval).ToList();
        }
        else
        {
            _validationDataCheck = false;
        }
    }

    private async Task ExportExcel()
    {
        var exportProperties = new ExcelExportProperties
        {
            IncludeTemplateColumn = true
        };
        await Grid.ExportToExcelAsync(exportProperties);
    }
    public async Task ToolbarClickHandler(ClickEventArgs args)
    {
        if (args.Item.Id.Contains("_excelexport")) //Id is combination of Grid's ID and itemname
        {
            var exportProperties = new ExcelExportProperties
            {
                IncludeTemplateColumn = true
            };
            await Grid.ExportToExcelAsync(exportProperties);
        }
    }
    public void ExcelQueryCellInfoHandler(ExcelQueryCellInfoEventArgs<GetScholarshipPostingResponse> args)
    {
        var a = args.Column.Field;
        if (args.Column.Field == "Status")
        {
            args.Cell.Value = args.Data.Status.GetDescription().ToString();
        }
        else if (args.Column.Field == "AwardeeQuota")
        {
            args.Cell.Value = args.Data.CurentAplicant + " / " + args.Data.AwardeeQuota;
            ;
        }
    }
}
