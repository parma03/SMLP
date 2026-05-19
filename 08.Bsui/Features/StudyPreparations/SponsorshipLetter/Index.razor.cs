using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Psmp.Base.Scholarships.Enums;
using Psmp.Base.Selections.Enums;
using Psmp.Base.Studies.Enums;
using Psmp.Bsui.Common.Constants;
using Psmp.Bsui.Common.Extensions;
using Psmp.Shared.Common.Constants;
using Psmp.Shared.Common.Extensions;
using Psmp.Shared.Common.Responses;
using Psmp.Shared.Documents.Commands.CreateDocument;
using Psmp.Shared.Masters.Queries;
using Psmp.Shared.Scholarships.ScholarshipPosting.Queries;
using Psmp.Shared.StudyPreparations.SponsorshipLetter.Commands.Create;
using Psmp.Shared.StudyPreparations.SponsorshipLetter.Queries;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Inputs;
using Syncfusion.XlsIO;

namespace Psmp.Bsui.Features.StudyPreparations.SponsorshipLetter;

public partial class Index
{
    #region Protected Properties
    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;
    protected ClaimsPrincipal _user = default!;

    protected ErrorResponse? _error;
    protected bool _isLoading = false;
    protected bool _isLoadingDialog = false;
    protected List<GetSponsorshipLetterQueryResponse> _dataGrid = new();

    protected long _maximumFileSizeInBytes = long.MaxValue;
    protected DataTable _tableExcel = new();
    protected List<GetSponsorshipLetterQueryResponse> _workbookExcel = new();
    protected SfUploader? _uploadExcel;
    #endregion

    #region Modal Properties
    private string? ActionType { get; set; }
    private bool IsEnabled { get; set; }
    private bool VisiblePopup { get; set; }
    private string TitlePopup { get; set; } = "Add";
    public List<ApplicantDropdown> _listApplicant = new();
    public List<DropdownItemResponse> _listBringFamily = new();
    protected CreateSponsorshipLetterRequest _createData = new();
    public Guid DocumentIdSupportDoc { get; set; } = new();
    public FileUploadResponse FileUploadSupportDoc { get; set; } = new();
    #endregion

    #region Card Properties
    protected int CountAll { get; set; } = 0;
    protected int CountRequest { get; set; } = 0;
    protected int CountCompleted { get; set; } = 0;
    #endregion

    #region Dropdown
    public List<DropdownItemResponse> _dropdownHps = new();
    public List<DropdownItemResponse> _dropdownCompany = new();
    public List<DropdownItemResponse> _dropdownDirectorate = new();
    public List<DropdownItemResponse> _dropdownScholarshipCode = new();
    public List<DropdownItemResponse> _dropdownBatch = new();
    public List<DropdownItemResponse> _dropdownStatus = new();
    #endregion
    #region Dropdown Properties
    protected ScholarshipLocation _category = 0;
    protected int _companyGroupId = 0;
    protected string _companyCode = "0000";
    protected int _directorateId = 0;
    protected string _scholarshipCodeId = string.Empty;
    protected ScholarshipBatch _batch = 0;

    public List<DropdownItemResponse> _dropdownCategories = new();
    public List<GetCompanyGroupQueryResponse> _dropdownCompanyGroups = new();
    public List<GetCompanyQueryResponse> _dropdownCompanies = new();
    public List<GetDirectorateQueryResponse> _dropdownDirectorates = new();
    protected List<GetScholarshipPostingDropdownResponse> _dropdownScholarshipCodes = new();
    public IEnumerable<ScholarshipBatchResponse> _dropdownBatchs = GetEnumScholarshipBatch<ScholarshipBatch>();
    #endregion

    protected override async Task OnInitializedAsync()
    {
        _user = (await AuthenticationStateTask).User;
        _maximumFileSizeInBytes = _documentOptions.Value.MaximumFileSizeInBytes;
        _isLoading = true;
        _companyCode = _user.GetCompanyCode();
        await GetDropdownStatus();
        await SetDataOnLoadAsync();
        await OnLoadHardcode();

        await GetDropdownCategories();
        await GetDropdownCompanyGroup();
        await GetDropdownCompany();
        await GetDropdownDirectorate();
        await GetDropdownScholarshipCodes();
        _isLoading = false;
    }

    #region DataGrid
    private async Task SetDataOnLoadAsync()
    {
        var data = await _studyPreparationSponsorshipLetterService.GetStudyPreparationSponsorshipLetterAsync();
        if (data.Error is null)
        {
            _dataGrid = data.Result!.Items.ToList();

            if (_category != 0)
            {
                _dataGrid = _dataGrid.Where(x => x.Category == _category).ToList();
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

            CountAll = _dataGrid.GroupBy(x => x.EmployeeNumber).Count();
            CountRequest = _dataGrid.Count;
            CountCompleted = _dataGrid.Where(x => x.Status == SponsorshipLetterStatus.Completed).Count();
        }
        else
        {
            _error = data.Error;
        }

        await Task.CompletedTask;
    }
    #endregion

    #region Dropdown
    private async Task GetDropdownStatus()
    {
        _dropdownStatus = EnumExtentions.ToDropdownItem<SponsorshipLetterStatus>();
        _dropdownStatus = _dropdownStatus.OrderBy(x => x.Id).ToList();
        await Task.CompletedTask;
    }

    private async Task GetDropdownCategories()
    {
        _dropdownCategories = EnumExtentions.ToDropdownItem<ScholarshipLocation>();
        _dropdownCategories = _dropdownCategories.OrderBy(x => x.Id == 0).ToList();
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

    private async Task OnLoadHardcode()
    {
        var data = new List<DropdownItemResponse>()
        {
            new() {Id = 0, Description = "No" },
            new() {Id = 1, Description = "Yes"}
        };
        _listBringFamily = data;

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
    #endregion

    #region Dropdown Change
    public async Task OnChangeCategory(Syncfusion.Blazor.DropDowns.ChangeEventArgs<int, DropdownItemResponse> args)
    {
        if (args.ItemData == null)
        {
            _category = 0;
        }
        else if (args.ItemData.Id == 1)
        {
            _category = ScholarshipLocation.Abroad;
        }
        else if (args.ItemData.Id == 2)
        {
            _category = ScholarshipLocation.Domestic;
        }

        StateHasChanged();
        await SetDataOnLoadAsync();
        await Task.CompletedTask;
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
        await Task.CompletedTask;
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
        await Task.CompletedTask;
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
        await Task.CompletedTask;
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
        await Task.CompletedTask;
    }
    public async Task OnChangeBatchAsync(Syncfusion.Blazor.DropDowns.ChangeEventArgs<ScholarshipBatch, ScholarshipBatchResponse> args)
    {
        if (args.ItemData == null)
        {
            _batch = 0;
        }
        else
        {
            _batch = args.ItemData.Value ?? default!;
        }

        await SetDataOnLoadAsync();
        StateHasChanged();
        await Task.CompletedTask;
    }
    #endregion

    #region Command Event
    public async Task OnCommandClicked(CommandClickEventArgs<GetSponsorshipLetterQueryResponse> args)
    {
        _isLoadingDialog = true;
        var dataResponse = await _studyPreparationSponsorshipLetterService.GetStudyPreparationSponsorshipLetterByIdAsync(args.RowData.Id);
        if (dataResponse.Error is null)
        {
            _isLoadingDialog = false;
            var data = dataResponse.Result;
            switch (data!.Status)
            {
                case SponsorshipLetterStatus.Accepted:
                case SponsorshipLetterStatus.Completed:
                case SponsorshipLetterStatus.ProcessSign:
                case SponsorshipLetterStatus.ProcessToPOffice:
                case SponsorshipLetterStatus.Draft:
                case SponsorshipLetterStatus.Rejected:
                    _navigationManager.NavigateTo(Psmp.Bsui.Features.StudyPreparations.Constants.RouteFor.StudyPreparationDetailAccepted("SponsorshipLetter", args.RowData.Id), true);
                    break;
                case SponsorshipLetterStatus.WaitingApproval:
                default:
                    _navigationManager.NavigateTo(Psmp.Bsui.Features.StudyPreparations.Constants.RouteFor.StudyPreparationDetail("SponsorshipLetter", args.RowData.Id), true);
                    break;
            }
        }

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
        _createData = new CreateSponsorshipLetterRequest();
    }

    private async Task<string> UploadDocumentSupportDoc()
    {
        var memoryStream = new MemoryStream();
        await FileUploadSupportDoc.File.OpenReadStream(_maximumFileSizeInBytes).CopyToAsync(memoryStream);
        memoryStream.Position = 0; // Reset the stream position

        var formFile = new FormFile(memoryStream, 0, FileUploadSupportDoc.File.Size, FileUploadSupportDoc.File.Name, FileUploadSupportDoc.File.Name)
        {
            Headers = new HeaderDictionary(),
            ContentType = FileUploadSupportDoc.File.ContentType
        };

        var file = IBrowserFileExtensions.ToFormFile(FileUploadSupportDoc.File, memoryStream, FileUploadSupportDoc.File.Name);

        var requestFile = new CreateDocumentRequest
        {
            DocumentType = DocumentType.SponsorshipLetter,
            File = file,
            EmployeeNumber = _user.GetEmployeeId()
        };

        var resultUpload = await _documentService.CreateDocumentAsync(requestFile);
        if (resultUpload.Error is not null)
        {
            return string.Join(", ", resultUpload.Error.Details.ToList());
        }

        DocumentIdSupportDoc = resultUpload.Result!.DocumentId;
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
        _createData = new();
    }
    private void ClickedNo()
    {
        DialogClose();
    }

    private async Task ClickedYes()
    {
        _isLoadingDialog = true;
        if (FileUploadSupportDoc.FileName is null)
        {
            _snackbar.AddWarning("Please browse Letter of Acceptance !");
            _isLoadingDialog = false;
            StateHasChanged();
            return;
        }

        if (ActionType == "add")
        {
            #region Create
            var resUpload = await UploadDocumentSupportDoc();
            if (!string.IsNullOrEmpty(resUpload))
            {
                _snackbar.AddError("Error : " + resUpload);
                _isLoadingDialog = false;
                StateHasChanged();
                return;
            }

            var createData = new CreateSponsorshipLetterRequest
            {
                ApplicantId = _createData.ApplicantId,
                UniversityName = _createData.UniversityName,
                AddressTo = _createData.AddressTo,
                Study = _createData.Study,
                IsBringFamily = _createData.IsBringFamily,
                Country = _createData.Country,
                AcademicYear = _createData.AcademicYear,
                UniversityRank = _createData.UniversityRank,
                UniversityDueDate = _createData.UniversityDueDate,
                DocumentId = DocumentIdSupportDoc
            };
            var createDataResponse = await _studyPreparationSponsorshipLetterService.CreateStudyPreparationSponsorshipLetterAsync(createData);
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
    protected void OnRemoveUploaderSupportDoc(RemovingEventArgs args)
    {
        FileUploadSupportDoc = new();
        args.PostRawFile = false;
    }
    private async Task ValueChangeUploaderSupportDoc(UploadChangeEventArgs args)
    {
        FileUploadSupportDoc.FileName = args.Files.First().File.Name;
        FileUploadSupportDoc.FileSize = args.Files.First().File.Size;
        FileUploadSupportDoc.FileContentType = args.Files.First().File.ContentType;
        FileUploadSupportDoc.File = args.Files.First().File;

        await Task.CompletedTask;
    }
    #endregion

    #region Excel
    private async Task OnClickDownloadResultAsync()
    {
        var table = new DataTable();

        table.Columns.Add("Id", typeof(string));
        table.Columns.Add("Name", typeof(string));
        table.Columns.Add("Company Name", typeof(string));
        table.Columns.Add("Employee Number", typeof(string));
        table.Columns.Add("Email", typeof(string));
        table.Columns.Add("University Name", typeof(string));
        table.Columns.Add("Letter Addressed To", typeof(string));
        table.Columns.Add("Program Master Of", typeof(string));
        table.Columns.Add("Academic Year", typeof(string));
        table.Columns.Add("Bring Family", typeof(string));
        table.Columns.Add("Country", typeof(string));
        table.Columns.Add("Ranking THE/ARWU/QS", typeof(string));
        table.Columns.Add("University Application Due Date", typeof(string));
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
                    item.UniversityName,
                    item.AddressTo,
                    item.Study,
                    item.AcademicYear,
                    item.IsBringFamily == true ? "Yes" : "No",
                    item.Country,
                    item.UniversityRank,
                    item.UniversityDueDate.ToShortDateString(),
                    item.Status);
            }
        }

        await GenerateExcel(table);
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
        worksheet.Range[$"H1"].ColumnWidth = 40;
        worksheet.Range[$"I1"].ColumnWidth = 40;
        worksheet.Range[$"J1"].ColumnWidth = 40;
        worksheet.Range[$"K1"].ColumnWidth = 40;
        worksheet.Range[$"L1"].ColumnWidth = 40;
        worksheet.Range[$"M1"].ColumnWidth = 40;
        worksheet.Range[$"N1"].ColumnWidth = 40;

        var headingStyle = workbook.Styles.Add("HeadingStyle");
        headingStyle.Font.Bold = true;
        worksheet.Range["N1:N10000"].CellStyle = headingStyle;

        worksheet.Protect("sch0larship");
        worksheet.Range["B2:N10000"].CellStyle.Locked = false;

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
        commentE.Text = "Please Entry University Name";
        commentE.IsVisible = false;

        IComment commentG = worksheet.Range[$"G1"].AddComment();
        commentE.Text = "Please Entry Letter Addressed To";
        commentE.IsVisible = false;

        IComment commentH = worksheet.Range[$"H1"].AddComment();
        commentE.Text = "Please Entry Program Master Of";
        commentE.IsVisible = false;

        IComment commentI = worksheet.Range[$"I1"].AddComment();
        commentE.Text = "Please Entry Academic Year";
        commentE.IsVisible = false;

        IComment commentJ = worksheet.Range[$"J1"].AddComment();
        commentE.Text = "Please Entry Bring Family";
        commentE.IsVisible = false;

        IComment commentK = worksheet.Range[$"K1"].AddComment();
        commentE.Text = "Please Entry Country";
        commentE.IsVisible = false;

        IComment commentL = worksheet.Range[$"L1"].AddComment();
        commentE.Text = "Please Entry Ranking THE/ARWU/QS";
        commentE.IsVisible = false;

        IComment commentM = worksheet.Range[$"M1"].AddComment();
        commentE.Text = "Please Entry University Application Due Date";
        commentE.IsVisible = false;

        IComment commentN = worksheet.Range[$"N1"].AddComment();
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
            string.IsNullOrEmpty(fileName) ? $"SponsorshipLetter_{DateTime.Now:yyyyMMdd}.xlsx" : fileName,
            $"{ContentTypes.ApplicationOpenXmlExcel}",
            content);

    }
    #endregion
}

public class ApplicantDropdown
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
}
