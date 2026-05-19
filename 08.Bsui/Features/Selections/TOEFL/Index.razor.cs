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
using Smlp.Bsui.Features.Selections.Actions;
using Smlp.Bsui.Features.Selections.TOEFL.Constants;
using Smlp.Shared.Common.Constants;
using Smlp.Shared.Common.Extensions;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Documents.Commands.CreateDocument;
using Smlp.Shared.Documents.Queries.GetDocumentFile;
using Smlp.Shared.Documents.Queries.GetDocuments;
using Smlp.Shared.Masters.Queries;
using Smlp.Shared.Scholarships.ScholarshipPosting.Queries;
using Smlp.Shared.Selections.SelectionOverviews.Commands.Update;
using Smlp.Shared.Selections.SelectionOverviews.Queries;
using Smlp.Shared.Selections.SelectionTOEFLs.Commands.Update;
using Smlp.Shared.Selections.SelectionTOEFLs.Queries;
using Syncfusion.Blazor.Data;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Inputs;
using Syncfusion.XlsIO;
using Syncfusion.Blazor.Navigations;
using SelectionType = Smlp.Base.Selections.Enums.SelectionType;
using Smlp.Shared.Selections.SelectionPsychotests.Queries;

namespace Smlp.Bsui.Features.Selections.TOEFL;

public partial class Index
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

    protected ClaimsPrincipal _user = default!;

    [Parameter]
    public int Type { get; set; }

    protected List<GetCompanyQueryResponse> _dropdownCompaniesAdd = new();
    protected List<GetSelectionTOEFLQueryResponse> _selectionTOEFLs = new();
    private SfGrid<GetSelectionTOEFLQueryResponse> Grid { get; set; } = new();
    protected List<GetSelectionTOEFLHihgestChartResponse>? _getSelectionTOEFLHihgestChartResponse = new();
    protected List<GetSelectionTOEFLAverageChartResponse>? _getSelectionTOEFLAverageChartResponse = new();
    public IEnumerable<GetSelectionTOEFLStatusListResponse> _getSelectionTOEFLStatusListResponses = GetEnumDisplayNames<ApplicantStatus>();
    public UpdateSelectionTOEFLRequest _requestTOEFL = new();
    public UpdateSelectionTOEFLRequest _updateSelectionTOEFLRequest = new();
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

    public FileUploadResponse FileUploads { get; set; } = new();
    protected List<GetDocumentsDocument> _getListDocuments = new();

    protected ErrorResponse? _error;
    protected bool _isLoading = false;
    protected bool _isLoadingData = false;
    protected bool _isLoadingAverageChart = false;
    protected bool _isLoadingHighestChart = false;
    protected Guid? _scholarshipPostingId = null;
    protected string? _companyCode = null;
    protected string? _companyCodeApplicantFilter = null;
    protected int? _companyGroupId = null;
    protected int? _companyGroupIdApplicantFilter = null;
    protected int _directorateId = 0;
    protected int _divisionId = 0;
    protected string? _codeId = null;
    protected int? _batch = null;
    protected int? _year = null;
    protected ScholarshipLocation _category = 0;
    protected int _countApplicant = 0;
    protected int _toeflScore = 0;
    protected int _countApplicantPassed = 0;
    protected int _countApplicantRejected = 0;
    protected string _applicantSummary = "0";
    protected string _inProgressSummary = "0";
    protected string _passedSummary = "0";
    protected string _failedSummary = "0";
    protected string _canceledSummary = "0";
    protected string _toeflAverageSummary = "0";
    protected long _maximumFileSizeInBytes;
    private bool VisiblePopup { get; set; }
    private string TitlePopup { get; set; } = "Edit Applicant";
    private bool VisiblePopupDelete { get; set; } = false;
    private string? DeleteType { get; set; }
    protected Guid DocumentId { get; set; } = Guid.Empty;
    protected string _actionType = "0000";
    private bool IsEnabled { get; set; }
    private bool IsEnabledSKStatus { get; set; }
    private bool IsAuploadedDocument { get; set; } = false;
    protected bool _isLoadingDialog = false;
    protected SfUploader? _uploadObj;

    protected override async Task OnInitializedAsync()
    {
        _user = (await AuthenticationStateTask).User;
        //_companyCode = _user.GetCompanyCode();
        _isLoading = true;
        _maximumFileSizeInBytes = _documentOptions.Value.MaximumFileSizeInBytes;
        await GetDropdownCompanyGroup();
        await GetDropdownCompany();
        await GetListCompany();
        await GetDropdownCategories();
        await GetDropdownCompanyGroupApplicantFilter();
        await GetDropdownCompanyApplicantFilter();
        await GetDropdownCodes();
        await SetDataOnLoadAsync();
        _isLoading = false;
    }

    private async Task SetDataOnLoadAsync()
    {
        _isLoadingData = true;
        var param = new GetSelectionTOEFLRequest
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
        var response = await _selectionTOEFLService.GetSelectionTeoflListAsync(param);

        if (response.Error is null)
        {
            _selectionTOEFLs = response.Result!.Items.ToList();
            _applicantSummary = _selectionTOEFLs.Count().ToString();
            _passedSummary = _selectionTOEFLs.Where(x => x.Status == ApplicantStatus.Passed).ToList().Count().ToString() + "/" + _applicantSummary;
            _failedSummary = _selectionTOEFLs.Where(x => x.Status == ApplicantStatus.Failed).ToList().Count().ToString() + "/" + _applicantSummary;
            _canceledSummary = _selectionTOEFLs.Where(x => x.Status == ApplicantStatus.Canceled).ToList().Count().ToString() + "/" + _applicantSummary;
            _inProgressSummary = _selectionTOEFLs.Where(x => x.Status == ApplicantStatus.InProgress).ToList().Count().ToString() + "/" + _applicantSummary;
            var total = _selectionTOEFLs.Sum(x => x.Score);
            var count = _selectionTOEFLs.Count;
            var average = (float)total / (float)count;
            if (!float.IsNaN(average))
            {
                _toeflAverageSummary = average == float.NaN ? "0" : average.ToString("n2");
            }
            else
            {
                _toeflAverageSummary = "0";
            }
        }
        else
        {
            _error = response.Error;
            _isLoadingData = false;
        }

        _isLoadingData = false;
    }

    private async Task GetListCompany()
    {
        var companies = await _companyService.GetCompaniesAsync();
        if (companies.Error is null)
        {
            _dropdownCompaniesAdd = companies.Result!.Items.ToList();
            _dropdownCompaniesAdd = _dropdownCompaniesAdd.OrderBy(x => x.Code).ToList();
        }
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

    #region Dropdown Change

    public async Task OnChangeYear(Syncfusion.Blazor.DropDowns.ChangeEventArgs<int, DropdownItemResponse> args)
    {
        _year = args.Value;
        await SetDataOnLoadAsync();
        StateHasChanged();
    }
    public async Task OnChangeBatchAsync(Syncfusion.Blazor.DropDowns.ChangeEventArgs<int?, ScholarshipBatchNewResponse> args)
    {
        if (args.ItemData == null)
        {
            _batch = 0;
        }
        else
        {
            _batch = (int)args.Value!;
        }

        await SetDataOnLoadAsync();
        StateHasChanged();
    }

    public async Task OnChangeCompanyAsync(Syncfusion.Blazor.DropDowns.ChangeEventArgs<string, GetCompanyQueryResponse> args)
    {
        _companyCode = args.ItemData.Code;

        await GetDropdownDirectorate();
        await SetDataOnLoadAsync();
        StateHasChanged();
    }

    public async Task OnChangeCodeAsync(Syncfusion.Blazor.DropDowns.ChangeEventArgs<Guid, GetScholarshipPostingDropdownResponse> args)
    {
        _codeId = args.ItemData.Code;
        _scholarshipPostingId = args.ItemData.Id;

        await SetDataOnLoadAsync();
        StateHasChanged();
    }

    public async Task OnChangeCompanyGroupAsync(Syncfusion.Blazor.DropDowns.ChangeEventArgs<int, GetCompanyGroupQueryResponse> args)
    {
        _companyGroupId = args.ItemData.Id;
        await GetDropdownCompany();
        await SetDataOnLoadAsync();
        StateHasChanged();
    }

    #endregion Dropdown Change

    private async Task GetToeflHihgestChartAsync()
    {
        _isLoadingHighestChart = true;
        var param = new GetSelectionTOEFLRequest
        {
            CompanyCode = _companyCode,
            Category = _category,
            Batch = _scholarshipPostingId
        };
        var response = await _selectionTOEFLService.GetSelectionTeoflHighestChartAsync(param);
        if (response.Error is null)
        {
            _getSelectionTOEFLHihgestChartResponse = response.Result!.Items.ToList();
        }

        _isLoadingHighestChart = false;
    }

    private async Task GetToeflAverageChartAsync()
    {
        _isLoadingAverageChart = true;
        var param = new GetSelectionTOEFLRequest
        {
            CompanyCode = _companyCode,
            Category = _category,
            Batch = _scholarshipPostingId
        };
        var response = await _selectionTOEFLService.GetSelectionTeoflAverageChartAsync(param);
        if (response.Error is null)
        {
            _getSelectionTOEFLAverageChartResponse = response.Result!.Items.ToList();
        }

        _isLoadingAverageChart = false;
    }

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
                //_snackbar.AddError(ex.Message);
            }
        }
    }

    protected void OnRemoveUploaderApplicants(RemovingEventArgs args)
    {
        _workbookApplicants = new();
        args.PostRawFile = false;
    }

    protected void ValueChangeUploaderToeflCertificate(UploadChangeEventArgs args)
    {
        FileUploads.FileName = args.Files.First().File.Name;
        FileUploads.FileSize = args.Files.First().File.Size;
        FileUploads.FileContentType = args.Files.First().File.ContentType;
        FileUploads.File = args.Files.First().File;
    }

    protected void OnRemoveUploaderToeflCertificate(RemovingEventArgs args)
    {
        FileUploads = new();
        args.PostRawFile = false;
    }

    protected DataTable _table = new();
    protected List<GetSelectionOverviewQueryResponse> _workbookApplicants = new();

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
                    new GetSelectionOverviewQueryResponse
                    {
                        Id = new Guid((string)row["Id"]),
                        PostingId = new Guid((string)row["ScholarshipId"]),
                        ApplicantId = new Guid((string)row["ApplicantId"]),
                        Name = (string)row["Name"],
                        CompanyCode = (string)row["CompanyCode"],
                        EmployeeNumber = (string)row["EmployeeNumber"],
                        Email = (string)row["Email"],
                        Filename = (string)row["Filename"],
                        TOEFLScore = (string)row["Score"],
                        Stage = (SelectionStage)(int)Enum.Parse<SelectionStage>(stage),
                        Status = (ApplicantStatus)(int)Enum.Parse<ApplicantStatus>(status),
                    });
            }
        }
    }

    private DataTable DataTableExport()
    {
        var table = new DataTable();

        table.Columns.Add("Id", typeof(string));
        table.Columns.Add("ScholarshipId", typeof(string));
        table.Columns.Add("ApplicantId", typeof(string));
        table.Columns.Add("Name", typeof(string));
        table.Columns.Add("CompanyCode", typeof(string));
        table.Columns.Add("EmployeeNumber", typeof(string));
        table.Columns.Add("Faculty", typeof(string));
        table.Columns.Add("Major", typeof(string));
        table.Columns.Add("Email", typeof(string));
        table.Columns.Add("Filename", typeof(string));
        table.Columns.Add("Score", typeof(string));
        table.Columns.Add("Bussines Issue", typeof(string));
        table.Columns.Add("Mentor 1", typeof(string));
        table.Columns.Add("Mentor 2", typeof(string));
        table.Columns.Add("Stage", typeof(string));
        table.Columns.Add("Status", typeof(string));

        if (_selectionTOEFLs is not null)
        {
            var dataSelectionToefls = _selectionTOEFLs.Where(x => x.Status != ApplicantStatus.Canceled).ToList();
            foreach (var item in dataSelectionToefls)
            {
                table.Rows.Add(
                    item.Id,
                    item.PostingId,
                    item.ApplicantId,
                    item.Name,
                    item.CompanyCode,
                    item.EmployeeNumber,
                    item.Faculty,
                    item.Major,
                    item.Email,
                    item.Filename,
                    item.Score,
                    item.BussinesIssue,
                    item.MentorName1,
                    item.MentorName2,
                    item.Stage.GetDescription(),
                    item.Status.GetDescription());
            }
        }

        return table;
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
            foreach (var item in _workbookApplicants.Where(x => x.Stage is SelectionStage.TOEFL or SelectionStage.Psychotest))
            {
                var getTOEFL = await _selectionTOEFLService.GetSelectionToeflByApplicantIdAsync(item.ApplicantId);
                var getScholarshipToeflScore = await _scholarshipPostingService.GetScholarshipPostingByIdAsync(item.PostingId);
                //if (getTOEFL.Result!.Document is null)
                //{
                //    _snackbar.AddWarning("Please upload TOEFL Certificate !");
                //    _isLoadingDialog = false;
                //    StateHasChanged();
                //    return;
                //}

                #region Upload File

                if (!string.IsNullOrEmpty(item.Filename))
                {
                    var requestFile = new CreateDocumentByTypeRequest
                    {
                        DocumentType = DocumentType.CertificateTOEFL,
                        Filename = item.Filename,
                    };

                    var resultUpload = await _documentService.CreateDocumentByTypeAsync(requestFile);
                    if (resultUpload.Error is not null)
                    {
                        var result = string.Empty;
                        for (var i = 0; i < resultUpload.Error.Details.Count; i++)
                        {
                            result = result + resultUpload.Error.Details[i] + ",";
                        }

                        _snackbar.AddError("Document - Error : " + result);
                        _isLoadingDialog = false;
                        StateHasChanged();
                        return;
                    }

                    DocumentId = resultUpload.Result!.DocumentId;
                }

                #endregion Upload File

                #region Update selection TOEFL
                _updateSelectionTOEFLRequest.Id = item.Id;
                _updateSelectionTOEFLRequest!.Status = item.Status;
                _updateSelectionTOEFLRequest.ScholarshipPostingApplicantId = item.ApplicantId;
                _updateSelectionTOEFLRequest!.Score = float.Parse(item.TOEFLScore);
                _updateSelectionTOEFLRequest!.TestDate = getTOEFL.Result!.TestDate is not null ? getTOEFL.Result!.TestDate : DateOnly.FromDateTime(DateTime.Now);
                _updateSelectionTOEFLRequest!.Expired = getTOEFL.Result!.Expired is not null ? getTOEFL.Result!.Expired : DateOnly.FromDateTime(DateTime.Now).AddYears(2);
                _updateSelectionTOEFLRequest.DocumentId = DocumentId;

                // validation toefl
                if (_updateSelectionTOEFLRequest.Score >= getScholarshipToeflScore.Result!.TOEFLScore)
                {
                    _updateSelectionTOEFLRequest.Status = ApplicantStatus.Passed;
                }
                else
                {
                    _updateSelectionTOEFLRequest.Status = ApplicantStatus.Failed;
                }

                //if (_updateSelectionTOEFLRequest!.Score > 1500)
                //{
                //    _snackbar.AddWarning("Score maximal 1500!!");
                //    _isLoadingDialog = false;
                //    StateHasChanged();
                //    return;
                //}

                var response = await _selectionTOEFLService.UpdateSelectionToeflAsync(_updateSelectionTOEFLRequest);
                if (response.Error is not null)
                {
                    var result = string.Empty;
                    for (var i = 0; i < response.Error.Details.Count; i++)
                    {
                        result = result + response.Error.Details[i] + ",";
                    }

                    _snackbar.AddError("TOEFL - Error : " + result);
                    _isLoadingDialog = false;
                    return;
                }

                #endregion Update selection TOEFL

                #region Update selection overview

                var updateOverviewRequest = new UpdateSelectionOverviewByApplicantIdRequest
                {
                    ApplicantId = item.ApplicantId,
                    Stage = _updateSelectionTOEFLRequest.Status == ApplicantStatus.Passed ? SelectionStage.Psychotest : SelectionStage.TOEFL,
                    Status = _updateSelectionTOEFLRequest.Status == ApplicantStatus.Passed ? ApplicantStatus.InProgress : ApplicantStatus.Failed,
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

                #region Update selection psychotest when passed

                if (_updateSelectionTOEFLRequest.Status == ApplicantStatus.Passed)
                {
                    var selection = new SelectionCreate(_selectionOverviewService, _selectionAdministrationService, _selectionTOEFLService, _selectionPsychotestService, _selectionTalentReviewFirstService);
                    var getPsychotest = await _selectionPsychotestService.GetSelectionPsychotestByApplicantIdAsync(item.ApplicantId);
                    if (getPsychotest is not null)
                    {
                        if (getPsychotest.Result!.Id == Guid.Empty)
                        {
                            if (_updateSelectionTOEFLRequest!.Status == ApplicantStatus.Passed)
                            {
                                #region Create selection psychotest

                                await selection.CreateSelectionPsychotest(item.ApplicantId);

                                #endregion Create selection psychotest
                            }
                        }
                    }
                }

                #endregion Update selection psychotest when passed
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

        worksheet.Range["G1"].ColumnWidth = 40;
        worksheet.Range["H1"].ColumnWidth = 40;
        worksheet.Range["I1"].ColumnWidth = 40;
        worksheet.Range["J1"].ColumnWidth = 40;
        worksheet.Range["L1"].ColumnWidth = 30;
        worksheet.Range["M1"].ColumnWidth = 30;
        worksheet.Range["N1"].ColumnWidth = 30;
        worksheet.Range["O1"].ColumnWidth = 30;
        worksheet.Range["P1"].ColumnWidth = 30;

        var headingStyle = workbook.Styles.Add("HeadingStyle");
        headingStyle.Font.Bold = true;
        worksheet.Range["J1:J" + (_selectionTOEFLs.Count() + 1)].CellStyle = headingStyle;
        worksheet.Range["K1:K" + (_selectionTOEFLs.Count() + 1)].CellStyle = headingStyle;
        worksheet.Range["P1:P" + (_selectionTOEFLs.Count() + 1)].CellStyle = headingStyle;

        worksheet.Protect("sch0larship");
        worksheet.Range["J2:J" + (_selectionTOEFLs.Count() + 1)].CellStyle.Locked = false;
        worksheet.Range["K2:K" + (_selectionTOEFLs.Count() + 1)].CellStyle.Locked = false;

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

        IComment commentL = worksheet.Range["L1"].AddComment();
        commentL.Text = "Do not modify";
        commentL.IsVisible = false;

        IComment commentM = worksheet.Range["M1"].AddComment();
        commentM.Text = "Do not modify";
        commentM.IsVisible = false;

        IComment commentN = worksheet.Range["N1"].AddComment();
        commentN.Text = "Do not modify";
        commentN.IsVisible = false;

        IComment commentO = worksheet.Range["O1"].AddComment();
        commentO.Text = "Do not modify";
        commentO.IsVisible = false;

        IComment commentP = worksheet.Range["P1"].AddComment();
        commentP.Text = "Do not modify";
        commentP.IsVisible = false;

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
            $"SelectionTOEFLs_{DateTime.Now:yyyyMMdd}.xlsx",
            $"{ContentTypes.ApplicationOpenXmlExcel}",
            content);
    }

    public static List<GetSelectionTOEFLStatusListResponse> GetEnumDisplayNames<T>()
    {
        var type = typeof(T);
        return Enum.GetValues(type)
                   .Cast<T>()
                   .Select(x => new GetSelectionTOEFLStatusListResponse
                   {
                       Value = x as ApplicantStatus?,
                       Text = type.GetMember(x!.ToString()!)
                   .First()
                   .GetCustomAttribute<DisplayAttribute>()?.Name ?? x.ToString()
                   }).Where(x => x.Value is ApplicantStatus.Passed || x.Value == ApplicantStatus.Failed).ToList();
    }

    private void OnClickAddAsync(MouseEventArgs e)
    {
        IsEnabled = true;
        VisiblePopup = true;
    }

    #region Popup Event Add/Edit

    private void DialogOpen()
    {
        VisiblePopup = true;
    }

    private void DialogClose()
    {
        VisiblePopup = false;
        _requestTOEFL = new UpdateSelectionTOEFLRequest();
    }

    private void ClickedNo()
    {
        VisiblePopup = false;
        _requestTOEFL = new UpdateSelectionTOEFLRequest();
    }

    private async Task ClickedYes()
    {
        var selection = new SelectionCreate(_selectionOverviewService, _selectionAdministrationService,
            _selectionTOEFLService, _selectionPsychotestService, _selectionTalentReviewFirstService);
        _isLoadingDialog = true;

        #region Psychotest Schedule Validation
        var getTOEFL = await _selectionTOEFLService.GetSelectionToeflByApplicantIdAsync(_requestTOEFL.ScholarshipPostingApplicantId);
        Guid? scholarshipPostingId = null;

        if (getTOEFL?.Result != null)
        {
            scholarshipPostingId = getTOEFL.Result.PostingId;
        }

        var getPsychotest = await _selectionPsychotestService.GetSelectionPsychotestByApplicantIdAsync(_requestTOEFL.ScholarshipPostingApplicantId);
        if (getPsychotest is not null)
        {
            if (getPsychotest.Result!.Id != Guid.Empty)
            {
                if (getPsychotest.Result!.Status != ApplicantStatus.InProgress)
                {
                    _snackbar.AddWarning($"Cannot update, because Psychotest status is {getPsychotest.Result.Status}");
                    VisiblePopup = false;
                    _isLoadingDialog = false;
                    StateHasChanged();
                    return;
                }
            }

            if (getPsychotest.Result.Status == ApplicantStatus.InProgress)
            {
                var scheduleValidationRequest = new GetSelectionPsychotestScheduleValidationRequest
                {
                    ApplicantId = _requestTOEFL.ScholarshipPostingApplicantId,
                    ScholarshipPostingId = scholarshipPostingId
                };

                var getPsychotestScheduleValidation = await _selectionPsychotestService
                    .GetSelectionPsychotestScheduleValidationAsync(scheduleValidationRequest);

                if (getPsychotestScheduleValidation?.Result?.Items != null &&
                    getPsychotestScheduleValidation.Result.Items.Any())
                {
                    var hasSchedule = getPsychotestScheduleValidation.Result.Items.First().HasSchedule;

                    if (hasSchedule)
                    {
                        _snackbar.AddWarning("Applicant cannot be updated, because Applicant already has a Psychotest Schedule");
                        VisiblePopup = false;
                        _isLoadingDialog = false;
                        StateHasChanged();
                        return;
                    }
                }
            }
        }

        if (_requestTOEFL.TestDate is null)
        {
            _snackbar.AddWarning($"Please Add Test Date!");
            _isLoadingDialog = false;
            StateHasChanged();
            return;
        }

        if (_requestTOEFL.Expired is null)
        {
            _snackbar.AddWarning($"Please Add Expired Date!");
            _isLoadingDialog = false;
            StateHasChanged();
            return;
        }

        if (FileUploads.FileName != null)
        {
            DocumentId = Guid.Empty;
            await UploadToeflCertificate(_requestTOEFL.EmployeeNumber);
        }

        if (IsAuploadedDocument == false)
        {
            if (FileUploads.FileName is null)
            {
                _snackbar.AddWarning("Please browse TOEFL Certificate !");
                _isLoadingDialog = false;
                StateHasChanged();
                return;
            }

            await UploadToeflCertificate(_requestTOEFL.EmployeeNumber);
        }

        if (DocumentId == Guid.Empty)
        {
            _snackbar.AddWarning("Please please upload certificate !");
            _isLoadingDialog = false;
            StateHasChanged();
            return;
        }
        #endregion

        #region Update TOEFL

        _updateSelectionTOEFLRequest.Id = _requestTOEFL.Id;
        _updateSelectionTOEFLRequest!.Score = _requestTOEFL!.Score;
        _updateSelectionTOEFLRequest!.Status = _requestTOEFL!.Status;
        _updateSelectionTOEFLRequest!.TestDate = _requestTOEFL.TestDate;
        _updateSelectionTOEFLRequest!.Expired = _requestTOEFL.Expired;
        _updateSelectionTOEFLRequest.DocumentId = DocumentId;
        _updateSelectionTOEFLRequest.ScholarshipPostingApplicantId = _requestTOEFL.ScholarshipPostingApplicantId;

        if (_updateSelectionTOEFLRequest.Score >= _toeflScore)
        {
            _updateSelectionTOEFLRequest.Status = ApplicantStatus.Passed;
        }
        else
        {
            _updateSelectionTOEFLRequest.Status = ApplicantStatus.Failed;
        }

        var updateTOEFL = await _selectionTOEFLService.UpdateSelectionToeflAsync(_updateSelectionTOEFLRequest);
        if (updateTOEFL.Error is not null)
        {
            var result = string.Empty;
            for (var i = 0; i < updateTOEFL.Error.Details.Count; i++)
            {
                result = result + updateTOEFL.Error.Details[i] + ",";
            }

            _snackbar.AddError("TOEFL - Error : " + result);
            _isLoadingDialog = false;
            StateHasChanged();
            return;
        }

        #endregion Update TOEFL

        #region Create selection overview

        var updateOverviewRequest = new UpdateSelectionOverviewByApplicantIdRequest
        {
            ApplicantId = updateTOEFL.Result!.ApplicantId,
            Stage = _updateSelectionTOEFLRequest.Status == ApplicantStatus.Passed ? SelectionStage.Psychotest : SelectionStage.TOEFL,
            Status = _updateSelectionTOEFLRequest.Status == ApplicantStatus.Passed ? ApplicantStatus.InProgress : ApplicantStatus.Failed,
        };

        await _selectionOverviewService.UpdateSelectionOverviewByApplicantIdAsync(updateOverviewRequest);

        #endregion Create selection overview

        #region Create selection psychotest

        if (getPsychotest is not null)
        {
            if (getPsychotest.Result!.Id == Guid.Empty)
            {
                if (_updateSelectionTOEFLRequest!.Status == ApplicantStatus.Passed)
                {
                    #region Create selection psychotest

                    await selection.CreateSelectionPsychotest(updateTOEFL.Result!.ApplicantId);

                    #endregion Create selection psychotest
                }
            }
        }

        #endregion Create selection psychotest

        VisiblePopup = false;
        _isLoadingDialog = false;
        _requestTOEFL = new UpdateSelectionTOEFLRequest();
        FileUploads = new FileUploadResponse();
        DocumentId = Guid.Empty;
        IsAuploadedDocument = false;
        _getListDocuments.Clear();
        _snackbar.AddSuccess("Success update data");
        await SetDataOnLoadAsync();
        StateHasChanged();
    }

    #endregion Popup Event Add/Edit

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
        if (DeleteType == "DeleteDocument")
        {
            var response = await _selectionTOEFLService.DeleteToeflCertificateAsync(DocumentId);
            if (response.Error is null)
            {
                _snackbar.AddSuccess("Success delete data");
                VisiblePopupDelete = false;
                VisiblePopup = false;
                _getListDocuments = new();
                await SetDataOnLoadAsync();
                StateHasChanged();
            }
        }
        else if (DeleteType == "DeleteToefl")
        {
            var response = await _selectionTOEFLService.DeleteSelectionToeflAsync(_requestTOEFL!.Id);
            if (response.Error is null)
            {
                _snackbar.AddSuccess("Success delete data");
                VisiblePopupDelete = false;
                await SetDataOnLoadAsync();
                StateHasChanged();
            }
        }
    }

    #endregion Popup Delete

    protected void OnClickSchedule(MouseEventArgs e)
    {
        if (_scholarshipPostingId == Guid.Empty || _scholarshipPostingId == null)
        {
            _snackbar.AddWarning("Please select code !");
            return;
        }

        // can add if exist applicant
        if (_selectionTOEFLs.Count() == 0)
        {
            _snackbar.AddWarning("Applicant not found!");
            return;
        }

        if (Type == 0)
        {
            _navigationManager.NavigateTo(RouteFor.Schedule(Type, (Guid)_scholarshipPostingId), true);
        }
        else
        {
            _navigationManager.NavigateTo(RouteFor.Schedule(Type, (Guid)_scholarshipPostingId), true);
        }
    }

    public async Task OnCommandClicked(CommandClickEventArgs<GetSelectionTOEFLQueryResponse> args)
    {
        var getToeflScore = await _scholarshipPostingService.GetScholarshipPostingByIdAsync(args.RowData.PostingId);
        _toeflScore = (int)getToeflScore.Result.TOEFLScore;

        _getListDocuments = new();
        if (args.CommandColumn.Type == CommandButtonType.Edit)
        {
            IsEnabled = true;
            VisiblePopup = true;
            TitlePopup = "Edit Applicant";
            _requestTOEFL.Id = args.RowData.Id;
            _requestTOEFL.ScholarshipPostingApplicantId = args.RowData.ApplicantId;
            _requestTOEFL.Name = args.RowData.Name;
            _requestTOEFL.CompanyCode = args.RowData.CompanyCode;
            _requestTOEFL.EmployeeNumber = args.RowData.EmployeeNumber;
            _requestTOEFL.Email = args.RowData.Email;
            _requestTOEFL.Score = args.RowData.Score;
            _requestTOEFL.TestDate = args.RowData.TestDate;
            _requestTOEFL.Expired = args.RowData.Expired;
            _requestTOEFL.Status = args.RowData.Status;

            var psycotestData = await _selectionPsychotestService.GetSelectionPsychotestByApplicantIdAsync(args.RowData.ApplicantId);
            if (psycotestData.Result!.Status == ApplicantStatus.InProgress)
            {
                IsEnabled = true;
            }
            else if (psycotestData.Result.Id == Guid.Empty)
            {
                IsEnabled = true;
            }
            else if (args.RowData.Status == ApplicantStatus.InProgress)
            {
                IsEnabled = true;
            }
            else if (args.RowData.Status == ApplicantStatus.Failed)
            {
                IsEnabled = true;
            }
            else
            {
                IsEnabled = false;
            }

            var skData = await _selectionDecreesService.GetSelectionDecreeByApplicantIdAsync(args.RowData.ApplicantId);

            if (skData.Result != null)
            {
                var spData = await _scholarshipPostingService.GetScholarshipPostingByIdAsync(args.RowData.PostingId);

                if (spData.Result != null)
                {
                    if (skData.Result.Status != ApplicantStatus.SKSend || spData.Result.Status != ScholarshipPostingStatus.Closed)
                    {
                        IsEnabledSKStatus = true;
                    }
                    else
                    {
                        IsEnabledSKStatus = false;
                    }
                }
            }

            if (args.RowData.Document is not null)
            {
                DocumentId = args.RowData.Document.Id;
                IsAuploadedDocument = true;
                _getListDocuments.Add(new GetDocumentsDocument
                {
                    Id = args.RowData.Document.Id,
                    FileName = args.RowData.Document.FileName,
                    DocumentType = DocumentType.CertificateTOEFL,
                });
            }
            else
            {
                IsAuploadedDocument = false;
            }
        }
        else if (args.CommandColumn.Type == CommandButtonType.Delete)
        {
            DeleteType = "DeleteToefl";
            VisiblePopupDelete = true;
            _requestTOEFL!.Id = args.RowData.Id;
        }
        else
        {
            IsEnabled = false;
            VisiblePopup = true;
            TitlePopup = "View Applicant";
            _requestTOEFL.Id = args.RowData.Id;
            _requestTOEFL.ScholarshipPostingApplicantId = args.RowData.ApplicantId;
            _requestTOEFL.Name = args.RowData.Name;
            _requestTOEFL.CompanyCode = args.RowData.CompanyCode;
            _requestTOEFL.EmployeeNumber = args.RowData.EmployeeNumber;
            _requestTOEFL.Email = args.RowData.Email;
            _requestTOEFL.Score = args.RowData.Score;
            _requestTOEFL.Status = args.RowData.Status;
            _requestTOEFL.TestDate = args.RowData.TestDate;
            _requestTOEFL.Expired = args.RowData.Expired;

            if (args.RowData.Document is not null)
            {
                DocumentId = args.RowData.Document.Id;
                IsAuploadedDocument = true;
                _getListDocuments.Add(new GetDocumentsDocument
                {
                    Id = args.RowData.Document.Id,
                    FileName = args.RowData.Document.FileName,
                    DocumentType = DocumentType.CertificateTOEFL,
                });
            }
            else
            {
                IsAuploadedDocument = false;
            }
        }
    }

    private async Task<MemoryStream> UploadToeflCertificate(string employeeNumber)
    {
        var memoryStream = new MemoryStream();
        await FileUploads.File.OpenReadStream(_maximumFileSizeInBytes).CopyToAsync(memoryStream);
        memoryStream.Position = 0; // Reset the stream position

        var formFile = new FormFile(memoryStream, 0, FileUploads.File.Size, FileUploads.File.Name, FileUploads.File.Name)
        {
            Headers = new HeaderDictionary(),
            ContentType = FileUploads.File.ContentType
        };

        var file = IBrowserFileExtensions.ToFormFile(FileUploads.File, memoryStream, FileUploads.File.Name);

        var requestFile = new CreateDocumentRequest
        {
            DocumentType = DocumentType.CertificateTOEFL,
            File = file,
            EmployeeNumber = employeeNumber,
        };

        var resultUpload = await _documentService.CreateDocumentAsync(requestFile);
        if (resultUpload.Error is null)
        {
            DocumentId = resultUpload.Result!.DocumentId;
            //_updateSelectionTOEFLRequest.DocumentId = resultUpload.Result!.DocumentId;
        }

        return memoryStream;
    }

    public async void OnCommandClickedDocument(CommandClickEventArgs<GetDocumentsDocument> args)
    {
        if (args.CommandColumn.Type == CommandButtonType.None)
        {
            if (args.CommandColumn.Title == "Download")
            {
                await OnClickDownload(args.RowData.Id);
            }
            else if (args.CommandColumn.Title == "Delete")
            {
                DocumentId = args.RowData.Id;
                VisiblePopupDelete = true;
                DeleteType = "DeleteDocument";
            }
        }
    }

    private async Task OnClickDownload(Guid documentId)
    {
        var request = new GetDocumentFileRequest
        {
            DocumentId = documentId,
            DocumentType = DocumentType.CertificateTOEFL,
        };

        var response = await _documentService.GetDocumentFileByDocumentTypeAsync(request);

        if (response.Error is not null)
        {
            var result = string.Empty;
            for (var i = 0; i < response.Error.Details.Count; i++)
            {
                result = result + response.Error.Details[i] + ",";
            }

            _snackbar.AddError(result);

            return;
        }

        if (response.Result is null)
        {
            _snackbar.AddError($"Failed to retrieve the document {CommonDisplayTextFor.File}.");

            return;
        }

        await _jsRuntime!.InvokeVoidAsync(
            JavaScriptIdentifierFor.DownloadFile,
            response.Result.FileName,
            response.Result.ContentType,
            response.Result.Content);
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
        else if (status == "In Progress")
        {
            result = "InProgress";
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
        else if (stage == "TOEFL")
        {
            result = "TOEFL";
        }
        else if (stage == "Psychotest")
        {
            result = "Psychotest";
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
    public void ExcelQueryCellInfoHandler(ExcelQueryCellInfoEventArgs<GetSelectionTOEFLQueryResponse> args)
    {
        var a = args.Column.Field;
        if (args.Column.Field == "Status")
        {
            args.Cell.Value = args.Data.Status.GetDescription().ToString();
        }
    }
}
