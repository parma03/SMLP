using System.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Psmp.Base.Scholarships.Enums;
using Psmp.Base.Selections.Enums;
using Psmp.Base.Universities.Enums;
using Psmp.Bsui.Common.Constants;
using Psmp.Bsui.Common.Extensions;
using Psmp.Shared.Common.Constants;
using Psmp.Shared.Common.Extensions;
using Psmp.Shared.Common.Responses;
using Psmp.Shared.Documents.Commands.CreateDocument;
using Psmp.Shared.Masters.Queries;
using Psmp.Shared.Scholarships.ScholarshipPosting.Queries;
using Psmp.Shared.Universities.Admission.Queries;
using Psmp.Shared.Universities.AdmissionSuperiorLetter.Commands.Create;
using Psmp.Shared.Universities.UniversityRecommendation.Queries;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Inputs;
using Syncfusion.XlsIO;
using Syncfusion.Blazor.Navigations;

namespace Psmp.Bsui.Features.Universities.Admission;

public partial class Index
{
    #region Protected Properties
    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;
    protected ClaimsPrincipal _user = default!;

    protected ErrorResponse? _error;
    protected bool _isLoading = false;
    protected bool _isLoadingDialog = false;
    protected bool _isLoadingData = false;
    protected List<GetAdmissionQueryResponse> _dataGrid = new();
    private SfGrid<GetAdmissionQueryResponse> Grid { get; set; } = new();
    protected long _maximumFileSizeInBytes = long.MaxValue;
    protected DataTable _tableExcel = new();
    protected List<GetAdmissionQueryResponse> _workbookExcel = new();
    protected SfUploader? _uploadExcel;
    #endregion

    #region Modal Properties
    private string? ActionType { get; set; }
    private bool IsEnabled { get; set; }
    private bool VisiblePopup { get; set; }
    private string TitlePopup { get; set; } = "Add";

    private bool IsEnableDocumentLoA { get; set; } = false;
    private bool IsEnableDocumentSuperiorApproval { get; set; } = false;
    private string? DocumentPathLoA { get; set; }
    private string? DocumentPathSuperiorApproval { get; set; }
    public List<GetUniversityRecommendationQueryResponse> _dropdownUnivesity = new();
    public List<DropdownItemResponse> _dropdownStudyTime = new();
    protected CreateAdmissionSuperiorLetterRequest _createAdmissionSuperiorLetterRequest = new();
    public Guid DocumentIdLoA { get; set; } = new();
    public FileUploadResponse FileUploadLoA { get; set; } = new();
    public Guid DocumentIdSuperiorApproval { get; set; } = new();
    public FileUploadResponse FileUploadSuperiorApproval { get; set; } = new();
    #endregion

    #region Card Properties
    protected int CountAll { get; set; } = 0;
    protected int CountCompleted { get; set; } = 0;
    protected int CountSubmitted { get; set; } = 0;
    protected int CountWaitingForApproval { get; set; } = 0;
    #endregion

    #region Dropdown Properties
    protected UniversityCategory _category = 0;
    protected int _companyGroupId = 0;
    protected string _companyCode = "0000";
    protected int _directorateId = 0;
    protected string _scholarshipCodeId = string.Empty;
    protected ScholarshipBatch _batch = 0;

    public List<DropdownEnumResponse<UniversityCategory>> _dropdownEnumCategories = new();
    public List<GetCompanyGroupQueryResponse> _dropdownCompanyGroups = new();
    public List<GetCompanyQueryResponse> _dropdownCompanies = new();
    public List<GetDirectorateQueryResponse> _dropdownDirectorates = new();
    protected List<GetScholarshipPostingDropdownResponse> _dropdownScholarshipCodes = new();
    public List<DropdownEnumResponse<ScholarshipBatch>> _dropdownEnumBatchs = new();
    #endregion

    public List<DropdownItemResponse> _dropdownStatus = new();
    public List<ApplicantDropdown> _listApplicant = new();

    protected override async Task OnInitializedAsync()
    {
        _user = (await AuthenticationStateTask).User;
        _isLoading = true;
        _companyCode = _user.GetCompanyCode();
        await SetDataOnLoadAsync();
        await OnLoadUniversity();
        await OnLoadStudyTime();
        await OnLoadApplicant();

        await GetDropdownCategories();
        await GetDropdownBatchs();
        await GetDropdownCompanyGroup();
        await GetDropdownCompany();
        await GetDropdownDirectorate();
        await GetDropdownScholarshipCodes();

        await GetDropdownStatus();
        _isLoading = false;
    }

    #region DataGrid
    private async Task SetDataOnLoadAsync()
    {
        _isLoadingData = true;
        var data = await _universityAdmissionService.GetUniversityAdmissionAsync();
        if (data.Error is null)
        {
            _dataGrid = data.Result!.Items.ToList();

            if (_category != 0)
            {
                _dataGrid = _dataGrid.Where(x => x.UniversityCategory == _category).ToList();
            }

            if (_companyGroupId != 0)
            {
                _dataGrid = _dataGrid.Where(x => x.CompanyGroupId == _companyGroupId).ToList();
            }

            if ((string.IsNullOrEmpty(_companyCode) ? "0000" : _companyCode) != "0000")
            {
                _dataGrid = _dataGrid.Where(x => x.CompanyCode == _companyCode).ToList();
            }

            if (_directorateId != 0)
            {
                _dataGrid = _dataGrid.Where(x => x.DirectorateId == _directorateId).ToList();
            }

            if (_scholarshipCodeId != "")
            {
                _dataGrid = _dataGrid.Where(x => x.Code == _scholarshipCodeId).ToList();
            }

            if (_batch != 0)
            {
                _dataGrid = _dataGrid.Where(x => x.Batch == _batch).ToList();
            }

            CountAll = _dataGrid.Count;
            CountCompleted = _dataGrid.Where(x => x.Status == AdmissionStatus.Completed).Count();
            CountSubmitted = _dataGrid.Where(x => x.Status == AdmissionStatus.Submitted).Count();
            CountWaitingForApproval = _dataGrid.Where(x => x.Status == AdmissionStatus.WaitingApproval).Count();
        }
        else
        {
            _error = data.Error;
        }

        _isLoading = false;
    }
    #endregion

    #region Dropdown
    private async Task OnLoadUniversity()
    {
        var datas = await _universityRecommendationService.GetUniversityRecommendationAsync();
        if (datas.Error is null)
        {
            _dropdownUnivesity = datas.Result!.Items.ToList();
            _dropdownUnivesity = _dropdownUnivesity.OrderBy(x => x.University).ToList();
        }

        await Task.CompletedTask;
    }
    private async Task OnLoadStudyTime()
    {
        _dropdownStudyTime = new List<DropdownItemResponse>()
        {
            new() {Id = 1, Description = "Full Time"},
            new() {Id = 2, Description = "Part Time"}
        };
        await Task.CompletedTask;
    }
    private async Task OnLoadApplicant()
    {
        _listApplicant = new List<ApplicantDropdown>()
        {
            new() {Id = new Guid("4E16DA60-1BA0-45E8-89B7-08DD04511A63"), Name = "Padma Viona Leony" },
            new() {Id = new Guid("A4C88D93-601F-4FF8-129E-08DD0A4A0E64"), Name = "Kamandaka Dierja Agastya" },
            new() {Id = new Guid("A7548F1A-9690-4163-F0D8-08DD0E238EC4"), Name = "Rinjani Intan Lisa" },
            new() {Id = new Guid("03A60320-940A-46E7-94DB-7FD04C2028E2"), Name = "Saharsa Chandika Hanenda" },
            new() {Id = new Guid("4E16DA30-1BA0-45E8-89B7-08DD04511A63"), Name = "Sample Local" },
        };

        await Task.CompletedTask;
    }

    private async Task GetDropdownCategories()
    {
        _dropdownEnumCategories = EnumExtentions.ToDropdownEnum<UniversityCategory>();
        await Task.CompletedTask;
    }
    private async Task GetDropdownBatchs()
    {
        _dropdownEnumBatchs = EnumExtentions.ToDropdownEnum<ScholarshipBatch>();
        await Task.CompletedTask;
    }
    private async Task GetDropdownCompanyGroup()
    {
        var data = await _companyGroupService.GetCompanyGroupsAsync();
        if (data.Error is null)
        {
            _dropdownCompanyGroups = data.Result!.Items.Where(x => _companyCode == "1010" ? true : x.CompanyCode == _companyCode).OrderBy(x => x.Id).ToList();
            _companyGroupId = _dropdownCompanyGroups.FirstOrDefault()!.Id;
        }
    }
    private async Task GetDropdownCompany()
    {
        var data = await _companyService.GetCompanyByCompanyGroupIdAsync(_companyGroupId);
        if (data.Error is null)
        {
            _dropdownCompanies = data.Result!.Items.ToList();
            _dropdownCompanies = _dropdownCompanies.OrderBy(x => x.Code).ToList();
        }
    }
    private async Task GetDropdownDirectorate()
    {
        var data = await _directorateService.GetDirectotaeByCompanyCodeAsync(_companyCode);
        if (data.Error is null)
        {
            _dropdownDirectorates = data.Result!.Items.ToList();
        }
    }
    private async Task GetDropdownScholarshipCodes()
    {
        var request = new GetScholarshipPostingDropdownRequest();
        var data = await _scholarshipPostingService.GetScholarshipDropdownAsync(request);
        if (data.Error is null)
        {
            _dropdownScholarshipCodes = data.Result!.Items.ToList();
        }
    }

    private async Task GetDropdownStatus()
    {
        _dropdownStatus = EnumExtentions.ToDropdownItem<AdmissionStatus>();
        _dropdownStatus = _dropdownStatus.OrderBy(x => x.Id).ToList();
        await Task.CompletedTask;
    }
    #endregion

    #region Dropdown Change
    public async Task OnChangeCategory(Syncfusion.Blazor.DropDowns.ChangeEventArgs<UniversityCategory, DropdownEnumResponse<UniversityCategory>> args)
    {
        if (args.ItemData == null)
        {
            _category = 0;
        }
        else
        {
            _category = args.ItemData.Id;
        }

        await SetDataOnLoadAsync();
        StateHasChanged();
    }

    public async Task OnChangeCompanyGroupAsync(Syncfusion.Blazor.DropDowns.ChangeEventArgs<int, GetCompanyGroupQueryResponse> args)
    {
        if (args.ItemData is not null)
        {
            _companyGroupId = args.ItemData.Id;
        }
        else
        {
            _companyGroupId = 0;
        }

        await GetDropdownCompany();
        await SetDataOnLoadAsync();
        StateHasChanged();
    }

    public async Task OnChangeCompanyAsync(Syncfusion.Blazor.DropDowns.ChangeEventArgs<string, GetCompanyQueryResponse> args)
    {
        if (args.ItemData is not null)
        {
            _companyCode = args.ItemData.Code;
        }
        else
        {
            _companyCode = "0000";
        }

        await GetDropdownDirectorate();
        await SetDataOnLoadAsync();
        StateHasChanged();
    }

    public async Task OnChangeDirectorateAsync(Syncfusion.Blazor.DropDowns.ChangeEventArgs<int, GetDirectorateQueryResponse> args)
    {
        if (args.ItemData is not null)
        {
            _directorateId = args.ItemData.Id;
        }
        else
        {
            _directorateId = 0;
        }

        await SetDataOnLoadAsync();
        StateHasChanged();
    }

    public async Task OnChangeScholarshipCodeAsync(Syncfusion.Blazor.DropDowns.ChangeEventArgs<Guid, GetScholarshipPostingDropdownResponse> args)
    {
        if (args.ItemData is not null)
        {
            _scholarshipCodeId = args.ItemData.Code;
        }
        else
        {
            _scholarshipCodeId = string.Empty;
        }

        await SetDataOnLoadAsync();
        StateHasChanged();
    }
    public async Task OnChangeBatchAsync(Syncfusion.Blazor.DropDowns.ChangeEventArgs<ScholarshipBatch, DropdownEnumResponse<ScholarshipBatch>> args)
    {
        if (args.ItemData == null)
        {
            _batch = 0;
        }
        else
        {
            _batch = args.ItemData.Id;
        }

        await SetDataOnLoadAsync();
        StateHasChanged();
    }
    #endregion

    #region OnClik Event
    //private void OnClickDownloadResultAsync()
    //{

    //}
    #endregion

    #region OnChange Event

    #endregion

    #region Excel
    private async Task OnClickUploadExcelAsync(MouseEventArgs e)
    {
        await Task.CompletedTask;
    }
    private async Task OnClickDownloadExcelAsync(MouseEventArgs e)
    {
        _isLoadingDialog = true;

        var table = new DataTable();

        table.Columns.Add("Id", typeof(string));
        table.Columns.Add("Name", typeof(string));
        table.Columns.Add("CompanyName", typeof(string));
        table.Columns.Add("EmployeeNumber", typeof(string));
        table.Columns.Add("Email", typeof(string));
        table.Columns.Add("University", typeof(string));
        table.Columns.Add("Status", typeof(string));

        if (_dataGrid is not null)
        {
            foreach (var item in _dataGrid)
            {
                table.Rows.Add(
                    item.Id,
                    item.Name,
                    item.CompanyName,
                    item.EmployeeNumber,
                    item.Email,
                    item.University,
                    item.Status);
            }
        }

        await GenerateExcel(table);

        _isLoadingDialog = false;
        StateHasChanged();
    }

    private async Task GenerateExcel(DataTable data, string fileName = "")
    {
        using var excelEngine = new ExcelEngine();
        var application = excelEngine.Excel;
        application.DefaultVersion = ExcelVersion.Xlsx;

        //Create a workbook
        var workbook = application.Workbooks.Create(1);
        var worksheet = workbook.Worksheets[0];
        worksheet.Range[$"A1"].ColumnWidth = 40;
        worksheet.Range[$"B1"].ColumnWidth = 40;
        worksheet.Range[$"C1"].ColumnWidth = 40;
        worksheet.Range[$"D1"].ColumnWidth = 30;
        worksheet.Range[$"E1"].ColumnWidth = 40;
        worksheet.Range[$"F1"].ColumnWidth = 40;
        worksheet.Range[$"G1"].ColumnWidth = 40;

        var headingStyle = workbook.Styles.Add("HeadingStyle");
        headingStyle.Font.Bold = true;
        worksheet.Range["H1:H10000"].CellStyle = headingStyle;

        worksheet.Protect("sch0larship");
        worksheet.Range["B2:H10000"].CellStyle.Locked = false;

        //Add Comments (Notes)
        IComment commentA = worksheet.Range[$"A1"].AddComment();
        commentA.Text = "Do not modify";
        commentA.IsVisible = false;

        IComment commentB = worksheet.Range[$"B1"].AddComment();
        commentB.Text = "Please entry Name";
        commentB.IsVisible = false;

        IComment commentC = worksheet.Range[$"C1"].AddComment();
        commentC.Text = "Please Entry Company Name";
        commentC.IsVisible = false;

        IComment commentD = worksheet.Range[$"D1"].AddComment();
        commentD.Text = "Please Entry Employee Number";
        commentD.IsVisible = false;

        IComment commentE = worksheet.Range[$"E1"].AddComment();
        commentE.Text = "Please Entry Email";
        commentE.IsVisible = false;

        IComment commentF = worksheet.Range[$"F1"].AddComment();
        commentF.Text = "University";
        commentF.IsVisible = false;

        IComment commentG = worksheet.Range[$"G1"].AddComment();
        commentG.Text = "Status : " + string.Join(" or ", _dropdownStatus.Select(x => x.Description).ToList());
        commentG.IsVisible = false;

        //import data from datatable
        worksheet.ImportDataTable(data, true, 1, 1, true);

        //save document as a stream and retrun the stream.
        var downloadStream = new MemoryStream();
        workbook.SaveAs(downloadStream, ExcelSaveType.SaveAsXLS);
        workbook.Close();

        var content = downloadStream.ToArray();

        //download document
        await _jsRuntime.InvokeVoidAsync
            (JavaScriptIdentifierFor.DownloadFile,
            string.IsNullOrEmpty(fileName) ? $"UniversityAdmission_{DateTime.Now:yyyyMMdd}.xlsx" : fileName,
            $"{ContentTypes.ApplicationOpenXmlExcel}",
            content);

    }

    private async Task ValueChangeUploaderExcel(UploadChangeEventArgs args)
    {
        await Task.CompletedTask;
    }
    protected void OnRemoveUploaderExcel(RemovingEventArgs args)
    {
    }
    #endregion

    #region Command Event
    public async Task OnCommandClicked(CommandClickEventArgs<GetAdmissionQueryResponse> args)
    {
        _navigationManager.NavigateTo(Psmp.Bsui.Features.Universities.AdmissionDetail.Constants.RouteFor.Detail(args.RowData.Id), true);
        await Task.CompletedTask;
    }
    #endregion

    #region Modal
    private void OnClickAddAsync()
    {
        TitlePopup = "Add";
        ActionType = "add";
        IsEnabled = true;
        VisiblePopup = true;
        _createAdmissionSuperiorLetterRequest = new CreateAdmissionSuperiorLetterRequest { StudyStart = DateOnly.FromDateTime(DateTime.Now), StudyEnd = DateOnly.FromDateTime(DateTime.Now) };
    }

    private async Task<string> UploadDocumentLoA()
    {
        var memoryStream = new MemoryStream();
        await FileUploadLoA.File.OpenReadStream(_maximumFileSizeInBytes).CopyToAsync(memoryStream);
        memoryStream.Position = 0; // Reset the stream position

        var formFile = new FormFile(memoryStream, 0, FileUploadLoA.File.Size, FileUploadLoA.File.Name, FileUploadLoA.File.Name)
        {
            Headers = new HeaderDictionary(),
            ContentType = FileUploadLoA.File.ContentType
        };

        var file = IBrowserFileExtensions.ToFormFile(FileUploadLoA.File, memoryStream, FileUploadLoA.File.Name);

        var requestFile = new CreateDocumentRequest
        {
            DocumentType = DocumentType.AdmissionLetterOfAcceptance,
            File = file,
            EmployeeNumber = _user.GetEmployeeId()
        };

        var resultUpload = await _documentService.CreateDocumentAsync(requestFile);
        if (resultUpload.Error is not null)
        {
            return string.Join(", ", resultUpload.Error.Details.ToList());
        }

        DocumentIdLoA = resultUpload.Result!.DocumentId;
        return string.Empty;
    }

    private async Task<string> UploadDocumentSuperiorApproval()
    {
        var memoryStream = new MemoryStream();
        await FileUploadSuperiorApproval.File.OpenReadStream(_maximumFileSizeInBytes).CopyToAsync(memoryStream);
        memoryStream.Position = 0; // Reset the stream position

        var formFile = new FormFile(memoryStream, 0, FileUploadSuperiorApproval.File.Size, FileUploadSuperiorApproval.File.Name, FileUploadSuperiorApproval.File.Name)
        {
            Headers = new HeaderDictionary(),
            ContentType = FileUploadSuperiorApproval.File.ContentType
        };

        var file = IBrowserFileExtensions.ToFormFile(FileUploadSuperiorApproval.File, memoryStream, FileUploadSuperiorApproval.File.Name);

        var requestFile = new CreateDocumentRequest
        {
            DocumentType = DocumentType.AdmissionSuperiorApprovalLetter,
            File = file,
            EmployeeNumber = _user.GetEmployeeId(),
        };

        var resultUpload = await _documentService.CreateDocumentAsync(requestFile);
        if (resultUpload.Error is not null)
        {
            return string.Join(", ", resultUpload.Error.Details.ToList());
        }

        DocumentIdSuperiorApproval = resultUpload.Result!.DocumentId;
        return string.Empty;
    }

    private void DialogOpen()
    {
        VisiblePopup = true;
    }

    private void DialogClose()
    {
        ActionType = "";
        VisiblePopup = false;
        _createAdmissionSuperiorLetterRequest = new();
    }
    private void ClickedNo()
    {
        DialogClose();
    }

    private async Task ClickedYes()
    {
        _isLoadingDialog = true;
        if (FileUploadLoA.FileName is null)
        {
            _snackbar.AddWarning("Please browse Letter of Acceptance !");
            _isLoadingDialog = false;
            StateHasChanged();
            return;
        }

        if (FileUploadSuperiorApproval.FileName is null)
        {
            _snackbar.AddWarning("Please browse Superior Approval Letter !");
            _isLoadingDialog = false;
            StateHasChanged();
            return;
        }

        if (ActionType == "add")
        {
            #region Create
            var resUpload = await UploadDocumentLoA();
            if (!string.IsNullOrEmpty(resUpload))
            {
                _snackbar.AddError("Error : " + resUpload);
                _isLoadingDialog = false;
                StateHasChanged();
                return;
            }

            resUpload = await UploadDocumentSuperiorApproval();
            if (!string.IsNullOrEmpty(resUpload))
            {
                _snackbar.AddError("Error : " + resUpload);
                _isLoadingDialog = false;
                StateHasChanged();
                return;
            }

            var createData = new CreateAdmissionSuperiorLetterRequest
            {
                UniversityRecommendationId = _createAdmissionSuperiorLetterRequest.UniversityRecommendationId,
                ApplicantId = _createAdmissionSuperiorLetterRequest.ApplicantId,
                IsFulltime = _createAdmissionSuperiorLetterRequest.IsFulltime,
                StudyStart = _createAdmissionSuperiorLetterRequest.StudyStart,
                StudyEnd = _createAdmissionSuperiorLetterRequest.StudyEnd,
                AcceptanceLetterDocumentId = DocumentIdLoA,
                ApprovalLetterDocumentId = DocumentIdSuperiorApproval
            };
            var createDataResponse = await _universityAdmissionSuperiorLetterService.CreateUniversityAdmissionSuperiorLetterAsync(createData);
            if (createDataResponse.Error is not null)
            {
                var result = string.Join(", ", createDataResponse.Error.Details.ToList());
                _snackbar.AddError("Applicant - Error : " + result);
                _isLoadingDialog = false;
                StateHasChanged();
                return;
            }

            _snackbar.AddSuccess("Success add data");
            #endregion
        }

        await SetDataOnLoadAsync();
        StateHasChanged();
        DialogClose();
        _isLoadingDialog = false;
    }

    private async Task ValueChangeUploaderLoA(UploadChangeEventArgs args)
    {
        FileUploadLoA.FileName = args.Files.First().File.Name;
        FileUploadLoA.FileSize = args.Files.First().File.Size;
        FileUploadLoA.FileContentType = args.Files.First().File.ContentType;
        FileUploadLoA.File = args.Files.First().File;

        await Task.CompletedTask;
    }

    protected void OnRemoveUploaderLoA(RemovingEventArgs args)
    {
        FileUploadLoA = new();
        args.PostRawFile = false;
    }
    private async Task ValueChangeUploaderSuperiorApproval(UploadChangeEventArgs args)
    {
        FileUploadSuperiorApproval.FileName = args.Files.First().File.Name;
        FileUploadSuperiorApproval.FileSize = args.Files.First().File.Size;
        FileUploadSuperiorApproval.FileContentType = args.Files.First().File.ContentType;
        FileUploadSuperiorApproval.File = args.Files.First().File;

        await Task.CompletedTask;
    }
    protected void OnRemoveUploaderSuperiorApproval(RemovingEventArgs args)
    {
        FileUploadSuperiorApproval = new();
        args.PostRawFile = false;
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
    public void ExcelQueryCellInfoHandler(ExcelQueryCellInfoEventArgs<GetAdmissionQueryResponse> args)
    {
        var a = args.Column.Field;
        if (args.Column.Field == "Status")
        {
            args.Cell.Value = args.Data.Status.GetDescription().ToString();
        }
    }
}

public class ApplicantDropdown
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
}
