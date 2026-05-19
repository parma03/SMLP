using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Psmp.Base.Scholarships.Enums;
using Psmp.Base.Selections.Enums;
using Psmp.Bsui.Common.Constants;
using Psmp.Bsui.Common.Extensions;
using Psmp.Shared.Applicants.Commands.Create;
using Psmp.Shared.Common.Constants;
using Psmp.Shared.Common.Extensions;
using Psmp.Shared.Common.Responses;
using Psmp.Shared.Documents.Queries.GetDocuments;
using Psmp.Shared.Masters.Queries;
using Psmp.Shared.Scholarships.ScholarshipPosting.Queries;
using Psmp.Shared.Scholarships.ScholarshipPostingApplicant.Commands.Create;
using Psmp.Shared.Selections.SelectionAdministrations.Commands.CreateSelectionAdministration;
using Psmp.Shared.Selections.SelectionAdministrations.Commands.UpdateSelectionAdministration;
using Psmp.Shared.Selections.SelectionAdministrations.Queries;
using Psmp.Shared.Selections.SelectionOverviews.Commands.Update;
using Psmp.Shared.Selections.SelectionTOEFLs.Commands.Update;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Inputs;
using Syncfusion.XlsIO;
using Syncfusion.Blazor.Navigations;
using SelectionCreate = Psmp.Bsui.Features.Selections.Actions.SelectionCreate;
using SelectionType = Psmp.Base.Selections.Enums.SelectionType;
using Psmp.Shared.Selections.SelectionTOEFLs.Queries;

namespace Psmp.Bsui.Features.Selections.Administration;

public partial class Index
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

    protected ClaimsPrincipal _user = default!;

    [Parameter]
    public int Type { get; set; }

    protected List<GetCompanyQueryResponse> _dropdownCompaniesAddApplicant = new();
    protected List<GetSelectionAdministrationQueryResponse> _selectionAdministrations = new();
    private SfGrid<GetSelectionAdministrationQueryResponse> Grid { get; set; } = new();
    protected CreateAdministrationApplicantRequest _createAdministrationApplicantRequest = new();
    protected UpdateAdministrationApplicantStatusRequest _updateAdministrationApplicantStatusRequest = new();
    public IEnumerable<ApplicantStatusResponse> _applicantStatusResponse = GetEnumDisplayNames<ApplicantStatus>();
    public IEnumerable<MCUStatusResponse> _mcuStatusResponse = GetEnumDisplayNamesMCUStatus<McuStatus>();
    public IEnumerable<EmploymentStatusResponse> _employmentStatusResponse = GetEnumDisplayNamesEmploymentStatus<EmploymentStatus>();
    public IEnumerable<IndiciplinaryStatusResponse> _indiciplinaryStatusResponse = GetEnumDisplayIndiciplinaryStatus<IndiciplinaryStatus>();
    protected List<GetDocumentsDocument> _getDocumentsDocument = new();
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
    protected Guid _applicantId;
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
    protected string _canceledSummary = "0";
    protected long _maximumFileSizeInBytes = long.MaxValue;
    private bool VisiblePopup { get; set; }
    private string TitlePopup { get; set; } = "Add/ Edit Applicant";
    private bool VisiblePopupDelete { get; set; } = false;
    private string? ActionType { get; set; }
    private bool IsEnabled { get; set; }
    public SfTextBox? _textBoxNopek;
    private bool _isLoadingDialog = false;
    protected SfUploader? _uploadObj;

    protected bool IsShowError { get; set; }
    protected string ErrorMessage { get; set; } = string.Empty!;

    protected override async Task OnInitializedAsync()
    {
        _user = (await AuthenticationStateTask).User;
        //_companyCode = _user.GetCompanyCode();
        _isLoading = true;
        await GetDropdownCategories();
        //await GetDropdownBatchs();
        await GetDropdownCompanyGroup();
        await GetDropdownCompany();
        await GetCompany();
        await GetDropdownCompanyGroupApplicantFilter();
        await GetDropdownCompanyApplicantFilter();
        await GetDropdownCodes();
        await SetDataOnLoadAsync();
        _isLoading = false;
    }

    private async Task GetCompany()
    {
        var companies = await _companyService.GetCompaniesAsync();
        if (companies.Error is null)
        {
            _dropdownCompaniesAddApplicant = companies.Result!.Items.ToList();
            _dropdownCompaniesAddApplicant = _dropdownCompaniesAddApplicant.OrderByDescending(x => x.Code == "0000").ToList();
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

    #endregion Dropdown Applicant Filter

    private async Task SetDataOnLoadAsync()
    {
        _isLoadingData = true;
        var patam = new GetSelectionAdministrationRequest()
        {
            CompanyCode = _companyCode,
            Category = _category,
            SelectionBatch = _batch,
            CompanyGroupId = _companyGroupId,
            DirectorateId = _directorateId,
            DivisionId = _divisionId,
            Code = _codeId,
            SelectionType = (SelectionType)Type,
            CompanyGroupIdApplicantFilter = _companyGroupIdApplicantFilter,
            CompanyCodeApplicantFilter = _companyCodeApplicantFilter,
            Year = _year,
            AdminCompany = _user.GetCompanyCode()
        };

        var administrations = await _selectionAdministrationService.GetSelectionAdministrationAsync(patam);
        if (administrations.Error is null)
        {
            _selectionAdministrations = administrations.Result!.Items.ToList();
            //summary
            _applicantSummary = _selectionAdministrations.Count.ToString();
            _passedSummary = $"{_selectionAdministrations.Where(x => x.Status == ApplicantStatus.Passed).ToList().Count}/{_applicantSummary}";
            _failedSummary = $"{_selectionAdministrations.Where(x => x.Status == ApplicantStatus.Failed).ToList().Count}/{_applicantSummary}";
            _canceledSummary = $"{_selectionAdministrations.Where(x => x.Status == ApplicantStatus.Canceled).ToList().Count}/{_applicantSummary}";
        }
        else
        {
            _error = administrations.Error;
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
            SelectionType = (SelectionType)Type,
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

    #endregion Get Dropdown

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
            _batch = (int)args.Value;
        }

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
        //await GetDropdownDirectorate();
        await SetDataOnLoadAsync();
        StateHasChanged();
    }

    public async Task OnChangeDirectorateAsync(Syncfusion.Blazor.DropDowns.ChangeEventArgs<int, GetDirectorateQueryResponse> args)
    {
        _directorateId = args.ItemData.Id;

        //await GetDropdownDivision();
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

    #endregion Dropdown Change

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

    #endregion Applicant Filter

    private async Task ValueChangeUploaderApplicants(UploadChangeEventArgs args)
    {
        foreach (var file in args.Files)
        {
            var excelEngine = new ExcelEngine();
            var application = excelEngine.Excel;
            application.DefaultVersion = ExcelVersion.Excel2016;

            var memoryStream = new MemoryStream();
            await args.Files[0].File.OpenReadStream(long.MaxValue).CopyToAsync(memoryStream);
            memoryStream.Position = 0; // Reset the stream position

            var workbook = application.Workbooks.Open(memoryStream);

            //get the first worksheet in the workbook into IWorksheet
            var worksheet = workbook.Worksheets[0];

            _table = worksheet.ExportDataTable(worksheet.UsedRange, ExcelExportDataTableOptions.ColumnNames);

            //convert to list
            try
            {
                GenerateListFromTable(_table);
            }
            catch (Exception ex)
            {
                _snackbar.AddError(ex.Message);
            }
        }
    }

    protected DataTable _table = new();
    protected List<GetSelectionAdministrationQueryResponse> _workbookApplicants = new();

    protected void GenerateListFromTable(DataTable table)
    {
        if (table is not null)
        {
            foreach (DataRow row in table.Rows)
            {
                var status = GetStatusForUpload((string)row["Status"]);
                var stage = string.Empty;
                if ((string)row["Stage"] is "Talent Review Meeting" or "Create SK")
                {
                    var getStage = GetStageForUpload((string)row["Stage"]);
                    stage = getStage;
                }
                else
                {
                    stage = (string)row["Stage"];
                }

                _workbookApplicants.Add(
                    new GetSelectionAdministrationQueryResponse
                    {
                        Id = new Guid((string)row["Id"]),
                        PostingId = new Guid((string)row["ScholarshipId"]),
                        ApplicantId = new Guid((string)row["ApplicantId"]),
                        Name = (string)row["Name"],
                        CompanyCode = (string)row["CompanyCode"],
                        EmployeeNumber = (string)row["EmployeeNumber"],
                        Position = (string)row["Position"],
                        Email = (string)row["Email"],
                        Stage = (SelectionStage)(int)Enum.Parse<SelectionStage>(stage),
                        Status = (ApplicantStatus)(int)Enum.Parse<ApplicantStatus>(status)
                    });
            }
        }
    }

    protected void OnRemoveUploaderApplicants(RemovingEventArgs args)
    {
        _workbookApplicants = new();
        args.PostRawFile = false;
    }

    private async Task OnClickUploadAsync(MouseEventArgs e)
    {
        if (_workbookApplicants.Count() == 0)
        {
            _snackbar.AddWarning("Please browse file");
            return;
        }

        _isLoadingDialog = true;
        if (_workbookApplicants.Count() > 0)
        {
            foreach (var item in _workbookApplicants.Where(x => x.Stage is SelectionStage.Administration or SelectionStage.TOEFL))
            {
                #region Update selection administration

                _updateAdministrationApplicantStatusRequest.Id = item.Id;
                _updateAdministrationApplicantStatusRequest.Status = item.Status;

                var updateSelectionAdministration = await _selectionAdministrationService.UpdateAdministrationApplicantStatusAsync(_updateAdministrationApplicantStatusRequest);
                if (updateSelectionAdministration.Error is not null)
                {
                    var result = string.Empty;
                    for (var i = 0; i < updateSelectionAdministration.Error.Details.Count; i++)
                    {
                        result = result + updateSelectionAdministration.Error.Details[i] + ",";
                    }

                    _snackbar.AddError("Administration - Error : " + result);
                    _isLoadingDialog = false;
                    return;
                }

                #endregion Update selection administration

                var getTOEFL = await _selectionTOEFLService.GetSelectionToeflByApplicantIdAsync(item.ApplicantId);
                if (getTOEFL.Error is null)
                {
                    if (getTOEFL is not null)
                    {
                        if (getTOEFL.Result!.Status == ApplicantStatus.InProgress)
                        {
                            #region Update selection overview

                            var updateOverviewRequest = new UpdateSelectionOverviewByApplicantIdRequest
                            {
                                ApplicantId = item.ApplicantId,
                                Stage = item.Status == ApplicantStatus.Passed ? SelectionStage.TOEFL : SelectionStage.Administration,
                                Status = item.Status,
                            };

                            var updateOverview = await _selectionOverviewService.UpdateSelectionOverviewByApplicantIdAsync(updateOverviewRequest);
                            if (updateOverview.Error is not null)
                            {
                                var result = string.Empty;
                                for (var i = 0; i < updateOverview.Error.Details.Count; i++)
                                {
                                    result = result + updateOverview.Error.Details[i] + ",";
                                }

                                _snackbar.AddError("Overview - Error : " + result);
                                _isLoadingDialog = false;
                                return;
                            }

                            #endregion Update selection overview

                            #region Update selection TOEFL

                            if (item.Status == ApplicantStatus.Passed)
                            {
                                var updateTOEFLRequest = new UpdateSelectionTOEFLRequest
                                {
                                    Id = getTOEFL.Result!.Id,
                                    Status = ApplicantStatus.InProgress,
                                    ScholarshipPostingApplicantId = item.ApplicantId,
                                };

                                var updateTOEFL = await _selectionTOEFLService.UpdateSelectionToeflAsync(updateTOEFLRequest);
                                if (updateTOEFL.Error is not null)
                                {
                                    var result = string.Empty;
                                    for (var i = 0; i < updateTOEFL.Error.Details.Count; i++)
                                    {
                                        result = result + updateTOEFL.Error.Details[i] + ",";
                                    }

                                    _snackbar.AddError("TOEFL - Error : " + result);
                                    _isLoadingDialog = false;
                                    return;
                                }
                            }

                            #endregion Update selection TOEFL
                        }
                    }
                }
            }

            _snackbar.AddSuccess("Success upload data");
            _isLoadingDialog = false;
            await _uploadObj!.ClearAllAsync();
            _workbookApplicants = new();
            await SetDataOnLoadAsync();
            StateHasChanged();
        }
    }

    private async Task OnClickDownloadAsync(MouseEventArgs e)
    {
        using var excelEngine = new ExcelEngine();
        var application = excelEngine.Excel;
        application.DefaultVersion = ExcelVersion.Xlsx;

        //Create a workbook
        var workbook = application.Workbooks.Create(1);
        var worksheet = workbook.Worksheets[0];
        worksheet.Range["A1"].ColumnWidth = 40;
        worksheet.Range["B1"].ColumnWidth = 40;
        worksheet.Range["C1"].ColumnWidth = 40;
        worksheet.Range["D1"].ColumnWidth = 30;
        worksheet.Range["F1"].ColumnWidth = 40;
        worksheet.Range["G1"].ColumnWidth = 40;
        worksheet.Range["H1"].ColumnWidth = 40;
        worksheet.Range["I1"].ColumnWidth = 40;
        worksheet.Range["J1"].ColumnWidth = 30;
        worksheet.Range["K1"].ColumnWidth = 30;
        worksheet.Range["L1"].ColumnWidth = 30;
        worksheet.Range["M1"].ColumnWidth = 30;

        var headingStyle = workbook.Styles.Add("HeadingStyle");
        headingStyle.Font.Bold = true;
        worksheet.Range["O1:O" + (_selectionAdministrations.Count() + 1)].CellStyle = headingStyle;

        worksheet.Protect("sch0larship");
        worksheet.Range["O2:O" + (_selectionAdministrations.Count() + 1)].CellStyle.Locked = false;
        //Add Comments (Notes)
        IComment commentA = worksheet.Range["A1"].AddComment();
        commentA.Text = "Do not modify";
        commentA.IsVisible = false;

        IComment commentB = worksheet.Range["B1"].AddComment();
        commentB.Text = "Do not modify";
        commentB.IsVisible = false;

        IComment commentC = worksheet.Range["C1"].AddComment();
        commentC.Text = "Do not modify";
        commentC.IsVisible = false;

        IComment commentD = worksheet.Range["D1"].AddComment();
        commentD.Text = "Do not modify";
        commentD.IsVisible = false;

        IComment commentE = worksheet.Range["E1"].AddComment();
        commentE.Text = "Do not modify";
        commentE.IsVisible = false;

        IComment commentF = worksheet.Range["F1"].AddComment();
        commentF.Text = "Do not modify";
        commentF.IsVisible = false;

        IComment commentG = worksheet.Range["G1"].AddComment();
        commentG.Text = "Do not modify";
        commentG.IsVisible = false;

        IComment commentH = worksheet.Range["H1"].AddComment();
        commentH.Text = "Do not modify";
        commentH.IsVisible = false;

        IComment commentI = worksheet.Range["I1"].AddComment();
        commentI.Text = "Do not modify";
        commentI.IsVisible = false;

        IComment commentJ = worksheet.Range["J1"].AddComment();
        commentJ.Text = "Do not modify";
        commentJ.IsVisible = false;

        IComment commentK = worksheet.Range["K1"].AddComment();
        commentK.Text = "Do not modify";
        commentK.IsVisible = false;

        IComment commentL = worksheet.Range["L1"].AddComment();
        commentL.Text = "Do not modify";
        commentL.IsVisible = false;

        IComment commentM = worksheet.Range["M1"].AddComment();
        commentM.Text = "Do not modify";
        commentM.IsVisible = false;

        IComment commentN = worksheet.Range["N1"].AddComment();
        commentN.Text = "Do not modify";
        commentN.IsVisible = false;

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
            $"SelectionAdministration_{DateTime.Now:yyyyMMdd}.xlsx",
            $"{ContentTypes.ApplicationOpenXmlExcel}",
            content);
    }

    private DataTable DataTableExport()
    {
        var table = new DataTable();

        table.Columns.Add("Id", typeof(string));
        table.Columns.Add("ScholarshipId", typeof(string));
        table.Columns.Add("ApplicantId", typeof(string));
        table.Columns.Add("Name", typeof(string));
        table.Columns.Add("CompanyCode", typeof(string));
        table.Columns.Add("Faculty", typeof(string));
        table.Columns.Add("Major", typeof(string));
        table.Columns.Add("EmployeeNumber", typeof(string));
        table.Columns.Add("Position", typeof(string));
        table.Columns.Add("Email", typeof(string));
        table.Columns.Add("Bussines Issue", typeof(string));
        table.Columns.Add("Mentor 1", typeof(string));
        table.Columns.Add("Mentor 2", typeof(string));
        table.Columns.Add("Stage", typeof(string));
        table.Columns.Add("Status", typeof(string));

        if (_selectionAdministrations is not null)
        {
            foreach (var item in _selectionAdministrations)
            {
                table.Rows.Add(
                    item.Id,
                    item.PostingId,
                    item.ApplicantId,
                    item.Name,
                    item.CompanyCode,
                    item.Faculty,
                    item.Major,
                    item.EmployeeNumber,
                    item.Position,
                    item.Email,
                    item.BussinesIssue,
                    item.MentorName1,
                    item.MentorName2,
                    item.Stage.GetDescription(),
                    item.Status.GetDescription());
            }
        }

        return table;
    }

    public async Task OnCommandClicked(CommandClickEventArgs<GetSelectionAdministrationQueryResponse> args)
    {
        if (args.CommandColumn.Type == CommandButtonType.Edit)
        {
            var skData = await _selectionDecreesService.GetSelectionDecreeByApplicantIdAsync(args.RowData.ApplicantId);
            var toeflData = await _selectionTOEFLService.GetSelectionToeflByApplicantIdAsync(args.RowData.ApplicantId);

            if (skData?.Result?.Status == ApplicantStatus.SKSend)
            {
                ActionType = "edit";
                VisiblePopup = true;
                IsEnabled = false;
                TitlePopup = "Edit Applicant";
                _createAdministrationApplicantRequest.Id = args.RowData.Id;
                _createAdministrationApplicantRequest.ScholarshipPostingApplicantId = args.RowData.ApplicantId;
                _createAdministrationApplicantRequest.Name = args.RowData.Name;
                _createAdministrationApplicantRequest.CompanyCode = args.RowData.CompanyCode;
                _createAdministrationApplicantRequest.EmployeeNumber = args.RowData.EmployeeNumber;
                _createAdministrationApplicantRequest.Email = args.RowData.Email;
                _createAdministrationApplicantRequest.ApplicationStatus = args.RowData.Status;
                _createAdministrationApplicantRequest.PRL = args.RowData.Prl;
                _createAdministrationApplicantRequest.MCUStatus = args.RowData.Mcu;
                _createAdministrationApplicantRequest.EmploymentStatus = args.RowData.EmploymentStatus;
                _createAdministrationApplicantRequest.WorkPeriod = args.RowData.WorkPeriod;
                _createAdministrationApplicantRequest.AveragePerformance = args.RowData.PerformanceRating;
                _createAdministrationApplicantRequest.IndisciplinaryRecord = args.RowData.Indiciplinary;
            }
            else if (toeflData?.Result?.Status == ApplicantStatus.Passed)
            {
                ActionType = "edit";
                VisiblePopup = true;
                IsEnabled = false;
                TitlePopup = "Edit Applicant";
                _createAdministrationApplicantRequest.Id = args.RowData.Id;
                _createAdministrationApplicantRequest.ScholarshipPostingApplicantId = args.RowData.ApplicantId;
                _createAdministrationApplicantRequest.Name = args.RowData.Name;
                _createAdministrationApplicantRequest.CompanyCode = args.RowData.CompanyCode;
                _createAdministrationApplicantRequest.EmployeeNumber = args.RowData.EmployeeNumber;
                _createAdministrationApplicantRequest.Email = args.RowData.Email;
                _createAdministrationApplicantRequest.ApplicationStatus = args.RowData.Status;
                _createAdministrationApplicantRequest.PRL = args.RowData.Prl;
                _createAdministrationApplicantRequest.MCUStatus = args.RowData.Mcu;
                _createAdministrationApplicantRequest.EmploymentStatus = args.RowData.EmploymentStatus;
                _createAdministrationApplicantRequest.WorkPeriod = args.RowData.WorkPeriod;
                _createAdministrationApplicantRequest.AveragePerformance = args.RowData.PerformanceRating;
                _createAdministrationApplicantRequest.IndisciplinaryRecord = args.RowData.Indiciplinary;
            }
            else
            {
                ActionType = "edit";
                VisiblePopup = true;
                IsEnabled = true;
                TitlePopup = "Edit Applicant";
                _createAdministrationApplicantRequest.Id = args.RowData.Id;
                _createAdministrationApplicantRequest.ScholarshipPostingApplicantId = args.RowData.ApplicantId;
                _createAdministrationApplicantRequest.Name = args.RowData.Name;
                _createAdministrationApplicantRequest.CompanyCode = args.RowData.CompanyCode;
                _createAdministrationApplicantRequest.EmployeeNumber = args.RowData.EmployeeNumber;
                _createAdministrationApplicantRequest.Email = args.RowData.Email;
                _createAdministrationApplicantRequest.ApplicationStatus = args.RowData.Status;
                _createAdministrationApplicantRequest.PRL = args.RowData.Prl;
                _createAdministrationApplicantRequest.MCUStatus = args.RowData.Mcu;
                _createAdministrationApplicantRequest.EmploymentStatus = args.RowData.EmploymentStatus;
                _createAdministrationApplicantRequest.WorkPeriod = args.RowData.WorkPeriod;
                _createAdministrationApplicantRequest.AveragePerformance = args.RowData.PerformanceRating;
                _createAdministrationApplicantRequest.IndisciplinaryRecord = args.RowData.Indiciplinary;
            }
        }
        else
        {
            IsEnabled = false;
            ActionType = "view";
            VisiblePopup = true;
            TitlePopup = "View Applicant";
            _createAdministrationApplicantRequest.Id = args.RowData.Id;
            _createAdministrationApplicantRequest.ScholarshipPostingApplicantId = args.RowData.ApplicantId;
            _createAdministrationApplicantRequest.Name = args.RowData.Name;
            _createAdministrationApplicantRequest.CompanyCode = args.RowData.CompanyCode;
            _createAdministrationApplicantRequest.EmployeeNumber = args.RowData.EmployeeNumber;
            _createAdministrationApplicantRequest.Email = args.RowData.Email;
            _createAdministrationApplicantRequest.ApplicationStatus = args.RowData.Status;
            _createAdministrationApplicantRequest.PRL = args.RowData.Prl;
            _createAdministrationApplicantRequest.MCUStatus = args.RowData.Mcu;
            _createAdministrationApplicantRequest.EmploymentStatus = args.RowData.EmploymentStatus;
            _createAdministrationApplicantRequest.WorkPeriod = args.RowData.WorkPeriod;
            _createAdministrationApplicantRequest.AveragePerformance = args.RowData.PerformanceRating;
            _createAdministrationApplicantRequest.IndisciplinaryRecord = args.RowData.Indiciplinary;
        }
    }

    #region Popup Event Add/ Edit

    private void DialogOpen()
    {
        VisiblePopup = true;
    }

    private void DialogClose()
    {
        VisiblePopup = false;
        _updateAdministrationApplicantStatusRequest = new UpdateAdministrationApplicantStatusRequest();
    }

    private void ClickedNo()
    {
        VisiblePopup = false;
        _updateAdministrationApplicantStatusRequest = new UpdateAdministrationApplicantStatusRequest();
    }

    private async Task<bool> ValidateApplicantForManualAdd(string employeeId)
    {
        var isValid = true;
        var validationErrors = new List<string>();

        var postingResponse = await _scholarshipPostingService.GetScholarshipPostingByIdAsync(_scholarshipPostingId);
        if (postingResponse.Error != null || postingResponse.Result == null)
        {
            _snackbar.AddError("Failed to get scholarship posting details");
            return false;
        }

        var posting = postingResponse.Result;

        var profile = await _hrisService.GetEmployeeByIdAsync(employeeId);
        if (profile == null)
        {
            _snackbar.AddError("Employee not found in HRIS");
            return false;
        }

        var birthDate = profile.BirthDate ?? DateTime.Now.ToString("MM/dd/yyyy");
        var hiringDate = profile.HiringDate ?? DateTime.Now.ToString("MM/dd/yyyy");
        var mcuStatus = 0;
        if (!string.IsNullOrEmpty(profile.McuResult))
        {
            mcuStatus = (int)Enum.Parse<McuStatus>(profile.McuResult);
        }

        // 1. Validate minimum PRL
        if (Convert.ToInt16(profile.Prl) < posting.Prl)
        {
            isValid = false;
            validationErrors.Add($"PRL ({profile.Prl}) does not meet minimum requirement ({posting.Prl})");
        }

        // 2. Validate maximum age
        var age = Int64Extensions.GetAge(DateTimeExtensions.ToDateTime(birthDate));
        if (age > posting.Age)
        {
            isValid = false;
            validationErrors.Add($"Age ({age}) exceeds maximum limit ({posting.Age})");
        }

        // 3. Validate minimum work period
        var workPeriod = Int64Extensions.GetAge(DateTimeExtensions.ToDateTime(hiringDate));
        if (workPeriod < posting.WorkPeriod)
        {
            isValid = false;
            validationErrors.Add($"Work period ({workPeriod} years) does not meet minimum requirement ({posting.WorkPeriod} years)");
        }

        // 4. Validate indisciplinary
        if (!await _hrisService.IsGrievanceByIdAsync(employeeId))
        {
            isValid = false;
            validationErrors.Add("Indisciplinary record detected");
        }

        // 5. Validate performance rating
        var performanceRate = await _hrisService.GetPerformanceRate(employeeId);
        if (performanceRate < posting.PerformanceRating)
        {
            isValid = false;
            validationErrors.Add($"Performance rating ({performanceRate}) does not meet minimum requirement ({posting.PerformanceRating})");
        }

        // 6. Validate MCU
        // if (mcuStatus == 0 || mcuStatus > posting.Mcu)
        // {
        //     isValid = false;
        //     validationErrors.Add($"MCU status does not meet requirement ({posting.Mcu})");
        // }

        if (!isValid)
        {
            var errorMessage = "<b>Validation Failed:</b><br/>" + string.Join("<br/>", validationErrors);
            _snackbar.AddWarning(errorMessage);
        }

        return isValid;
    }

    private async Task ClickedYes()
    {
        VisiblePopup = false;
        StateHasChanged();
        await Task.Delay(100);

        _isLoadingDialog = true;
        StateHasChanged();

        if (ActionType == "add")
        {
            var validationResult = await _applicantService.GetApplicantByEmployeeIdAsync(_createAdministrationApplicantRequest.EmployeeNumber);
            if (validationResult!.Result!.Items.Where(x => x.ApplicantStatus != ApplicantStatus.Failed || x.ApplicantStatus.Value != ApplicantStatus.Canceled).Any())
            {
                _isLoadingDialog = false;
                VisiblePopup = false;
                _snackbar.AddWarning($"For information, applicant have been <b>registered</b> to participate in this Scholarship Program.<br />");
                StateHasChanged();
                return;
            }

            var fullEmployeeId = $"{_createAdministrationApplicantRequest.EmployeeNumber}";
            var isValidApplicant = await ValidateApplicantForManualAdd(fullEmployeeId);

            if (!isValidApplicant)
            {
                VisiblePopup = false;
                _isLoadingDialog = false;
                StateHasChanged();
                await Task.Delay(100);

                var confirmDialog = await _dialogService.ShowMessageBox(
                    "Validation Warning",
                    "This applicant does not meet all scholarship requirements. Do you want to continue?",
                    yesText: "Continue Anyway",
                    cancelText: "Cancel"
                );

                if (confirmDialog != true)
                {
                    _isLoadingDialog = false;
                    StateHasChanged();
                    return;
                }
            }

            _isLoadingDialog = true;
            StateHasChanged();

            var selection = new SelectionCreate(_selectionOverviewService, _selectionAdministrationService, _selectionTOEFLService, _selectionPsychotestService, _selectionTalentReviewFirstService);

            #region Create Applicant
            var createApplicantRequest = new CreateScholarshipPostingApplicantRequest
            {
                Email = _createAdministrationApplicantRequest.Email,
                EmployeeNumber = _createAdministrationApplicantRequest.EmployeeNumber,
                Name = _createAdministrationApplicantRequest.Name,
                Position = _createAdministrationApplicantRequest.Position,
                ScholarshipPostingId = (Guid)_scholarshipPostingId,
                CompanyId = _createAdministrationApplicantRequest.CompanyCode,
                Gender = _createAdministrationApplicantRequest.Gender
            };

            var createApplicant = await _scholarshipPostingApplicantService.CreateScholarshipPostingApplicantAsync(createApplicantRequest);
            if (createApplicant.Error is not null)
            {
                var result = string.Empty;
                for (var i = 0; i < createApplicant.Error.Details.Count; i++)
                {
                    result = result + createApplicant.Error.Details[i] + ",";
                }

                _snackbar.AddError("Applicant - Error : " + result);
                _isLoadingDialog = false;
                VisiblePopup = false;
                StateHasChanged();
                return;
            }

            #endregion Create Applicant

            #region Create applicant criteria

            var criteria = new CreateApplicantAdministrativeCriteriaRequest
            {
                ApplicantId = createApplicant.Result.Id,
                Prl = Convert.ToInt16(_createAdministrationApplicantRequest.PRL),
                Age = _createAdministrationApplicantRequest.Age,
                WorkPeriod = Convert.ToInt16(_createAdministrationApplicantRequest.WorkPeriod),
                PerformanceRating = _createAdministrationApplicantRequest.AveragePerformance,
                IndiciplinaryStatus = (int)_createAdministrationApplicantRequest.IndisciplinaryRecord,
                McuStatus = (int)_createAdministrationApplicantRequest.MCUStatus
            };
            var responseCriteria = await _registrationService.CreateApplicantAdministrativeCriteriaAsync(criteria);
            if (responseCriteria.Error is not null)
            {
                var result = string.Empty;
                for (var i = 0; i < responseCriteria.Error.Details.Count; i++)
                {
                    result = result + responseCriteria.Error.Details[i] + ",";
                }

                _snackbar.AddError("Applicant - Error : " + result);
                _isLoadingDialog = false;
                VisiblePopup = false;
                StateHasChanged();
                return;
            }

            #endregion Create applicant criteria

            #region Create selection overview

            //create overview
            var createOverview = await selection.CreateSelectionOverview(createApplicant.Result!.Id, null);

            var updateOverviewRequest = new UpdateSelectionOverviewByApplicantIdRequest
            {
                ApplicantId = createApplicant.Result!.Id,
                Stage = _createAdministrationApplicantRequest.ApplicationStatus == ApplicantStatus.Passed ? SelectionStage.TOEFL : SelectionStage.Administration,
                Status = _createAdministrationApplicantRequest.ApplicationStatus == ApplicantStatus.Passed ? ApplicantStatus.InProgress : ApplicantStatus.Failed,
            };

            if (createOverview.Error == null)
            {
                await _selectionOverviewService.UpdateSelectionOverviewByApplicantIdAsync(updateOverviewRequest);
            }

            #endregion Create selection overview

            #region Create selection administration

            var createAdministtaion = await selection.CreateSelectionAdministration(createApplicant.Result.Id);

            #endregion Create selection administration

            #region Create selection TOEFL

            var createToefl = await selection.CreateSelectionToefl(createApplicant.Result.Id);

            #endregion Create selection TOEFL

            VisiblePopup = false;
            _isLoadingDialog = false;

            if (isValidApplicant)
            {
                _snackbar.AddSuccess("Success add applicant");
            }
            else
            {
                _snackbar.AddWarning("Applicant added but does not meet all requirements");
            }

            _updateAdministrationApplicantStatusRequest = new UpdateAdministrationApplicantStatusRequest();
            await SetDataOnLoadAsync();
            StateHasChanged();
        }
        else
        {
            #region TOEFL Schedule Validation
            var getTOEFL = await _selectionTOEFLService.GetSelectionToeflByApplicantIdAsync(_createAdministrationApplicantRequest.ScholarshipPostingApplicantId);

            if (getTOEFL.Result is not null)
            {
                if (getTOEFL.Result!.Status != ApplicantStatus.InProgress)
                {
                    _snackbar.AddWarning($"Applicant cannot be updated, because TOEFL Status is {getTOEFL.Result!.Status}");
                    VisiblePopup = false;
                    _isLoadingDialog = false;
                    StateHasChanged();
                    return;
                }

                if (getTOEFL.Result.Status == ApplicantStatus.InProgress)
                {
                    var scholarshipPostingId = _createAdministrationApplicantRequest.Id != Guid.Empty
                        ? _selectionAdministrations.FirstOrDefault(x => x.Id == _createAdministrationApplicantRequest.Id)?.PostingId
                        : _scholarshipPostingId;

                    if (scholarshipPostingId == null || scholarshipPostingId == Guid.Empty)
                    {
                        _snackbar.AddError("Scholarship Posting ID not found");
                        VisiblePopup = false;
                        _isLoadingDialog = false;
                        StateHasChanged();
                        return;
                    }

                    var scheduleValidationRequest = new GetSelectionTOEFLScheduleValidationRequest
                    {
                        ApplicantId = _createAdministrationApplicantRequest.ScholarshipPostingApplicantId,
                        ScholarshipPostingId = scholarshipPostingId.Value
                    };

                    var getTOEFLScheduleValidation = await _selectionTOEFLService.GetSelectionToeflScheduleValidationAsync(scheduleValidationRequest);

                    if (getTOEFLScheduleValidation?.Result?.Items != null && getTOEFLScheduleValidation.Result.Items.Any())
                    {
                        var hasSchedule = getTOEFLScheduleValidation.Result.Items.First().HasSchedule;

                        if (hasSchedule)
                        {
                            _snackbar.AddWarning("Applicant cannot be updated, because Applicant already has a Test Schedule");
                            VisiblePopup = false;
                            _isLoadingDialog = false;
                            StateHasChanged();
                            return;
                        }
                    }
                }
            }
            #endregion

            #region Update selection administration

            _updateAdministrationApplicantStatusRequest.Id = _createAdministrationApplicantRequest.Id;
            _updateAdministrationApplicantStatusRequest.Status = _createAdministrationApplicantRequest.ApplicationStatus;

            var response = await _selectionAdministrationService.UpdateAdministrationApplicantStatusAsync(_updateAdministrationApplicantStatusRequest);
            if (response.Error is not null)
            {
                var result = string.Empty;
                for (var i = 0; i < response.Error.Details.Count; i++)
                {
                    result = result + response.Error.Details[i] + ",";
                }

                _snackbar.AddError("Administration - Error : " + result);
                _isLoadingDialog = true;
                return;
            }

            #endregion Update selection administration

            //#region Update TOEFL when status pass
            //if (_createAdministrationApplicantRequest.ApplicationStatus == ApplicantStatus.Passed)
            //{
            //    var getToefl = await _selectionTOEFLService.GetSelectionToeflByApplicantIdAsync(_createAdministrationApplicantRequest.ScholarshipPostingApplicantId);
            //    if (getToefl is not null)
            //    {
            //        var updateTOEFLRequest = new UpdateSelectionTOEFLRequest
            //        {
            //            Id = getToefl.Result!.Id,
            //            Status = ApplicantStatus.InProgress,
            //            ScholarshipPostingApplicantId = _createAdministrationApplicantRequest.ScholarshipPostingApplicantId,
            //        };

            //        var updateTOEFL = await _selectionTOEFLService.UpdateSelectionToeflAsync(updateTOEFLRequest);
            //        if (updateTOEFL.Error is not null)
            //        {
            //            var result = string.Empty;
            //            for (var i = 0; i < updateTOEFL.Error.Details.Count; i++)
            //            {
            //                result = result + updateTOEFL.Error.Details[i] + ",";
            //            }

            //            _snackbar.AddError("TOEFL - Error : " + result);
            //            _isLoadingDialog = false;
            //            return;
            //        }
            //    }
            //}
            //#endregion

            #region Update selection overview

            var updateOverviewRequest = new UpdateSelectionOverviewByApplicantIdRequest
            {
                ApplicantId = _createAdministrationApplicantRequest.ScholarshipPostingApplicantId,
                Stage = _createAdministrationApplicantRequest.ApplicationStatus == ApplicantStatus.Failed ? SelectionStage.Administration : SelectionStage.TOEFL,
                Status = _createAdministrationApplicantRequest.ApplicationStatus,
            };

            var updateOverview = await _selectionOverviewService.UpdateSelectionOverviewByApplicantIdAsync(updateOverviewRequest);
            if (updateOverview.Error is not null)
            {
                var result = string.Empty;
                for (var i = 0; i < updateOverview.Error.Details.Count; i++)
                {
                    result = result + updateOverview.Error.Details[i] + ",";
                }

                _snackbar.AddError("Overview - Error : " + result);
                _isLoadingDialog = true;
                return;
            }

            #endregion Update selection overview

            VisiblePopup = false;
            _updateAdministrationApplicantStatusRequest = new UpdateAdministrationApplicantStatusRequest();
            _snackbar.AddSuccess("Success update data");
            _isLoadingDialog = false;
            await SetDataOnLoadAsync();
            StateHasChanged();
        }
    }

    #endregion Popup Event Add/ Edit

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
        var response = await _selectionAdministrationService.DeleteSelectionAdministrationAsync(_createAdministrationApplicantRequest!.Id);
        if (response.Error is null)
        {
            _snackbar.AddSuccess("Success delete data");
            await SetDataOnLoadAsync();
            StateHasChanged();
        }
    }

    #endregion Popup Delete

    public static List<ApplicantStatusResponse> GetEnumDisplayNames<T>()
    {
        var type = typeof(T);
        return Enum.GetValues(type)
                   .Cast<T>()
                   .Select(x => new ApplicantStatusResponse
                   {
                       Value = x as ApplicantStatus?,
                       Text = type.GetMember(x!.ToString()!)
                   .First()
                   .GetCustomAttribute<DisplayAttribute>()?.Name ?? x.ToString()
                   }).Where(x => x.Value is ApplicantStatus.Passed || x.Value == ApplicantStatus.Failed).ToList();
    }

    public static List<EmploymentStatusResponse> GetEnumDisplayNamesEmploymentStatus<T>()
    {
        var type = typeof(T);
        return Enum.GetValues(type)
                   .Cast<T>()
                   .Select(x => new EmploymentStatusResponse
                   {
                       Value = x as EmploymentStatus?,
                       Text = type.GetMember(x!.ToString()!)
                   .First()
                   .GetCustomAttribute<DisplayAttribute>()?.Name ?? x.ToString()
                   }).ToList();
    }

    public static List<MCUStatusResponse> GetEnumDisplayNamesMCUStatus<T>()
    {
        var type = typeof(T);
        return Enum.GetValues(type)
                   .Cast<T>()
                   .Select(x => new MCUStatusResponse
                   {
                       Value = x as McuStatus?,
                       Text = type.GetMember(x!.ToString()!)
                   .First()
                   .GetCustomAttribute<DisplayAttribute>()?.Name ?? x.ToString()
                   }).ToList();
    }

    public static List<IndiciplinaryStatusResponse> GetEnumDisplayIndiciplinaryStatus<T>()
    {
        var type = typeof(T);
        return Enum.GetValues(type)
                   .Cast<T>()
                   .Select(x => new IndiciplinaryStatusResponse
                   {
                       Value = x as IndiciplinaryStatus?,
                       Text = type.GetMember(x!.ToString()!)
                   .First()
                   .GetCustomAttribute<DisplayAttribute>()?.Name ?? x.ToString()
                   }).ToList();
    }

    private void OnClickAddAsync()
    {
        ActionType = "add";
        IsEnabled = true;
        TitlePopup = "Add Applicant";
        if (_scholarshipPostingId.HasValue && _scholarshipPostingId.Value != Guid.Empty)
        {
            _createAdministrationApplicantRequest.ScholarshipPostingApplicantId = Guid.NewGuid();
            _createAdministrationApplicantRequest.ApplicationStatus = ApplicantStatus.Passed;
            _createAdministrationApplicantRequest.MCUStatus = McuStatus.No;

            VisiblePopup = true;
        }
        else
        {
            _snackbar.AddWarning("Please select code !");
        }
    }

    public async Task SearchClick()
    {
        _isLoadingDialog = true;
        if (_createAdministrationApplicantRequest.EmployeeNumber is null)
        {
            IsShowError = true;
            ErrorMessage = "Please input employee number";
            _isLoadingDialog = false;
            StateHasChanged();
            return;
        }

        var dataHRIS = await _hrisService.GetEmployeeByIdAsync(_createAdministrationApplicantRequest.EmployeeNumber);
        var dataMasterHRIS = await _hrisService.GetEmployeeMasterDataByIdAsync(_createAdministrationApplicantRequest.EmployeeNumber);
        var dataPerformanceRating = await _hrisService.GetPerformanceRate(_createAdministrationApplicantRequest.EmployeeNumber);
        var dataIndiciplinaryStatus = await _hrisService.IsGrievanceByIdAsync(_createAdministrationApplicantRequest.EmployeeNumber);
        if (dataHRIS is not null)
        {
            if (dataMasterHRIS.HiringDate is not null)
            {
                var splitHiringDate = dataMasterHRIS.HiringDate.Split(" ")[0].Split("/");
                var hiringDate = new DateTime(Convert.ToInt32(splitHiringDate[2]), Convert.ToInt32(splitHiringDate[0]), Convert.ToInt32(splitHiringDate[1]), 00, 00, 00);
                _createAdministrationApplicantRequest.WorkPeriod = Convert.ToInt16(Int64Extensions.GetAge(Convert.ToDateTime(hiringDate)));
            }

            if (dataHRIS.BirthDate is not null)
            {
                var splitBirthDate = dataHRIS.BirthDate.Split(" ")[0].Split("/");
                var birthDate = new DateTime(Convert.ToInt32(splitBirthDate[2]), Convert.ToInt32(splitBirthDate[0]), Convert.ToInt32(splitBirthDate[1]), 00, 00, 00);
                _createAdministrationApplicantRequest.Age = Convert.ToInt16(Int64Extensions.GetAge(Convert.ToDateTime(birthDate)));
            }

            if (string.IsNullOrEmpty(dataHRIS.EmployeeName))
            {
                IsShowError = true;
                ErrorMessage = "Employee not found in HRIS !";

                var employeeNumber = _createAdministrationApplicantRequest.EmployeeNumber;
                _createAdministrationApplicantRequest = new()
                {
                    EmployeeNumber = employeeNumber
                };

                _isLoadingDialog = false;
                StateHasChanged();
                return;
            }

            _createAdministrationApplicantRequest.Name = dataHRIS.EmployeeName;
            _createAdministrationApplicantRequest.CompanyCode = dataHRIS.CompanyCode;
            _createAdministrationApplicantRequest.Email = dataHRIS.Email is not null ? dataHRIS.Email : "";
            _createAdministrationApplicantRequest.EmploymentStatus = EmploymentStatus.Pwtt;
            _createAdministrationApplicantRequest.PRL = Convert.ToInt32(dataHRIS.Prl);
            _createAdministrationApplicantRequest.AveragePerformance = dataPerformanceRating;
            _createAdministrationApplicantRequest.IndisciplinaryRecord = (IndiciplinaryStatus)Convert.ToInt32(!dataIndiciplinaryStatus);
            _createAdministrationApplicantRequest.MCUStatus = dataHRIS.McuResult is not null ? Enum.Parse<McuStatus>(dataHRIS.McuResult) : McuStatus.No;
            _createAdministrationApplicantRequest.Gender = Int64Extensions.StringToInt(dataHRIS.GenderCode);
            _createAdministrationApplicantRequest.Position = dataMasterHRIS.PositionName;

            _snackbar.AddInfo("Success retrive data from HRIS");
            IsShowError = false;
            ErrorMessage = string.Empty;
            _isLoadingDialog = false;

            StateHasChanged();
        }
        else
        {
            IsShowError = true;
            ErrorMessage = "Employee not found in HRIS !";
            _isLoadingDialog = false;
            StateHasChanged();
        }
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

    private string GetStatusForUpload(string status)
    {
        var result = string.Empty;
        if (status == "Pass")
        {
            result = "Passed";
        }
        else if (status == "Fail")
        {
            result = "Failed";
        }
        else if (status == "In Progress TRM")
        {
            result = "InProgressTRM";
        }
        else if (status == "In Progress SK")
        {
            result = "InProgressSK";
        }
        else if (status == "SK Created")
        {
            result = "SKCreated";
        }
        else if (status == "SK Send")
        {
            result = "SKSend";
        }
        else if (status == "Approved by Director")
        {
            result = "ApprovedDirector";
        }
        else if (status == "In Progress BA")
        {
            result = "InProgressBA";
        }
        else if (status == "In Progress P-Office")
        {
            result = "InProgressPoffice";
        }
        else if (status == "In Progress P-Sign")
        {
            result = "InProgressPsign";
        }

        return result;
    }

    private string GetStageForUpload(string stage)
    {
        var result = string.Empty;
        if (stage == "Talent Review Meeting")
        {
            result = "TalentReview";
        }
        else if (stage == "Create SK")
        {
            result = "Decree";
        }

        return result;
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
    public void ExcelQueryCellInfoHandler(ExcelQueryCellInfoEventArgs<GetSelectionAdministrationQueryResponse> args)
    {
        var a = args.Column.Field;
        if (args.Column.Field == "Status")
        {
            args.Cell.Value = args.Data.Status.GetDescription().ToString();
        }
    }
}
