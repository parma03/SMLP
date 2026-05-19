using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using Psmp.Shared.Common.Responses;
using Psmp.Shared.Masters.Queries;
using Psmp.Shared.Scholarships.ScholarshipPosting.Queries;
using System.Security.Claims;
using Psmp.Shared.Selections.SelectionTalentReviews.Queries;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Psmp.Base.Selections.Enums;
using Psmp.Bsui.Common.Constants;
using Psmp.Bsui.Common.Extensions;
using Psmp.Shared.Selections.SelectionOverviews.Queries;
using Syncfusion.Blazor.Inputs;
using Syncfusion.XlsIO;
using System.Data;
using Psmp.Shared.Common.Constants;
using Psmp.Base.Scholarships.Enums;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Navigations;
using Psmp.Shared.Selections.SelectionTalentReviews.Commands.Create;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Psmp.Shared.Selections.SelectionTalentReviews.Commands.Update;
using Psmp.Shared.Documents.Queries.GetDocuments;
using Psmp.Shared.Common.Extensions;
using Psmp.Shared.Selections.SelectionPsychotests.Queries;
using RouteFor = Psmp.Bsui.Features.Selections.TalentReview.Constants.RouteFor;
using SelectionType = Psmp.Base.Selections.Enums.SelectionType;
using Psmp.Shared.Documents.Queries.GetDocumentFile;
using Psmp.Shared.Selections.SelectionOverviews.Commands.Update;
using Psmp.Shared.Scholarships.ScholarshipPosting.Commands.Update;
using Psmp.Shared.Selections.BeritaAcara.Commands.Create;
using Psmp.Shared.Selections.CreateSK.Queries;

namespace Psmp.Bsui.Features.Selections.TalentReview;

public partial class Index
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

    protected ClaimsPrincipal _user = default!;
    [Parameter]
    public int Type { get; set; }
    protected List<GetCompanyQueryResponse> _dropdownCompaniesAdd = new();
    protected List<GetSelectionTalentReviewQueryResponse> _selectionTalentReviews = new();
    private SfGrid<GetSelectionTalentReviewQueryResponse>? Grid { get; set; }
    public IEnumerable<ApplicantStatusResponse> _applicantStatusResponse = GetEnumDisplayNames<ApplicantStatus>();
    public CreateSelectionTalentReviewRequest _requestTalentReview2 = new();
    public GetSelectionTalentReviewQueryResponse _talentReviewResponse = new();
    public GetSelectionTalentReviewByIdQueryResponse _talentReviewEditResponse = new();
    public UpdateSelectionTalentReviewRequest _updateSelectionTalentReviewRequest = new();
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

    public IEnumerable<GetSelectionPsychotestQualificationStatusResponse> _getSelectionPsychotestQualificationStatusResponse = GetEnumDisplayNamesQualificationStatus<SelectionQualificationStatus>();

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
    protected int? _batch = null;
    protected int? _year = null;
    protected ScholarshipLocation _category = 0;
    protected int _countApplicant = 0;
    protected int _countApplicantPassed = 0;
    protected int _countApplicantRejected = 0;
    protected string _applicantSummary = "0";
    protected string _passedSummary = "0";
    protected string _inProgressSummary = "0";
    protected string _failedSummary = "0";
    protected string _canceledSummary = "0";
    protected long _maximumFileSizeInBytes = long.MaxValue;
    private bool VisiblePopupDelete { get; set; } = false;
    private bool VisiblePopupView { get; set; }
    private string TitlePopupView { get; set; } = "Applicant Status";
    public string? Filename { get; set; }
    public Guid? DocumentId { get; set; }
    protected bool _isLoadingDialog = false;
    protected SfUploader? _uploadObj;
    private bool VisiblePopupWarrant { get; set; }
    private string TitlePopupWarrant { get; set; } = "Scholarship Warrant";
    public FileUploadResponse FileUploadWarant { get; set; } = new();
    protected List<GetDocumentsDocument> _getListDocumentWarrant = new();
    public string? TypeDelete { get; set; }
    private bool VisiblePopupDecree { get; set; }
    private string TitlePopupDecree { get; set; } = "Scholarship Decree";
    public FileUploadResponse FileUploads { get; set; } = new();
    protected List<GetDocumentsDocument> _getListDocuments = new();
    protected UpdateScholarshipPostingStatusRequest UpdateDataScholarshipPostingStatus { get; set; } = new();
    protected override async Task OnInitializedAsync()
    {
        _user = (await AuthenticationStateTask).User;
        _isLoading = true;
        await GetDropdownCompanyGroup();
        await GetDropdownCompany();
        await GetDropdownCategories();
        //await GetDropdownBatchs();
        await GetDropdownCompanyGroupApplicantFilter();
        await GetDropdownCompanyApplicantFilter();
        await GetDropdownCodes();
        await SetDataOnLoadAsync();
        _isLoading = false;
    }
    private async Task SetDataOnLoadAsync()
    {
        _isLoadingData = true;
        var param = new GetSelectionTalentReviewRequest
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
        var response = await _selectionTalentReviewService.GetSelectionTalentReviewListAsync(param);

        if (response.Error is null)
        {
            _selectionTalentReviews = response.Result!.Items.ToList();
            _applicantSummary = _selectionTalentReviews.Count().ToString();
            _passedSummary = _selectionTalentReviews.Where(x => x.Status == ApplicantStatus.Passed).ToList().Count().ToString() + "/" + _applicantSummary;
            _inProgressSummary = _selectionTalentReviews.Where(x => x.Status == ApplicantStatus.InProgress).ToList().Count().ToString() + "/" + _applicantSummary;
            _failedSummary = _selectionTalentReviews.Where(x => x.Status == ApplicantStatus.Failed).ToList().Count().ToString() + "/" + _applicantSummary;
            _canceledSummary = _selectionTalentReviews.Where(x => x.Status == ApplicantStatus.Canceled).ToList().Count().ToString() + "/" + _applicantSummary;
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
    #endregion
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
    public async Task OnChangeYear(Syncfusion.Blazor.DropDowns.ChangeEventArgs<int?, DropdownItemResponse> args)
    {
        _year = args.Value;
        await SetDataOnLoadAsync();
        StateHasChanged();
    }
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
    public async Task OnChangeBatchAsync(Syncfusion.Blazor.DropDowns.ChangeEventArgs<int?, DropdownItemResponse> args)
    {
        if (args.ItemData == null)
        {
            _batch = 0;
        }
        else
        {
            _batch = Convert.ToInt32(args.ItemData.Id);
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
        await SetDataOnLoadAsync();
        StateHasChanged();
    }
    public async Task OnChangeDirectorateAsync(Syncfusion.Blazor.DropDowns.ChangeEventArgs<int, GetDirectorateQueryResponse> args)
    {
        _directorateId = args.ItemData.Id;

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
    protected static void OnRemoveUploaderApplicants(RemovingEventArgs args)
    {
        args.PostRawFile = false;
    }

    private async void OnClickUploadAsync(MouseEventArgs e)
    {
        if (_workbookApplicants.Count() == 0)
        {
            _snackbar.AddWarning("Please browse file");
            return;
        }

        _isLoadingDialog = true;
        if (_workbookApplicants.Count() > 0)
        {
            foreach (var item in _workbookApplicants.Where(x => x.Stage == SelectionStage.TalentReview))
            {
                var getSelectionPsychotest = await _selectionPsychotestService.GetSelectionPsychotestByApplicantIdAsync(item.ApplicantId);
                //Update selection Talent Review
                var request = new UpdateSelectionTalentReviewRequest
                {
                    Id = item.Id,
                    ApplicantId = item.ApplicantId,
                    Status = item.Status,
                };

                var response = await _selectionTalentReviewService.UpdateSelectionTalentReviewAsync(request);
                if (response.Error is not null)
                {
                    var result = string.Empty;
                    for (var i = 0; i < response.Error.Details.Count; i++)
                    {
                        result = result + response.Error.Details[i] + ",";
                    }

                    _snackbar.AddError("Talent Review - Error : " + result);
                    _isLoadingDialog = false;
                    StateHasChanged();
                    return;
                }
                //Update selection overview
                var updateOverviewRequest = new UpdateSelectionOverviewByApplicantIdRequest
                {
                    ApplicantId = item.ApplicantId,
                    Stage = SelectionStage.BeritaAcara,
                    Status = item.Status == ApplicantStatus.Passed ? ApplicantStatus.InProgressBA : ApplicantStatus.Failed,
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
                    StateHasChanged();
                    return;
                }

                if (item.Status == ApplicantStatus.InProgressBA)
                {
                    var decreeRequest = new CreateSelectionBeritaAcaraApplicantRequest
                    {
                        ScholarshipPostingApplicantId = item.ApplicantId,
                        Status = ApplicantStatus.InProgress
                    };

                    var createSelectionBeritaAcaraApolicant = await _selectionBeritaAcaraService.CreateSelectionBeritAcaraApplicantAsync(decreeRequest);
                    if (createSelectionBeritaAcaraApolicant.Error is not null)
                    {
                        var result = string.Empty;
                        for (var i = 0; i < createSelectionBeritaAcaraApolicant.Error.Details.Count; i++)
                        {
                            result = result + createSelectionBeritaAcaraApolicant.Error.Details[i] + ",";
                        }

                        _snackbar.AddError("Berita Acara Applicant - Error : " + result);
                        _isLoadingDialog = false;
                        StateHasChanged();
                        return;
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

    protected DataTable _table = new();
    protected List<GetSelectionOverviewQueryResponse> _workbookApplicants = new();
    protected void GenerateListFromTable(DataTable table)
    {
        if (table is not null)
        {
            foreach (DataRow row in table.Rows)
            {
                var status = GetStatusForUpload((string)row["Status"]);
                var qualifiedStatus = GetQualifiedStatusForUpload((string)row["QualificationStatus"]);
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
                        TOEFLScore = (string)row["TOEFLScore"],
                        QualificationStatus = (SelectionQualificationStatus)(int)Enum.Parse<SelectionQualificationStatus>(qualifiedStatus),
                        Stage = (SelectionStage)(int)Enum.Parse<SelectionStage>(stage),
                        Status = (ApplicantStatus)(int)Enum.Parse<ApplicantStatus>(status)
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
        table.Columns.Add("Bussines Issue", typeof(string));
        table.Columns.Add("Email", typeof(string));
        table.Columns.Add("Mentor 1", typeof(string));
        table.Columns.Add("Mentor 2", typeof(string));
        table.Columns.Add("TOEFLScore", typeof(string));
        table.Columns.Add("QualificationStatus", typeof(string));
        table.Columns.Add("Stage", typeof(string));
        table.Columns.Add("Status", typeof(string));

        if (_selectionTalentReviews is not null)
        {
            foreach (var item in _selectionTalentReviews)
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
                    item.BusinessIssue,
                    item.Email,
                    item.Mentor,
                    item.MentorName2,
                    item.TOEFLScore,
                    item.QualificationStatus.GetDescription(),
                    item.Stage,
                    item.Status.GetDescription());
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

        worksheet.Range["A1"].ColumnWidth = 40;
        worksheet.Range["B1"].ColumnWidth = 40;
        worksheet.Range["C1"].ColumnWidth = 40;
        worksheet.Range["D1"].ColumnWidth = 30;

        worksheet.Range["G1"].ColumnWidth = 40;
        worksheet.Range["H1"].ColumnWidth = 40;
        worksheet.Range["I1"].ColumnWidth = 40;
        worksheet.Range["J1"].ColumnWidth = 40;

        worksheet.Range["K1"].ColumnWidth = 40;
        worksheet.Range["L1"].ColumnWidth = 40;

        worksheet.Range["N1"].ColumnWidth = 25;
        worksheet.Range["O1"].ColumnWidth = 30;
        worksheet.Range["P1"].ColumnWidth = 30;

        var headingStyle = workbook.Styles.Add("HeadingStyle");
        headingStyle.Font.Bold = true;
        worksheet.Range["P1:P" + (_selectionTalentReviews.Count() + 1)].CellStyle = headingStyle;

        worksheet.Protect("sch0larship");
        worksheet.Range["P2:P" + (_selectionTalentReviews.Count() + 1)].CellStyle.Locked = false;

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

        IComment commentO = worksheet.Range["O1"].AddComment();
        commentO.Text = "Do not modify";
        commentO.IsVisible = false;

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
            $"SelectionTalentReview_{DateTime.Now:yyyyMMdd}.xlsx",
            $"{ContentTypes.ApplicationOpenXmlExcel}",
            content);
    }
    public async Task OnCommandClicked(CommandClickEventArgs<GetSelectionTalentReviewQueryResponse> args)
    {
        if (args.CommandColumn.Type == CommandButtonType.Edit)
        {
            _navigationManager.NavigateTo(RouteFor.Details(args.RowData.Id));
        }
        else if (args.CommandColumn.Type == CommandButtonType.None)
        {
            _talentReviewResponse!.Id = args.RowData.Id;
            _talentReviewResponse!.ApplicantId = args.RowData.ApplicantId;
            _talentReviewResponse!.Name = args.RowData.Name;
            _talentReviewResponse!.CompanyCode = args.RowData.CompanyCode;
            _talentReviewResponse!.EmployeeNumber = args.RowData.EmployeeNumber;
            _talentReviewResponse!.Email = args.RowData.Email;
            _talentReviewResponse!.Status = args.RowData.Status;
            _talentReviewResponse!.TOEFLScore = args.RowData.TOEFLScore;
            _talentReviewResponse!.QualificationStatus = args.RowData.QualificationStatus;
            _talentReviewResponse!.Mentor = args.RowData.Mentor;
            _talentReviewResponse!.InovationProject = args.RowData.InovationProject;

            var request = new GetSelectionDecreeDocumentRequest
            {
                ScholarshipPostingId = args.RowData.PostingId
            };

            var getDocumentSK = await _selectionDecreesService.GetSelectionDecreeDocumentAsync(request);
            if (getDocumentSK.Error is null)
            {
                var document = await _documentService.GetDocumentByIdAsync(getDocumentSK.Result!.DocumentIdDecree);
                if (document.Error is null)
                {
                    DocumentId = document.Result!.Id;
                    Filename = document.Result.FileName;
                }
                else
                {
                    DocumentId = Guid.Empty;
                    Filename = "";
                }
            }
            else
            {
                DocumentId = Guid.Empty;
                Filename = "";
            }

            await Task.Delay(100);
            VisiblePopupView = true;
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
        if (TypeDelete == "decree")
        {
            var response = await _selectionDecreesService.DeleteSelectionDecreeLetterDocumentAsync((Guid)DocumentId!);
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

            _getListDocuments = new();
            VisiblePopupDecree = false;
        }
        else if (TypeDelete == "warrant")
        {
            var response = await _selectionDecreesService.DeleteSelectionDecreeWarrantDocumentAsync((Guid)DocumentId!);
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

            _getListDocumentWarrant = new();
            VisiblePopupWarrant = false;
        }
        else
        {
            var response = await _selectionTalentReviewService.DeleteSelectionTalentReviewAsync(_talentReviewResponse!.Id);
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
        }

        _snackbar.AddSuccess("Success delete data");
        VisiblePopupDelete = false;
        await SetDataOnLoadAsync();
        StateHasChanged();
    }
    #endregion
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
                   }).ToList();
        //}).Where(x => x.Value is ApplicantStatus.Passed || x.Value == ApplicantStatus.Failed || x.Value == ApplicantStatus.Rejected || x.Value == ApplicantStatus.Awaiting).ToList();
    }
    public static List<GetSelectionPsychotestQualificationStatusResponse> GetEnumDisplayNamesQualificationStatus<T>()
    {
        var type = typeof(T);
        return Enum.GetValues(type)
                   .Cast<T>()
                   .Select(x => new GetSelectionPsychotestQualificationStatusResponse
                   {
                       Value = x as SelectionQualificationStatus?,
                       Text = type.GetMember(x!.ToString()!)
                   .First()
                   .GetCustomAttribute<DisplayAttribute>()?.Name ?? x.ToString()

                   }).ToList();
    }
    #region Popup View Event

    private void DialogViewOpen()
    {
        VisiblePopupView = true;
    }

    private void DialogViewClose()
    {
        VisiblePopupView = false;
        _talentReviewResponse = new GetSelectionTalentReviewQueryResponse();
    }

    private void ClickedNo()
    {
        VisiblePopupView = false;
    }
    #endregion Popup Event

    private async Task OnClickDownloadDocument()
    {
        var request = new GetDocumentFileRequest
        {
            DocumentId = (Guid)DocumentId!,
            DocumentType = DocumentType.ScholarshipDecree,
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
    private string GetQualifiedStatusForUpload(string qualificationStatus)
    {
        var result = string.Empty;
        if (qualificationStatus == "Awaiting")
        {
            result = "Awaiting";
        }
        else if (qualificationStatus == "Not Qualified")
        {
            result = "NotQualified";
        }
        else if (qualificationStatus == "Qualified")
        {
            result = "Qualified";
        }
        else if (qualificationStatus == "Qualified Plus")
        {
            result = "QualifiedPlus";
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
    public void ExcelQueryCellInfoHandler(ExcelQueryCellInfoEventArgs<GetSelectionTalentReviewQueryResponse> args)
    {
        var a = args.Column.Field;
        if (args.Column.Field == "Status")
        {
            args.Cell.Value = args.Data.Status.GetDescription();
        }
    }
    //#region Create SP
    //private async Task OnClickCreateWarrant()
    //{
    //    if (_scholarshipPostingId == Guid.Empty)
    //    {
    //        _snackbar.AddWarning("Please select code !");
    //        return;
    //    }

    //    if (_selectionTalentReviews.Where(x => x.PostingId == _scholarshipPostingId && (x.Status == ApplicantStatus.InProgressSK || x.Status == ApplicantStatus.SKCreated || x.Status == ApplicantStatus.SKSend)).ToList().Count() == 0)
    //    {
    //        _snackbar.AddWarning("Applicant cannot complete !");
    //        return;
    //    }

    //    if (_selectionTalentReviews.Count() > 0)
    //    {
    //        var getSelectionTalentReview = _selectionTalentReviews.Where(x => x.PostingId == _scholarshipPostingId).FirstOrDefault();
    //        if (getSelectionTalentReview is not null)
    //        {
    //            if (getSelectionTalentReview!.DocumentIdWarrant is not null)
    //            {
    //                var document = await _documentService.GetDocumentByIdAsync(getSelectionTalentReview.DocumentIdWarrant);
    //                if (document is not null)
    //                {
    //                    DocumentId = document.Result!.Id;
    //                    _getListDocumentWarrant.Add(new GetDocumentsDocument
    //                    {
    //                        Id = document.Result.Id,
    //                        FileName = document.Result.FileName,
    //                        DocumentType = DocumentType.ScholarshipWarrant,
    //                    });
    //                }
    //            }
    //        }

    //        await Task.Delay(100);
    //        VisiblePopupWarrant = true;
    //    }
    //    else
    //    {
    //        _snackbar.AddWarning("Data is empty !");
    //    }
    //}
    //private void DialogWarrantOpen()
    //{
    //    VisiblePopupWarrant = true;
    //}
    //private void DialogWarrantClose()
    //{
    //    VisiblePopupWarrant = false;
    //    _getListDocumentWarrant = new();
    //    FileUploadWarant = new();
    //}
    //private void OnClickNoDialogWarrant()
    //{
    //    VisiblePopupWarrant = false;
    //    _getListDocumentWarrant = new();
    //    FileUploadWarant = new();
    //}
    //private async void OnClickYesDialogWarrant()
    //{
    //    _isLoadingDialog = true;
    //    if (FileUploadWarant.FileName is null)
    //    {
    //        _snackbar.AddWarning("Please browse file !");
    //        _isLoadingDialog = false;
    //        StateHasChanged();
    //        return;
    //    }

    //    await UploadDocumentSholarshipWarrant();

    //    if (DocumentId == Guid.Empty)
    //    {
    //        _snackbar.AddError("Error upload scholarship decree !");
    //        _isLoadingDialog = false;
    //        StateHasChanged();
    //        return;
    //    }

    //    if (_getListDocumentWarrant.Count() > 0)
    //    {
    //        _snackbar.AddWarning("Please delete document before re-upload !");
    //        _isLoadingDialog = false;
    //        StateHasChanged();
    //        return;
    //    }

    //    var decreeDocumentRequest = new CreateScholarshipDecreeRequest
    //    {
    //        Type = "warrant",
    //        ScholarshipPostingId = _scholarshipPostingId,
    //        DocumentId = (Guid)DocumentId!,
    //    };

    //    var createDecreeLetter = await _selectionDecreesService.CreateSelectionDecreeLetterDocumentAsync(decreeDocumentRequest);
    //    if (createDecreeLetter.Error is not null)
    //    {
    //        var result = string.Empty;
    //        for (var i = 0; i < createDecreeLetter.Error.Details.Count; i++)
    //        {
    //            result = result + createDecreeLetter.Error.Details[i] + ",";
    //        }

    //        _snackbar.AddError("Create SP - Error : " + result);
    //        _isLoadingDialog = false;
    //        StateHasChanged();
    //        return;
    //    }

    //    _snackbar.AddSuccess("Success upload scholarship warrant");
    //    await SetDataOnLoadAsync();
    //    _isLoadingDialog = false;
    //    VisiblePopupWarrant = false;
    //    FileUploadWarant = new();
    //    _getListDocumentWarrant = new();
    //    StateHasChanged();

    //}
    //private async Task<MemoryStream> UploadDocumentSholarshipWarrant()
    //{
    //    var memoryStream = new MemoryStream();
    //    await FileUploadWarant.File.OpenReadStream(_maximumFileSizeInBytes).CopyToAsync(memoryStream);
    //    memoryStream.Position = 0; // Reset the stream position

    //    var formFile = new FormFile(memoryStream, 0, FileUploadWarant.File.Size, FileUploadWarant.File.Name, FileUploadWarant.File.Name)
    //    {
    //        Headers = new HeaderDictionary(),
    //        ContentType = FileUploadWarant.File.ContentType
    //    };

    //    var file = IBrowserFileExtensions.ToFormFile(FileUploadWarant.File, memoryStream, FileUploadWarant.File.Name);

    //    var requestFile = new CreateDocumentRequest
    //    {
    //        DocumentType = DocumentType.ScholarshipWarrant,
    //        File = file,
    //    };

    //    var resultUpload = await _documentService.CreateDocumentAsync(requestFile);
    //    if (resultUpload.Error is null)
    //    {
    //        DocumentId = resultUpload.Result!.DocumentId;
    //    }

    //    return memoryStream;
    //}

    //protected void OnRemoveUploaderWarrant(RemovingEventArgs args)
    //{
    //    FileUploadWarant = new();
    //    _getListDocumentWarrant = new();
    //    args.PostRawFile = false;
    //}

    //protected void ValueChangeUploaderWarrant(UploadChangeEventArgs args)
    //{
    //    FileUploadWarant.FileName = args.Files.First().File.Name;
    //    FileUploadWarant.FileSize = args.Files.First().File.Size;
    //    FileUploadWarant.FileContentType = args.Files.First().File.ContentType;
    //    FileUploadWarant.File = args.Files.First().File;
    //}

    //public async void OnCommandClickedDocumentWarrant(CommandClickEventArgs<GetDocumentsDocument> args)
    //{
    //    if (args.CommandColumn.Type == CommandButtonType.None)
    //    {
    //        if (args.CommandColumn.Title == "Download")
    //        {
    //            await OnClickDownload(args.RowData.Id, args.RowData.DocumentType);
    //        }
    //        else if (args.CommandColumn.Title == "Delete")
    //        {
    //            TypeDelete = "warrant";
    //            DocumentId = args.RowData.Id;
    //            VisiblePopupDelete = true;
    //        }

    //    }
    //}

    //private async Task OnClickDownload(Guid documentId, DocumentType documentType)
    //{
    //    var request = new GetDocumentFileRequest
    //    {
    //        DocumentId = documentId,
    //        DocumentType = documentType,
    //    };

    //    var response = await _documentService.GetDocumentFileByDocumentTypeAsync(request);

    //    if (response.Error is not null)
    //    {
    //        var result = string.Empty;
    //        for (var i = 0; i < response.Error.Details.Count; i++)
    //        {
    //            result = result + response.Error.Details[i] + ",";
    //        }

    //        _snackbar.AddError(result);

    //        return;
    //    }

    //    if (response.Result is null)
    //    {
    //        _snackbar.AddError($"Failed to retrieve the document {CommonDisplayTextFor.File}.");

    //        return;
    //    }

    //    await _jsRuntime!.InvokeVoidAsync(
    //        JavaScriptIdentifierFor.DownloadFile,
    //        response.Result.FileName,
    //        response.Result.ContentType,
    //        response.Result.Content);
    //}
    //#endregion
    //#region Create SK
    //private async Task OnClickCreateSK()
    //{
    //    if (_scholarshipPostingId == Guid.Empty)
    //    {
    //        _snackbar.AddWarning("Please select code !");
    //        return;
    //    }

    //    if (_selectionTalentReviews.Where(x => x.PostingId == _scholarshipPostingId && x.DocumentIdWarrant is null).ToList().Count() > 0)
    //    {
    //        _snackbar.AddWarning("Please upload Surat Perintah !");
    //        return;
    //    }

    //    if (_selectionTalentReviews.Where(x => x.PostingId == _scholarshipPostingId && (x.Status == ApplicantStatus.InProgressSK || x.Status == ApplicantStatus.SKCreated || x.Status == ApplicantStatus.SKSend)).ToList().Count() == 0)
    //    {
    //        _snackbar.AddWarning("Applicant cannot complete !");
    //        return;
    //    }

    //    if (_selectionTalentReviews.Count() > 0)
    //    {
    //        var selectionTalentReview = _selectionTalentReviews.Where(x => x.PostingId == _scholarshipPostingId).FirstOrDefault();
    //        if (selectionTalentReview is not null)
    //        {
    //            if (selectionTalentReview!.DocumentIdDecree is not null)
    //            {
    //                var document = await _documentService.GetDocumentByIdAsync(selectionTalentReview.DocumentIdDecree);
    //                if (document is not null)
    //                {
    //                    DocumentId = document.Result!.Id;
    //                    _getListDocuments.Add(new GetDocumentsDocument
    //                    {
    //                        Id = document.Result.Id,
    //                        FileName = document.Result.FileName,
    //                        DocumentType = DocumentType.ScholarshipDecree,
    //                    });
    //                }
    //            }
    //        }

    //        await Task.Delay(100);
    //        VisiblePopupDecree = true;
    //    }
    //    else
    //    {
    //        _snackbar.AddWarning("Data is empty !");
    //    }
    //}
    //private void DialogDecreeOpen()
    //{
    //    VisiblePopupDecree = true;
    //}

    //private void DialogDecreeClose()
    //{
    //    VisiblePopupDecree = false;
    //    _getListDocuments = new();
    //    FileUploads = new();
    //}
    //private void OnClickNoDialogDecree()
    //{
    //    VisiblePopupDecree = false;
    //    _getListDocuments = new();
    //    FileUploads = new();
    //}
    //private async void OnClickYesDialogDecree()
    //{
    //    _isLoadingDialog = true;
    //    if (FileUploads.FileName is null)
    //    {
    //        _snackbar.AddWarning("Please browse file !");
    //        _isLoadingDialog = false;
    //        StateHasChanged();
    //        return;
    //    }

    //    await UploadDocumentSholarshipDecree();

    //    if (DocumentId == Guid.Empty)
    //    {
    //        _snackbar.AddError("Error upload scholarship decree !");
    //        _isLoadingDialog = false;
    //        StateHasChanged();
    //        return;
    //    }

    //    if (_getListDocuments.Count() > 0)
    //    {
    //        _snackbar.AddWarning("Please delete document before re-upload !");
    //        _isLoadingDialog = false;
    //        StateHasChanged();
    //        return;
    //    }

    //    var decreeDocumentRequest = new CreateScholarshipDecreeRequest
    //    {
    //        Type = "decree",
    //        ScholarshipPostingId = _scholarshipPostingId,
    //        DocumentId = (Guid)DocumentId!,
    //    };

    //    var createDecreeLetter = await _selectionDecreesService.CreateSelectionDecreeLetterDocumentAsync(decreeDocumentRequest);
    //    if (createDecreeLetter.Error is not null)
    //    {
    //        var result = string.Empty;
    //        for (var i = 0; i < createDecreeLetter.Error.Details.Count; i++)
    //        {
    //            result = result + createDecreeLetter.Error.Details[i] + ",";
    //        }

    //        _snackbar.AddError(result);
    //        _isLoadingDialog = false;
    //        StateHasChanged();
    //        return;
    //    }

    //    if (_selectionTalentReviews.Where(x => x.Status == ApplicantStatus.InProgressSK).ToList().Count() > 0)
    //    {
    //        var data = _selectionTalentReviews.Where(x => x.Status == ApplicantStatus.InProgressSK).ToList();
    //        foreach (var item in data)
    //        {
    //            //Update talent review
    //            var request = new UpdateSelectionTalentReviewRequest
    //            {
    //                Id = item.Id,
    //                ApplicantId = item.ApplicantId,
    //                Status = ApplicantStatus.SKCreated,
    //            };
    //            var updateTalentReview = await _selectionTalentReviewService.UpdateSelectionTalentReviewAsync(request);
    //            if (updateTalentReview.Error is not null)
    //            {
    //                var result = string.Empty;
    //                for (var i = 0; i < updateTalentReview.Error.Details.Count; i++)
    //                {
    //                    result = result + updateTalentReview.Error.Details[i] + ",";
    //                }

    //                _snackbar.AddError("Talent Review - Error : " + result);
    //                _isLoadingDialog = false;
    //                StateHasChanged();
    //                return;
    //            }
    //            //Update selection overview
    //            var updateOverviewRequest = new UpdateSelectionOverviewByApplicantIdRequest
    //            {
    //                ApplicantId = item.ApplicantId,
    //                Stage = SelectionStage.TalentReview,
    //                Status = ApplicantStatus.SKCreated,
    //            };

    //            var updateOverview = await _selectionOverviewService.UpdateSelectionOverviewByApplicantIdAsync(updateOverviewRequest);
    //            if (updateOverview.Error is not null)
    //            {
    //                var result = string.Empty;
    //                for (var i = 0; i < updateOverview.Error.Details.Count; i++)
    //                {
    //                    result = result + updateOverview.Error.Details[i] + ",";
    //                }

    //                _snackbar.AddError("Overview - Error : " + result);
    //                _isLoadingDialog = false;
    //                StateHasChanged();
    //                return;
    //            }

    //            //var scholarshipPosting = await _scholarshipPostingService.GetScholarshipPostingByIdAsync(_scholarshipPostingId);
    //            //if (scholarshipPosting.Error is not null)
    //            //{
    //            //    var result = string.Empty;
    //            //    for (var i = 0; i < scholarshipPosting.Error.Details.Count; i++)
    //            //    {
    //            //        result = result + scholarshipPosting.Error.Details[i] + ",";
    //            //    }

    //            //    _snackbar.AddError("Get ScholarshipPosting - Error : " + result);
    //            //    _isLoadingDialog = false;
    //            //    StateHasChanged();
    //            //    return;
    //            //}

    //            //var dataScholarshipPosting = scholarshipPosting.Result;

    //            //var postingStatus = new ScholarshipPostingStatus();
    //            //var awardeeQuota = scholarshipPosting is not null ? scholarshipPosting.Result!.AwardeeQuota : 0;
    //            //var awardeeSelection = _selectionTalentReviews.Where(x => x.PostingId == _scholarshipPostingId && x.Status == ApplicantStatus.SKCreated).ToList().Count();

    //            //if (awardeeSelection >= awardeeQuota)
    //            //{
    //            //    postingStatus = ScholarshipPostingStatus.Completed;
    //            //}
    //            //else
    //            //{
    //            //    postingStatus = ScholarshipPostingStatus.Closed;
    //            //}

    //            //UpdateDataScholarshipPostingStatus.PostingId = _scholarshipPostingId;
    //            //UpdateDataScholarshipPostingStatus.PostingStatus = postingStatus;
    //            //var updatePostingStatus = await _scholarshipPostingService.UpdateScholarshipPostingStatusAsync(UpdateDataScholarshipPostingStatus);
    //            //if (updatePostingStatus.Error is not null)
    //            //{
    //            //    var result = string.Empty;
    //            //    for (var i = 0; i < updatePostingStatus.Error.Details.Count; i++)
    //            //    {
    //            //        result = result + updatePostingStatus.Error.Details[i] + ",";
    //            //    }

    //            //    _snackbar.AddError("Set ScholarshipPosting Status - Error : " + result);
    //            //    _isLoadingDialog = false;
    //            //    StateHasChanged();
    //            //    return;
    //            //}
    //        }
    //    }

    //    _snackbar.AddSuccess("Success upload scholarship decree");
    //    await SetDataOnLoadAsync();
    //    _isLoadingDialog = false;
    //    VisiblePopupDecree = false;
    //    FileUploads = new();
    //    _getListDocuments = new();
    //    StateHasChanged();

    //}
    //protected void OnRemoveUploaderDecree(RemovingEventArgs args)
    //{
    //    FileUploads = new();
    //    _getListDocuments = new();
    //    args.PostRawFile = false;
    //}
    //protected void ValueChangeUploaderDecree(UploadChangeEventArgs args)
    //{
    //    FileUploads.FileName = args.Files.First().File.Name;
    //    FileUploads.FileSize = args.Files.First().File.Size;
    //    FileUploads.FileContentType = args.Files.First().File.ContentType;
    //    FileUploads.File = args.Files.First().File;
    //}
    //public async void OnCommandClickedDocument(CommandClickEventArgs<GetDocumentsDocument> args)
    //{
    //    if (args.CommandColumn.Type == CommandButtonType.None)
    //    {
    //        if (args.CommandColumn.Title == "Download")
    //        {
    //            await OnClickDownload(args.RowData.Id, args.RowData.DocumentType);
    //        }
    //        else if (args.CommandColumn.Title == "Delete")
    //        {
    //            TypeDelete = "decree";
    //            DocumentId = args.RowData.Id;
    //            VisiblePopupDelete = true;
    //        }

    //    }
    //}
    //private async Task<MemoryStream> UploadDocumentSholarshipDecree()
    //{
    //    var memoryStream = new MemoryStream();
    //    await FileUploads.File.OpenReadStream(_maximumFileSizeInBytes).CopyToAsync(memoryStream);
    //    memoryStream.Position = 0; // Reset the stream position

    //    var formFile = new FormFile(memoryStream, 0, FileUploads.File.Size, FileUploads.File.Name, FileUploads.File.Name)
    //    {
    //        Headers = new HeaderDictionary(),
    //        ContentType = FileUploads.File.ContentType
    //    };

    //    var file = IBrowserFileExtensions.ToFormFile(FileUploads.File, memoryStream, FileUploads.File.Name);

    //    var requestFile = new CreateDocumentRequest
    //    {
    //        DocumentType = DocumentType.ScholarshipDecree,
    //        File = file,
    //    };

    //    var resultUpload = await _documentService.CreateDocumentAsync(requestFile);
    //    if (resultUpload.Error is null)
    //    {
    //        DocumentId = resultUpload.Result!.DocumentId;
    //    }

    //    return memoryStream;
    //}
    //#endregion
    //#region Send SK
    //private async void OnClickSendSK()
    //{
    //    _isLoadingDialog = true;
    //    if (_scholarshipPostingId == Guid.Empty)
    //    {
    //        _snackbar.AddWarning("Please select code !");
    //        _isLoadingDialog = false;
    //        StateHasChanged();
    //        return;
    //    }

    //    var selectionDecree = _selectionTalentReviews.Where(x => x.PostingId == _scholarshipPostingId).FirstOrDefault();
    //    if (selectionDecree!.DocumentIdDecree is null || selectionDecree.DocumentIdWarrant is null)
    //    {
    //        _snackbar.AddWarning("Please upload Surat Perintah & Surat Keputusan !");
    //        _isLoadingDialog = false;
    //        StateHasChanged();
    //        return;
    //    }

    //    var sendEmailRequest = new SendEmailRequest
    //    {
    //        PostingId = _scholarshipPostingId,
    //    };

    //    var sendEmail = await _emailService.SendMailAsync(sendEmailRequest);

    //    if (sendEmail.Result!.Items.FirstOrDefault()!.Type == "S")
    //    {
    //        if (_selectionTalentReviews.Where(x => x.Status is ApplicantStatus.SKCreated || x.Status == ApplicantStatus.SKSend).ToList().Count() > 0)
    //        {
    //            var data = _selectionTalentReviews.Where(x => x.Status is ApplicantStatus.SKCreated || x.Status == ApplicantStatus.SKSend).ToList();
    //            foreach (var item in data)
    //            {
    //                //Update talent review
    //                var request = new UpdateSelectionTalentReviewRequest
    //                {
    //                    Id = item.Id,
    //                    ApplicantId = item.ApplicantId,
    //                    Status = ApplicantStatus.SKSend,
    //                };
    //                var uodateTalentReview = await _selectionTalentReviewService.UpdateSelectionTalentReviewAsync(request);
    //                if (uodateTalentReview.Error is not null)
    //                {
    //                    var result = string.Empty;
    //                    for (var i = 0; i < uodateTalentReview.Error.Details.Count; i++)
    //                    {
    //                        result = result + uodateTalentReview.Error.Details[i] + ",";
    //                    }

    //                    _snackbar.AddError("Talent Review - Error : " + result);
    //                    _isLoadingDialog = false;
    //                    StateHasChanged();
    //                    return;
    //                }
    //                //Update selection overview
    //                var updateOverviewRequest = new UpdateSelectionOverviewByApplicantIdRequest
    //                {
    //                    ApplicantId = item.ApplicantId,
    //                    Stage = SelectionStage.TalentReview,
    //                    Status = ApplicantStatus.SKSend,
    //                };

    //                var updateOverview = await _selectionOverviewService.UpdateSelectionOverviewByApplicantIdAsync(updateOverviewRequest);
    //                if (updateOverview.Error is not null)
    //                {
    //                    var result = string.Empty;
    //                    for (var i = 0; i < updateOverview.Error.Details.Count; i++)
    //                    {
    //                        result = result + updateOverview.Error.Details[i] + ",";
    //                    }

    //                    _snackbar.AddError("Overview - Error : " + result);
    //                    _isLoadingDialog = false;
    //                    StateHasChanged();
    //                    return;
    //                }
    //            }
    //        }
    //    }

    //    _snackbar.AddSuccess(sendEmail.Result!.Items.FirstOrDefault()!.Message!);
    //    _isLoadingDialog = false;
    //    StateHasChanged();
    //}
    //#endregion
}
