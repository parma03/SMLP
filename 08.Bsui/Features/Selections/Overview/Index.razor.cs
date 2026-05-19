using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Smlp.Base.Scholarships.Enums;
using Smlp.Base.Selections.Enums;
using Smlp.Bsui.Common.Constants;
using Smlp.Bsui.Common.Extensions;
using Smlp.Shared.Common.Constants;
using Smlp.Shared.Common.Extensions;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Masters.Queries;
using Smlp.Shared.Scholarships.ScholarshipPosting.Queries;
using Smlp.Shared.Selections.SelectionOverviews.Queries;
using Syncfusion.Blazor.Data;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Navigations;
using Syncfusion.XlsIO;

namespace Smlp.Bsui.Features.Selections.Overview;

public partial class Index
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;
    [Parameter]
    public int Type { get; set; }
    protected ClaimsPrincipal _user = default!;

    protected List<GetSelectionOverviewQueryResponse> _selectionOverviews = new();
    private SfGrid<GetSelectionOverviewQueryResponse> Grid { get; set; } = new();
    protected List<DropdownItemResponse> _dropdownCategories = new();
    protected List<DropdownItemResponse> _dropdownYear = new() { new() { Id = null, Description = "Select All" } };
    protected List<DropdownItemGuidResponse> _dropdownCodes = new() { new() { Id = null, Description = "Select All" } };
    protected List<DropdownItemStringResponse> _dropdownComapies = new() { new() { Id = null, Description = "Select All" } };
    protected List<GetDirectorateQueryResponse> _dropdownDirecrorates = new();
    protected List<GetDivisionQueryResponse> _dropdownDivisions = new();
    //Applicant filter
    protected List<DropdownItemStringResponse> _dropdownComapiesApplicantFilter = new() { new() { Id = null, Description = "Select All" } };
    public List<DropdownItemResponse> _dropdownBatchs = new() { new() { Id = 0, Description = "Select All" } };

    public List<DropdownItemResponse> _dropdownCompanyGroups = new() { new() { Id = null, Description = "Select All" } };
    public List<DropdownItemResponse> _dropdownCompanyGroupsApplicantFilter = new() { new() { Id = null, Description = "Select All" } };

    protected ErrorResponse? _error;
    protected bool _isLoading = false;
    protected bool _isLoadingData = false;
    protected Guid? _scholarshipPostingId = null;
    protected string? _companyCode = null;
    protected string? _companyCodeApplicantFilter = null;
    protected int? _companyGroupId = null;
    protected int? _companyGroupIdApplicantFilter = null;
    protected int _directorateId = 0;
    protected int _divisionId = 0;
    protected string? _codeId = null;
    protected ScholarshipLocation _category = 0;
    protected int? _batch = null;
    protected int? _year = null;
    protected int _countApplicant = 0;
    protected int _countApplicantPassed = 0;
    protected int _countApplicantRejected = 0;
    protected string _applicantSummary = "0";
    protected string _passedSummary = "0";
    protected string _failedSummary = "0";
    protected string _cancelSummary = "0";
    protected string _inProgressSummary = "0";
    protected string? StatusColor { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var a = Type;
        _user = (await AuthenticationStateTask).User;
        //_companyCode = _user.GetCompanyCode();
        _isLoading = true;
        await GetDropdownCategories();
        await GetDropdownCompanyGroupApplicantFilter();
        await GetDropdownCompanyApplicantFilter();
        await SetDataOnLoadAsync();

        await GetDropdownCompanyGroup();
        await GetDropdownCompany();
        await GetDropdownCodes();
        _isLoading = false;
    }

    private async Task SetDataOnLoadAsync()
    {
        _isLoadingData = true;
        var param = new GetSelectionOverviewRequest
        {
            CompanyCode = _companyCode,
            Category = _category,
            SelectionBatch = _batch,
            CompanyGroupId = _companyGroupId,
            DirectorateId = _directorateId,
            DivisionId = _divisionId,
            Code = _codeId,
            SelectionType = (Base.Selections.Enums.SelectionType)Type,
            CompanyGroupIdApplicantFilter = _companyGroupIdApplicantFilter,
            CompanyCodeApplicantFilter = _companyCodeApplicantFilter,
            Year = _year,
            AdminCompany = _user.GetCompanyCode()
        };
        var response = await _selectionOverviewService.GetSelectionOverviewListAsync(param);

        if (response.Error is null)
        {
            _selectionOverviews = response.Result!.Items.ToList();
            _applicantSummary = _selectionOverviews.Count().ToString();
            _passedSummary = _selectionOverviews.Where(x => x.Status == ApplicantStatus.Passed).ToList().Count().ToString() + "/" + _applicantSummary;
            _failedSummary = _selectionOverviews.Where(x => x.Status == ApplicantStatus.Failed).ToList().Count().ToString() + "/" + _applicantSummary;
            _cancelSummary = _selectionOverviews.Where(x => x.Status == ApplicantStatus.Canceled).ToList().Count().ToString() + "/" + _applicantSummary;
            _inProgressSummary = _selectionOverviews.Where(x => x.Status is ApplicantStatus.InProgress or ApplicantStatus.InProgressBA or ApplicantStatus.InProgressSK).ToList().Count.ToString() + "/" + _applicantSummary;
        }
        else
        {
            _error = response.Error;
            _isLoadingData = false;
        }

        _isLoadingData = false;
    }
    #region Get Dropdown
    private async Task GetDropdownCategories()
    {
        _dropdownCategories = EnumExtentions.ToDropdownItem<ScholarshipLocation>();
        _dropdownCategories = _dropdownCategories.OrderBy(x => x.Id == 0).ToList();
        await Task.CompletedTask;
    }
    private async Task GetDropdownCodes()
    {
        var request = new GetScholarshipPostingDropdownRequest()
        {
            SelectionType = (Base.Selections.Enums.SelectionType)Type,
            CompanyGroupId = _companyGroupId,
            ComapanyCode = _companyCode,
        };
        var response = await _scholarshipPostingService.GetScholarshipDropdownAsync(request);
        if (response.Error is null)
        {
            List<DropdownItemResponse> dropdownYears = new() { new() { Id = null, Description = "Select All" } };
            List<DropdownItemGuidResponse> dropdownCodess = new() { new() { Id = null, Description = "Select All" } };
            List<DropdownItemResponse> dropdownBatchss = new() { new() { Id = 0, Description = "Select All" } };

            var data = response.Result.Items;
            var dropdownCodes = dropdownCodess.Union(data.Where(x => _scholarshipPostingId != null ? x.Id == _scholarshipPostingId : true)
                .Select(x => new DropdownItemGuidResponse { Id = x.Id, Description = x.Code, Code = x.Code }).ToList()).ToList();
            _dropdownCodes = dropdownCodes;
            var year = data.Where(x => _scholarshipPostingId != null ? x.Id == _scholarshipPostingId : true).Select(x => x.DueDate!.Value.Year).Distinct().ToList();
            var dropdownYear = dropdownYears.Union(year.Select(x => new DropdownItemResponse { Id = x, Description = x.ToString() }).ToList()).ToList();
            _dropdownYear = dropdownYear;
            var batch = data.Where(x => _scholarshipPostingId != null ? x.Id == _scholarshipPostingId : true).Select(x => x.Batch).Distinct().OrderBy(x => x).ToList();
            var dropdownBatchs = dropdownBatchss.Union(batch.Select(x => new DropdownItemResponse { Description = $"Batch {x}", Id = x }).ToList()).ToList();
            _dropdownBatchs = dropdownBatchs;
        }

    }
    private async Task GetDropdownCompanyGroup()
    {
        var response = await _companyGroupService.GetCompanyGroupsAsync();
        if (response.Error is null)
        {
            var data = _dropdownCompanyGroups.Union(response.Result!.Items.Select(x => new DropdownItemResponse { Id = x.Id, Description = x.Name }).ToList()).ToList();
            //_companyGroupId = data.Select(x => x.Id).OrderBy(x => x).FirstOrDefault();
            _dropdownCompanyGroups = data;

        }
    }
    private async Task GetDropdownCompany()
    {
        var param = new GetCompanyRequest()
        {
            CompanyGroup = _companyGroupId
        };

        var response = await _companyService.GetCompanyByCompanyGroupAsync(param);
        if (response.Error is null)
        {
            List<DropdownItemStringResponse> dropdownComapiess = new() { new() { Id = null, Description = "Select All" } };

            var data = response.Result!.Items.ToList();
            var dataComapies = data.OrderBy(x => x.Code).ToList();
            var dropdownComapies = dropdownComapiess.Union(dataComapies.Select(x => new DropdownItemStringResponse { Id = x.Code, Description = x.Name }).ToList()).ToList();
            _dropdownComapies = dropdownComapies;
        }
    }
    #region Dropdown Applicant Filter
    private async Task GetDropdownCompanyGroupApplicantFilter()
    {
        //var data = await _companyGroupService.GetCompanyGroupsAsync();
        //if (data.Error is null)
        //{
        //    _dropdownCompanyGroupsApplicantFilter = data.Result!.Items.ToList();
        //}

        var response = await _companyGroupService.GetCompanyGroupsAsync();
        if (response.Error is null)
        {
            var data = response.Result!.Items.Select(x => new DropdownItemResponse { Id = x.Id, Description = x.Name }).ToList();
            _dropdownCompanyGroupsApplicantFilter.AddRange(data);
        }
    }
    private async Task GetDropdownCompanyApplicantFilter()
    {
        var param = new GetCompanyRequest()
        {
            CompanyGroup = _companyGroupIdApplicantFilter
        };
        var response = await _companyService.GetCompanyByCompanyGroupAsync(param);
        if (response.Error is null)
        {
            List<DropdownItemStringResponse> dropdownComapiesApplicantFilters = new() { new() { Id = null, Description = "Select All" } };
            var data = response.Result!.Items.ToList();
            var dataComapies = data.OrderBy(x => x.Code).ToList();
            var dropdownComapies = dropdownComapiesApplicantFilters.Union(dataComapies.Select(x => new DropdownItemStringResponse { Id = x.Code, Description = x.Name }).ToList()).ToList();
            _dropdownComapiesApplicantFilter = dropdownComapies;
        }

        //var data = await _companyService.GetCompanyByCompanyGroupIdAsync(_companyGroupIdApplicantFilter);
        //if (data.Error is null)
        //{
        //    _dropdownComapiesApplicantFilter = data.Result!.Items.ToList();
        //    _dropdownComapiesApplicantFilter = _dropdownComapiesApplicantFilter.OrderBy(x => x.Code).ToList();
        //}
    }
    #endregion
    private async Task GetDropdownDirectorate()
    {
        var data = await _directorateService.GetDirectotaeByCompanyCodeAsync(_companyCode);
        if (data.Error is null)
        {
            _dropdownDirecrorates = data.Result!.Items.ToList();
        }
    }

    private async Task GetDropdownDivision()
    {
        var data = await _divisionService.GetDivisionByDirectorateIdAsync(_directorateId);
        if (data.Error is null)
        {
            _dropdownDivisions = data.Result!.Items.ToList();
        }
    }
    #endregion
    #region Dropdown Change
    public async Task OnChangeCategory(Syncfusion.Blazor.DropDowns.ChangeEventArgs<int, DropdownItemResponse> args)
    {
        if (args.ItemData.Id == 1)
        {
            _category = ScholarshipLocation.Abroad;
        }
        else if (args.ItemData.Id == 2)
        {
            _category = ScholarshipLocation.Domestic;
        }

        await SetDataOnLoadAsync();
        StateHasChanged();
    }
    public async Task OnChangeYear(Syncfusion.Blazor.DropDowns.ChangeEventArgs<int?, DropdownItemResponse> args)
    {
        _year = args.Value;
        await SetDataOnLoadAsync();
        StateHasChanged();
    }
    public async Task OnChangeBatchAsync(Syncfusion.Blazor.DropDowns.ChangeEventArgs<int?, DropdownItemResponse> args)
    {
        if (args.ItemData == null)
        {
            _batch = 0;
        }
        else
        {
            _batch = (int)args.Value!;
        }

        _dropdownDirecrorates = new();
        _dropdownDivisions = new();
        await SetDataOnLoadAsync();
        StateHasChanged();
    }
    public async Task OnChangeCompanyAsync(Syncfusion.Blazor.DropDowns.ChangeEventArgs<string?, DropdownItemStringResponse> args)
    {
        _companyCode = args.ItemData.Id;
        _scholarshipPostingId = null;
        _codeId = null;
        _year = null;
        _batch = null;
        await GetDropdownCodes();
        await SetDataOnLoadAsync();
        StateHasChanged();
    }
    public async Task OnChangeDirectorateAsync(Syncfusion.Blazor.DropDowns.ChangeEventArgs<int, GetDirectorateQueryResponse> args)
    {
        _directorateId = args.ItemData.Id;

        await GetDropdownDivision();
        await SetDataOnLoadAsync();
        StateHasChanged();
    }
    public async Task OnChangeDivisionAsync(Syncfusion.Blazor.DropDowns.ChangeEventArgs<int, GetDivisionQueryResponse> args)
    {
        _divisionId = args.ItemData.Id;

        await SetDataOnLoadAsync();
        StateHasChanged();
    }
    public async Task OnChangeCodeAsync(Syncfusion.Blazor.DropDowns.ChangeEventArgs<Guid?, DropdownItemGuidResponse> args)
    {
        _codeId = args.ItemData.Code;
        _scholarshipPostingId = args.ItemData.Id;
        _year = null;
        _batch = null;
        await GetDropdownCodes();
        await SetDataOnLoadAsync();
        StateHasChanged();
    }
    public async Task OnChangeCompanyGroupAsync(Syncfusion.Blazor.DropDowns.ChangeEventArgs<int?, DropdownItemResponse> args)
    {
        _companyGroupId = args.ItemData.Id;
        _companyCode = null;
        _scholarshipPostingId = null;
        _codeId = null;
        _year = null;
        _batch = null;
        await GetDropdownCodes();
        await GetDropdownCompany();
        await SetDataOnLoadAsync();
        StateHasChanged();
    }

    #endregion
    #region Applicant Filter
    public async Task OnChangeCompanyGroupApplicantFilterAsync(Syncfusion.Blazor.DropDowns.ChangeEventArgs<int?, DropdownItemResponse> args)
    {
        _companyGroupIdApplicantFilter = args.ItemData.Id;
        _companyCodeApplicantFilter = null;
        await GetDropdownCompanyApplicantFilter();
        await SetDataOnLoadAsync();
        StateHasChanged();
    }
    public async Task OnChangeCompanyApplicantFilterAsync(Syncfusion.Blazor.DropDowns.ChangeEventArgs<string?, DropdownItemStringResponse> args)
    {
        _companyCodeApplicantFilter = args.ItemData.Id;

        await SetDataOnLoadAsync();
        StateHasChanged();
    }
    #endregion

    protected DataTable _table = new();
    protected List<GetSelectionOverviewQueryResponse> _workbookApplicants = new();

    protected void GenerateListFromTable(DataTable table)
    {
        if (table is not null)
        {
            foreach (DataRow row in table.Rows)
            {
                _workbookApplicants.Add(
                    new GetSelectionOverviewQueryResponse
                    {
                        Id = new Guid((string)row["Id"]),
                        StatusId = (int)Enum.Parse<ApplicantStatus>((string)row["Status"])
                    });
            }
        }
    }

    private DataTable DataTableExport()
    {
        var table = new DataTable();

        table.Columns.Add("Id", typeof(string));
        table.Columns.Add("Name", typeof(string));
        table.Columns.Add("CompanyCode", typeof(string));
        table.Columns.Add("EmployeeId", typeof(string));
        table.Columns.Add("Email", typeof(string));
        table.Columns.Add("Bussines Issue", typeof(string));
        table.Columns.Add("Mentor 1", typeof(string));
        table.Columns.Add("Mentor 2", typeof(string));
        table.Columns.Add("Stage", typeof(string));
        table.Columns.Add("Status", typeof(string));

        if (_selectionOverviews is not null)
        {
            foreach (var item in _selectionOverviews)
            {
                table.Rows.Add(
                    item.Id,
                    item.Name,
                    item.CompanyCode,
                    item.EmployeeNumber,
                    item.Email,
                    item.BussinesIssue,
                    item.MentorName1,
                    item.MentorName2,
                    item.Stage,
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
            $"SelectionOverview_{DateTime.Now:yyyyMMdd}.xlsx",
            $"{ContentTypes.ApplicationOpenXmlExcel}",
            content);
    }

    public async Task<string> GetRowStatusColor(GetSelectionOverviewQueryResponse? param)
    {
        var statusDescription = string.Empty;
        if (param?.Status == ApplicantStatus.Passed)
        {
            statusDescription = "Passed";
            StatusColor = "border-radius:50px 50px 50px 50px !important; background-color:#00e044;";
        }
        else if (param?.Status == ApplicantStatus.Failed)
        {
            statusDescription = "Failed";
            StatusColor = "border-radius:50px 50px 50px 50px !important; background-color:#fa4a7c;";
        }
        else if (param?.Status == ApplicantStatus.Awaiting)
        {
            statusDescription = "Awaiting";
            StatusColor = "border-radius:50px 50px 50px 50px !important; background-color:#FFC300;";
        }
        else if (param?.Status == ApplicantStatus.Rejected)
        {
            statusDescription = "Rejected";
            StatusColor = "border-radius:50px 50px 50px 50px !important; background-color:#fa4a7c;";
        }
        else if (param?.Status == ApplicantStatus.Processed)
        {
            statusDescription = "Processed";
            StatusColor = "border-radius:50px 50px 50px 50px !important; background-color:#c6c5b0;";
        }

        await Task.CompletedTask;
        return statusDescription.ToString();
    }

    public static List<ScholarshipBatchResponse> GetEnumScholarshipBatch<T>()
    {
        var type = typeof(T);
        return Enum.GetValues(type)
                   .Cast<T>()
                   .Select(x => new ScholarshipBatchResponse
                   {
                       Value = x as ScholarshipBatch?,
                       Text = type.GetMember(x!.ToString()!)
                   .First()
                   .GetCustomAttribute<DisplayAttribute>()?.Name ?? x.ToString()
                   }).ToList();
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
    public void ExcelQueryCellInfoHandler(ExcelQueryCellInfoEventArgs<GetSelectionOverviewQueryResponse> args)
    {
        var a = args.Column.Field;
        if (args.Column.Field == "Status")
        {
            args.Cell.Value = args.Data.Status.GetDescription().ToString();
        }
    }
}
