/*
* Pertamina Scholarship Management Portal
* Copyright (c) 2024-2025 PT. Pertamina Persero
*/

using System.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Psmp.Base.Scholarships.Enums;
using Psmp.Bsui.Common.Constants;
using Psmp.Bsui.Common.Extensions;
using Psmp.Bsui.Features.Scholarships.ScholarshipPlan.Constants;
using Psmp.Shared.Common.Constants;
using Psmp.Shared.Common.Extensions;
using Psmp.Shared.Common.Responses;
using Psmp.Shared.Masters.Queries;
using Psmp.Shared.Scholarships.ScholarshipPlan.Queries;
using Psmp.Shared.Scholarships.ScholarshipPosting.Queries;
using Psmp.Shared.Services.Authorization.Constants;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Navigations;
using Syncfusion.XlsIO;

namespace Psmp.Bsui.Features.Scholarships.ScholarshipPlan;

public partial class Index
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

    protected ClaimsPrincipal _user = default!;
    protected List<GetCompanyQueryResponse> _companies = new();
    protected List<GetScholarshipPlanResponse> _scholarshipPlans = new();
    public GetScholarshipPlanResponse RowDetails { get; set; } = new();
    protected ErrorResponse? _error;
    protected bool _isLoading = false;
    protected bool _isLoadingData = false;
    private SfGrid<GetScholarshipPlanResponse> Grid { get; set; } = new();
    public bool IsAllowScholarshipPlanCrud { get; set; } = false;
    protected Guid _scholarshipPostingId;
    protected Guid _scholarshipPlanId;
    protected int _companyGroupId = 9999;
    protected string _countWaitingForApproval = "0";
    protected string _countApproved = "0";
    protected string _countRejected = "0";
    protected string _countScholarshipPlanByCode = "0";
    protected string _countAwardeeQuota = "0";
    protected string _countAwaitingDirSdm = "0";
    protected string _countAwaitingSvpHcm = "0";
    protected string _countAwaitingVpPcu = "0";
    protected string _countAwaitingVpHc = "0";

    private bool VisiblePopup { get; set; }

    protected override async Task OnInitializedAsync()
    {
        _user = (await AuthenticationStateTask).User;
        if (_user.Identity!.IsAuthenticated)
        {
            if (_user.Claims.Where(x => x.Type.ToLower().Trim() == "permission" && x.Value.ToLower().Trim().Contains(Permissions.Psmp_ScholarshipPlan_Crud)).ToList().Count > 0)
            {
                IsAllowScholarshipPlanCrud = true;
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
        var param = new GetScholarshipPlanRequest
        {
            CompanyCode = "",
            CompanyGroupId = _companyGroupId
        };
        var scholarshipPlan = await _scholarshipPlanService.GetScholarshipPlanAllAsync(param);

        if (scholarshipPlan.Error is null)
        {
            _scholarshipPlans = scholarshipPlan.Result!.Items.ToList();

            _countApproved = _scholarshipPlans.Where(x => x.Status == ScholarshipPlanStatus.Approved).ToList().Count.ToString("N0");
            _countWaitingForApproval = _scholarshipPlans.Where(x => x.Status == ScholarshipPlanStatus.WaitingForApproval).ToList().Count.ToString("N0");
            _countRejected = _scholarshipPlans.Where(x => x.Status == ScholarshipPlanStatus.Rejected).ToList().Count.ToString("N0");
            //if (!IsAllowScholarshipPlanCrud) //role PCU
            //{
            _countScholarshipPlanByCode = _scholarshipPlans.ToList().Count.ToString("N0");
            var totalAwardeeQuota = 0;
            foreach (var item in _scholarshipPlans)
            {
                totalAwardeeQuota += item.AwardeeQuota;
            }

            _countAwardeeQuota = totalAwardeeQuota.ToString("N0");
            //}

            StateHasChanged();
        }
        else
        {
            _error = scholarshipPlan.Error;
            _isLoadingData = false;
        }

        _isLoadingData = false;
    }

    public void OnCommandClicked(CommandClickEventArgs<GetScholarshipPlanResponse> args)
    {
        RowDetails = args.RowData;

        if (args.CommandColumn.Type == CommandButtonType.Edit)
        {
            _navigationManager.NavigateTo(RouteFor.Registration(false, RowDetails.Id), true);
        }
        else if (args.CommandColumn.Type == CommandButtonType.Delete)
        {
            _scholarshipPlanId = RowDetails.Id;
            VisiblePopup = true;
        }
        else if (args.CommandColumn.Type == CommandButtonType.None)
        {
            _navigationManager.NavigateTo(RouteFor.Registration(false, RowDetails.Id), true);
        }
    }

    public async Task OnChangeBatchAsync(Syncfusion.Blazor.DropDowns.ChangeEventArgs<Guid, GetScholarshipPostingResponse> args)
    {
        _scholarshipPostingId = args.ItemData.Id;

        await SetDataOnLoadAsync();
        StateHasChanged();
    }

    public async Task OnChangeCompanyGroupAsync(Syncfusion.Blazor.DropDowns.ChangeEventArgs<int, GetCompanyGroupQueryResponse> args)
    {
        _companyGroupId = args.ItemData.Id;

        await SetDataOnLoadAsync();
        StateHasChanged();
    }

    private DataTable DataTableExport()
    {
        var table = new DataTable();

        table.Columns.Add("Scholarship Code", typeof(string));
        table.Columns.Add("H/SH/APS&P", typeof(string));
        table.Columns.Add("CompanyName", typeof(string));
        table.Columns.Add("Directorate/Function", typeof(string));
        table.Columns.Add("Division", typeof(string));
        table.Columns.Add("Business Issues", typeof(string));
        table.Columns.Add("References of Business Issues", typeof(string));
        table.Columns.Add("Performance Issues", typeof(string));
        table.Columns.Add("Competency Issues", typeof(string));
        table.Columns.Add("Degree", typeof(string));
        table.Columns.Add("Faculty", typeof(string));
        table.Columns.Add("Major", typeof(string));
        table.Columns.Add("University Reference", typeof(string));
        table.Columns.Add("University Location", typeof(string));
        table.Columns.Add("Latest Education Background", typeof(string));
        table.Columns.Add("Minimum Working Experience", typeof(string));
        table.Columns.Add("Scope", typeof(string));
        table.Columns.Add("Awardee Quota", typeof(string));
        table.Columns.Add("Batch", typeof(string));
        table.Columns.Add("Selection category", typeof(string));
        table.Columns.Add("Posting Year", typeof(string));
        table.Columns.Add("Minimum PRL Individu/BS", typeof(string));
        table.Columns.Add("Maximum Age", typeof(string));
        table.Columns.Add("Minimum Work Period", typeof(string));
        table.Columns.Add("Average Performance Rating Last 3 Years", typeof(string));
        table.Columns.Add("Indisciplinary Record Last 3 Years", typeof(string));
        table.Columns.Add("MCU Status", typeof(string));
        table.Columns.Add("Status", typeof(string));

        if (_scholarshipPlans is not null)
        {
            foreach (var item in _scholarshipPlans)
            {
                table.Rows.Add(
                    item.Code,
                    item.CompanyGroupName,
                    item.CompanyName,
                    item.DirectorateName,
                    item.DivisionName,
                    item.BusinessIssue,
                    item.BusinessIssueReferences,
                    item.PerformanceIssue,
                    item.CompetencyIssue,
                    item.DegreeName,
                    item.Faculty,
                    item.Major,
                    item.UniversityReference,
                    item.UniversityLocation.GetDescription(),
                    item.LatestEducation,
                    item.WorkingExperience,
                    item.ScholarshipScope.GetDescription(),
                    item.AwardeeQuota,
                    item.Batch,
                    item.SelectionType.GetDescription(),
                    item.PostingYear,
                    item.Prl,
                    item.Age,
                    item.WorkPeriod,
                    item.PerformanceRating,
                    item.Indiciplinary.GetDescription(),
                    item.Mcu.GetDescription(),
                    item.Status);
            }
        }

        return table;
    }

    private async Task OnClickDownloadAsync(MouseEventArgs e)
    {
        using var excelEngine = new ExcelEngine();
        var application = excelEngine.Excel;
        application.DefaultVersion = ExcelVersion.Xlsx;

        //Create a workbook
        var workbook = application.Workbooks.Create(1);
        var worksheet = workbook.Worksheets[0];
        var table = DataTableExport();

        //import data from datatable
        worksheet.ImportDataTable(table, true, 1, 1, true);

        //save document as a stream and retrun the stream.
        var downloadStream = new MemoryStream();
        workbook.SaveAs(downloadStream, ExcelSaveType.SaveAsXLS);
        workbook.Close();

        var content = downloadStream.ToArray();

        //download document
        await _jsRuntime.InvokeVoidAsync
            (JavaScriptIdentifierFor.DownloadFile,
            $"ScholarshipPlan_{DateTime.Now:yyyyMMdd}.xlsx",
            $"{ContentTypes.ApplicationOpenXmlExcel}",
            content);
    }

    protected void OnClickRegistration(MouseEventArgs e)
    {
        _navigationManager.NavigateTo(RouteFor.Created(true), true);
    }

    #region Popup Event

    private void DialogOpen()
    {
        VisiblePopup = true;
    }

    private void DialogClose()
    {
        VisiblePopup = false;
    }

    private void ClickedNo()
    {
        VisiblePopup = false;
    }

    private async void ClickedYes()
    {
        var response = await _scholarshipPlanService.DeleteScholarshipPlanAsync(_scholarshipPlanId);
        if (response.Error is not null)
        {
            _snackbar.AddError(response.Error.Detail);
        }
        else
        {
            _snackbar.AddSuccess($"Success delete Scholarship Plan, Code : {response.Result!.Code} with ID {response.Result!.Id}");
            VisiblePopup = false;
            await SetDataOnLoadAsync();
            StateHasChanged();
            await Grid.Refresh();
        }
    }

    #endregion Popup Event

    #region Create Draft
    protected async Task OnCreateDraft()
    {
        _navigationManager.NavigateTo(RouteFor.Draft(), true);
        await Task.CompletedTask;
    }
    #endregion

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
    public void ExcelQueryCellInfoHandler(ExcelQueryCellInfoEventArgs<GetScholarshipPlanResponse> args)
    {
        var a = args.Column.Field;
        if (args.Column.Field == "Status")
        {
            args.Cell.Value = args.Data.Status.GetDescription();
        }
    }
}
