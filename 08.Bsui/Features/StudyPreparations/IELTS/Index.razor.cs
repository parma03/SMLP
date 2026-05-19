using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using System.Data;
using System.Security.Claims;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.StudyPreparations.Ielts.Commands.Update;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Inputs;
using Smlp.Shared.StudyPreparations.Ielts.Queries;
using Smlp.Base.Studies.Enums;
using Smlp.Base.Scholarships.Enums;
using Smlp.Shared.Masters.Queries;
using Smlp.Shared.Scholarships.ScholarshipPosting.Queries;
using Smlp.Shared.Common.Extensions;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Smlp.Bsui.Common.Extensions;
using Smlp.Base.Selections.Enums;
using Smlp.Shared.Documents.Commands.CreateDocument;
using Smlp.Shared.Documents.Queries.GetDocuments;
using Microsoft.JSInterop;
using Smlp.Bsui.Common.Constants;
using Smlp.Shared.Common.Constants;
using Smlp.Shared.Documents.Queries.GetDocumentFile;
using Syncfusion.XlsIO;

namespace Smlp.Bsui.Features.StudyPreparations.IELTS;

public partial class Index
{
    #region Protected Properties
    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;
    protected ClaimsPrincipal _user = default!;

    protected ErrorResponse? _error;
    protected bool _isLoading = false;
    protected bool _isLoadingDialog = false;
    protected List<GetIeltsQueryResponse> _dataGrid = new();

    protected long _maximumFileSizeInBytes = long.MaxValue;
    protected DataTable _tableExcel = new();
    protected List<GetIeltsQueryResponse> _workbookExcel = new();
    protected SfUploader? _uploadExcel;
    #endregion

    #region Card Properties
    protected string _totalAwardee = "0";
    protected string _gmatParticipants = "0";
    protected string _gmatAverage = "0";
    #endregion

    #region Modal
    private bool IsAuploadedDocument { get; set; } = false;
    private bool VisiblePopup { get; set; }
    private bool VisiblePopupDelete { get; set; }
    private string TitlePopup { get; set; } = "Edit IELTS";
    private bool IsEnabled { get; set; }
    public UpdateIeltsRequest _requestIELTS = new();
    public UpdateIeltsRequest _updateRequestIELTS = new();

    public Guid DocumentIdIeltsDoc { get; set; } = new();
    public Guid DocumentIdDelete { get; set; } = new();
    private string? DeleteType { get; set; }
    public FileUploadResponse FileUploadIeltsDoc { get; set; } = new();
    protected List<GetDocumentsDocument> _getListDocuments = new();

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
        var data = await _studyPreparationIeltsService.GetStudyPreparationIeltsAsync();
        if (data.Error is null)
        {
            _dataGrid = data.Result!.Items.ToList();

            if ((string.IsNullOrEmpty(_companyCode) ? "0000" : _companyCode) != "0000")
            {
                _dataGrid = _dataGrid.Where(x => x.CompanyCode == _companyCode).ToList();
            }

            if (_directorateId != 0)
            {
                _dataGrid = _dataGrid.Where(x => x.DirectorateId == _directorateId).ToList();
            }

            if (_dataGrid.Count > 0)
            {
                _totalAwardee = _dataGrid.GroupBy(x => x.EmployeeNumber).Count().ToString();
                _gmatParticipants = _dataGrid.Count(x => x.Score > 0).ToString();
                _gmatAverage = TopScorersByName.Average(x => x.Score).ToString();
            }
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
        _dropdownStatus = EnumExtentions.ToDropdownItem<VisaGuaranteeStatus>();
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

    #region Command & Modal Events
    private readonly string _judul1 = "Total Awardee";
    private readonly string _judul2 = "IELTS Participants";
    private readonly string _judul3 = "IELTS Average";
    protected SfUploader? _uploadObj;

    private void OnClickAddAsync()
    {
    }
    private async Task<string> UploadDocumentIeltsDoc()
    {
        var memoryStream = new MemoryStream();
        await FileUploadIeltsDoc.File.OpenReadStream(_maximumFileSizeInBytes).CopyToAsync(memoryStream);
        memoryStream.Position = 0; // Reset the stream position

        var formFile = new FormFile(memoryStream, 0, FileUploadIeltsDoc.File.Size, FileUploadIeltsDoc.File.Name, FileUploadIeltsDoc.File.Name)
        {
            Headers = new HeaderDictionary(),
            ContentType = FileUploadIeltsDoc.File.ContentType
        };

        var file = IBrowserFileExtensions.ToFormFile(FileUploadIeltsDoc.File, memoryStream, FileUploadIeltsDoc.File.Name);

        var requestFile = new CreateDocumentRequest
        {
            DocumentType = DocumentType.IELTSCertificate,
            File = file,
            EmployeeNumber = _requestIELTS.EmployeeNumber
        };

        var resultUpload = await _documentService.CreateDocumentAsync(requestFile);
        if (resultUpload.Error is not null)
        {
            return string.Join(", ", resultUpload.Error.Details.ToList());
        }

        DocumentIdIeltsDoc = resultUpload.Result!.DocumentId;
        return string.Empty;
    }

    protected void OnRemoveUploaderIeltsDoc(RemovingEventArgs args)
    {
        FileUploadIeltsDoc = new();
        args.PostRawFile = false;
    }
    private async Task ValueChangeUploaderIeltsDoc(UploadChangeEventArgs args)
    {
        FileUploadIeltsDoc.FileName = args.Files.First().File.Name;
        FileUploadIeltsDoc.FileSize = args.Files.First().File.Size;
        FileUploadIeltsDoc.FileContentType = args.Files.First().File.ContentType;
        FileUploadIeltsDoc.File = args.Files.First().File;

        await Task.CompletedTask;
    }
    private async Task OnClickUploadAsync()
    {
    }
    private async Task OnClickDownloadAsync()
    {
        _isLoadingDialog = true;

        var table = new DataTable();

        table.Columns.Add("Id", typeof(string));
        table.Columns.Add("Name", typeof(string));
        table.Columns.Add("CompanyName", typeof(string));
        table.Columns.Add("EmployeeNumber", typeof(string));
        table.Columns.Add("Email", typeof(string));
        table.Columns.Add("IELTS Score", typeof(string));

        if (_dataGrid is not null)
        {
            foreach (var item in _dataGrid)
            {
                table.Rows.Add(
                    item.Id,
                    item.ApplicantName,
                    item.CompanyName,
                    item.EmployeeNumber,
                    item.Email,
                    item.Score);
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

        var headingStyle = workbook.Styles.Add("HeadingStyle");
        headingStyle.Font.Bold = true;
        worksheet.Range["G1:G10000"].CellStyle = headingStyle;

        worksheet.Protect("IELTS");
        worksheet.Range["B2:H10000"].CellStyle.Locked = false;

        ////Add Comments (Notes)
        //IComment commentA = worksheet.Range[$"A1"].AddComment();
        //commentA.Text = "Do not modify";
        //commentA.IsVisible = false;

        //IComment commentB = worksheet.Range[$"B1"].AddComment();
        //commentB.Text = "Please entry Name";
        //commentB.IsVisible = false;

        //IComment commentC = worksheet.Range[$"C1"].AddComment();
        //commentC.Text = "Please Entry Company Name";
        //commentC.IsVisible = false;

        //IComment commentD = worksheet.Range[$"D1"].AddComment();
        //commentD.Text = "Please Entry Employee Number";
        //commentD.IsVisible = false;

        //IComment commentE = worksheet.Range[$"E1"].AddComment();
        //commentE.Text = "Please Entry Email";
        //commentE.IsVisible = false;

        //IComment commentG = worksheet.Range[$"F1"].AddComment();
        //commentG.Text = "Status : " + string.Join(" or ", _dropdownStatus.Select(x => x.Description).ToList());
        //commentG.IsVisible = false;

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
            string.IsNullOrEmpty(fileName) ? $"IELTS_{DateTime.Now:yyyyMMdd}.xlsx" : fileName,
            $"{ContentTypes.ApplicationOpenXmlExcel}",
            content);

    }
    private void DialogOpen()
    {
        VisiblePopup = true;
    }

    private void DialogClose()
    {
        VisiblePopup = false;
        _requestIELTS = new UpdateIeltsRequest();
    }

    private void ClickedNo()
    {
        VisiblePopup = false;
        _requestIELTS = new UpdateIeltsRequest();
    }

    private async Task ClickedYes()
    {
        _isLoadingDialog = true;

        if (FileUploadIeltsDoc.FileName is null)
        {
            _snackbar.AddWarning("Please browse IELTS Certificate!");
            _isLoadingDialog = false;
            StateHasChanged();
            return;
        }

        var resUpload = await UploadDocumentIeltsDoc();
        if (!string.IsNullOrEmpty(resUpload))
        {
            _snackbar.AddError("Error : " + resUpload);
            _isLoadingDialog = false;
            StateHasChanged();
            return;
        }

        _updateRequestIELTS.Id = _requestIELTS.Id;
        _updateRequestIELTS.IeltsScore = _requestIELTS.IeltsScore;
        _updateRequestIELTS.LocationTest = _requestIELTS.LocationTest;
        _updateRequestIELTS.TestDate = _requestIELTS.TestDate;
        _updateRequestIELTS.TestCenter = _requestIELTS.TestCenter;
        _updateRequestIELTS.DocumentId = DocumentIdIeltsDoc;

        var updateIelts = await _studyPreparationIeltsService.UpdateIeltsAsync(_updateRequestIELTS);

        VisiblePopup = false;
        _isLoadingDialog = false;
        _requestIELTS = new UpdateIeltsRequest();
        _snackbar.AddSuccess("Success update data");
        _isLoadingDialog = false;
        await SetDataOnLoadAsync();
        StateHasChanged();
    }

    public void OnCommandClicked(CommandClickEventArgs<GetIeltsQueryResponse> args)
    {
        //_gridData = new();
        _getListDocuments = new();
        if (args.CommandColumn.Type == CommandButtonType.Edit)
        {
            IsEnabled = true;
            VisiblePopup = true;
            _requestIELTS.Id = args.RowData.Id;
            _requestIELTS.Name = args.RowData.ApplicantName;
            _requestIELTS.Company = args.RowData.CompanyName;
            _requestIELTS.Category = StudyCategory.Ielts.GetDescription();
            _requestIELTS.EmployeeNumber = args.RowData.EmployeeNumber;
            _requestIELTS.Email = args.RowData.Email;
            _requestIELTS.IeltsScore = args.RowData.Score;
            _requestIELTS.TestCenter = args.RowData.TestCenter;
            _requestIELTS.LocationTest = args.RowData.LocationTest;
            _requestIELTS.TestDate = args.RowData.TestDate;

            if (!string.IsNullOrEmpty(args.RowData.DocumentFileName))
            {
                IsAuploadedDocument = true;
                _getListDocuments.Add(new GetDocumentsDocument
                {
                    Id = args.RowData.DocumentId.Value,
                    FileName = args.RowData.DocumentFileName!,
                    DocumentType = DocumentType.IELTSCertificate,
                });
            }
            else
            {
                IsAuploadedDocument = false;
            }
        }
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
                DocumentIdDelete = args.RowData.Id;
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
            DocumentType = DocumentType.IELTSCertificate,
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

    #endregion

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
            var response = await _studyPreparationIeltsService.DeleteIeltsCertificateAsync(DocumentIdDelete);
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
    }

    #endregion Popup Delete

    private string FormatScore(float? score)
    {
        if (score == null)
        {
            return string.Empty; // Jika null, tampilkan kosong
        }

        return score.Value % 1 == 0 ? score.Value.ToString("0") : score.Value.ToString("0.##");
    }

    private List<GetIeltsQueryResponse> TopScorersByName => _dataGrid
    .GroupBy(x => x.ApplicantName)
    .Select(g => g.OrderByDescending(x => x.Score).First())
    .ToList();
}

public class DataStudyPreparation
{
    public int? Index { get; set; }
    public string? Name { get; set; }
    public string? Company { get; set; }
    public string? Category { get; set; }
    public string? EmployeeNumber { get; set; }
    public string? Email { get; set; }
    public int? IeltsScore { get; set; }
    public string? TestCenter { get; set; }
    public string? LocationTest { get; set; }
    public DateOnly? TestDate { get; set; }
    public string? Aksi { get; set; }
}
