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
using Psmp.Shared.Common.Constants;
using Psmp.Shared.Common.Extensions;
using Psmp.Shared.Common.Responses;
using Psmp.Shared.Documents.Commands.CreateDocument;
using Psmp.Shared.Masters.Queries;
using Psmp.Shared.Scholarships.ScholarshipPosting.Queries;
using Psmp.Shared.StudyMonitorings.Graduate.Commands.Create;
using Psmp.Shared.StudyMonitorings.Graduate.Queries;
using Psmp.Shared.Universities.Admission.Queries;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Inputs;
using Syncfusion.XlsIO;

namespace Psmp.Bsui.Features.StudyMonitoring.Graduate;

public partial class Index
{
    #region Parameter
    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

    protected ClaimsPrincipal _user = default!;
    #endregion

    #region Protected
    protected ErrorResponse? _error;
    protected bool _isLoading = false;
    protected bool _isLoadingDialog = false;
    protected List<GetGraduateQueryResponse> _dataGrid = new();
    #endregion

    #region Upload Download
    protected long _maximumFileSizeInBytes = long.MaxValue;
    protected DataTable _tableExcel = new();
    protected List<GetGraduateQueryResponse> _workbookExcel = new();
    protected SfUploader? _uploadExcel;
    #endregion

    #region Card
    private int _countAll = 0;
    private float _highest = 0;
    private float _lowest = 0;
    private float _average = 0;
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

    #region Add Data
    private string? ActionType { get; set; }
    private bool IsEnabled { get; set; }
    private bool VisiblePopup { get; set; }
    private string TitlePopup { get; set; } = "Add";
    protected CreateGraduateRequest CreateData { get; set; } = new();

    public Guid DocumentIdLastExam { get; set; } = new();
    public FileUploadResponse FileUploadLastExam { get; set; } = new();
    public Guid DocumentIdGraduation { get; set; } = new();
    public FileUploadResponse FileUploadGraduation { get; set; } = new();
    public Guid DocumentIdTranscript { get; set; } = new();
    public FileUploadResponse FileUploadTranscript { get; set; } = new();
    public Guid DocumentIdCompletion { get; set; } = new();
    public FileUploadResponse FileUploadCompletion { get; set; } = new();
    public Guid DocumentIdCourseFinal { get; set; } = new();
    public FileUploadResponse FileUploadCourseFinal { get; set; } = new();
    public List<GetAdmissionQueryResponse> _dropdownAdmission = new();
    #endregion

    protected override async Task OnInitializedAsync()
    {
        _user = (await AuthenticationStateTask).User;
        _isLoading = true;
        _companyCode = _user.GetCompanyCode();
        await SetDataOnLoadAsync();

        await GetDropdownCategories();
        await GetDropdownCompanyGroup();
        await GetDropdownCompany();
        await GetDropdownDirectorate();
        await GetDropdownScholarshipCodes();
        _isLoading = false;
        await Task.CompletedTask;
    }

    #region DataGrid
    private async Task SetDataOnLoadAsync()
    {
        var data = await _studyMonitoringGraduateService.GetGraduateAsync();
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

            _countAll = _dataGrid.Count();
            _highest = 0;
            _lowest = 0;
            _average = 0;
            if (_dataGrid.Count() > 0)
            {
                _highest = _dataGrid.Max(x => x.GpaFinalScore);
                _lowest = _dataGrid.Min(x => x.GpaFinalScore);
                _average = _dataGrid.Average(x => x.GpaFinalScore);
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

    #region Command Event
    public async Task OnCommandClicked(CommandClickEventArgs<GetGraduateQueryResponse> args)
    {
        _navigationManager.NavigateTo(Psmp.Bsui.Features.StudyMonitoring.Constants.RouteFor.StudyMonitoringDetail("Graduate", args.RowData.Id), true);
        await Task.CompletedTask;
    }
    #endregion

    #region Uploaded
    private async Task ValueChangeUploaderExcel(UploadChangeEventArgs args)
    {
        await Task.CompletedTask;
    }

    protected void OnRemoveUploaderExcel(RemovingEventArgs args)
    {
    }

    private async Task OnClickUploadAsync(MouseEventArgs e)
    {
        await Task.CompletedTask;
    }

    private async Task OnClickDownloadAsync(MouseEventArgs e)
    {
        _isLoadingDialog = true;

        var table = new DataTable();

        table.Columns.Add("Id", typeof(string));
        table.Columns.Add("Name", typeof(string));
        table.Columns.Add("CompanyName", typeof(string));
        table.Columns.Add("EmployeeNumber", typeof(string));
        table.Columns.Add("Email", typeof(string));
        table.Columns.Add("University", typeof(string));
        table.Columns.Add("GPA Score", typeof(string));

        if (_dataGrid is not null)
        {
            foreach (var item in _dataGrid)
            {
                table.Rows.Add(
                    item.AdmissionId,
                    item.Name,
                    item.CompanyName,
                    item.EmployeeNumber,
                    item.Email,
                    item.University,
                    item.GpaFinalScore);
            }
        }

        await GenerateExcel(table);

        _isLoadingDialog = false;
        StateHasChanged();
        await Task.CompletedTask;
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
        commentG.Text = "GPA Score";
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
            string.IsNullOrEmpty(fileName) ? $"Graduate_{DateTime.Now:yyyyMMdd}.xlsx" : fileName,
            $"{ContentTypes.ApplicationOpenXmlExcel}",
            content);

    }
    #endregion

    #region Add Data
    private async Task OnClickAddAsync()
    {
        TitlePopup = "Add";
        IsEnabled = true;
        VisiblePopup = true;
        CreateData = new CreateGraduateRequest();
        await OnLoadAdmission();
    }

    private async Task OnLoadAdmission()
    {
        var data = await _universityAdmissionService.GetUniversityAdmissionAsync();
        if (data.Error is null)
        {
            _dropdownAdmission = data.Result!.Items.ToList();
        }
    }
    private void DialogOpen()
    {
        VisiblePopup = true;
    }

    private void DialogClose()
    {
        ActionType = "";
        VisiblePopup = false;
        CreateData = new CreateGraduateRequest();
    }
    private void ClickedNo()
    {
        DialogClose();
    }

    private async Task ClickedYes()
    {
        _isLoadingDialog = true;

        if (CreateData.AdmissionId == Guid.Empty)
        {
            _snackbar.AddError("Please Add Admission!!");
            _isLoadingDialog = false;
            StateHasChanged();
            return;
        }

        var fileName = string.Format("LastExam_{0}{1}", _user.GetEmployeeId(), Path.GetExtension(FileUploadLastExam.FileName)).ToFormatFileName();
        var resDocument = await UploadDocument(FileUploadLastExam, DocumentType.Graduate, fileName);
        if (resDocument.Item2 != Guid.Empty)
        {
            DocumentIdLastExam = resDocument.Item2;
        }

        fileName = string.Format("Graduation_{0}{1}", _user.GetEmployeeId(), Path.GetExtension(FileUploadGraduation.FileName)).ToFormatFileName();
        resDocument = await UploadDocument(FileUploadGraduation, DocumentType.Graduate, fileName);
        if (resDocument.Item2 != Guid.Empty)
        {
            DocumentIdGraduation = resDocument.Item2;
        }

        fileName = string.Format("Transcript_{0}{1}", _user.GetEmployeeId(), Path.GetExtension(FileUploadTranscript.FileName)).ToFormatFileName();
        resDocument = await UploadDocument(FileUploadTranscript, DocumentType.Graduate, fileName);
        if (resDocument.Item2 != Guid.Empty)
        {
            DocumentIdTranscript = resDocument.Item2;
        }

        fileName = string.Format("Completion_{0}{1}", _user.GetEmployeeId(), Path.GetExtension(FileUploadCompletion.FileName)).ToFormatFileName();
        resDocument = await UploadDocument(FileUploadCompletion, DocumentType.Graduate, fileName);
        if (resDocument.Item2 != Guid.Empty)
        {
            DocumentIdCompletion = resDocument.Item2;
        }

        fileName = string.Format("CourseFinal_{0}{1}", _user.GetEmployeeId(), Path.GetExtension(FileUploadCourseFinal.FileName)).ToFormatFileName();
        resDocument = await UploadDocument(FileUploadCourseFinal, DocumentType.Graduate, fileName);
        if (resDocument.Item2 != Guid.Empty)
        {
            DocumentIdCourseFinal = resDocument.Item2;
        }

        #region Create
        var createData = new CreateGraduateRequest
        {
            AdmissionId = CreateData.AdmissionId,
            LastExamDocumentId = DocumentIdLastExam,
            GraduationDocumentId = DocumentIdGraduation,
            TranscriptDocumentId = DocumentIdTranscript,
            CompletionDocumentId = DocumentIdCompletion,
            CourseFinalDocumentId = DocumentIdCourseFinal,
            GpaFinalScore = CreateData.GpaFinalScore

        };
        var createDataResponse = await _studyMonitoringGraduateService.CreateGraduateAsync(createData);
        if (createDataResponse.Error is not null)
        {
            var result = string.Empty;
            for (var i = 0; i < createDataResponse.Error.Details.Count; i++)
            {
                result = result + createDataResponse.Error.Details[i] + ",";
            }

            _snackbar.AddError("Applicant - Error : " + result);
            _isLoadingDialog = false;
            StateHasChanged();
            return;
        }

        _snackbar.AddSuccess("Success add data");
        #endregion

        _isLoadingDialog = false;
        await SetDataOnLoadAsync();
        StateHasChanged();
        DialogClose();
    }

    protected void OnRemoveUploaderLastExam(RemovingEventArgs args)
    {
        FileUploadLastExam = new();
        args.PostRawFile = false;
    }
    private async Task ValueChangeUploaderLastExam(UploadChangeEventArgs args)
    {
        FileUploadLastExam.FileName = args.Files.First().File.Name;
        FileUploadLastExam.FileSize = args.Files.First().File.Size;
        FileUploadLastExam.FileContentType = args.Files.First().File.ContentType;
        FileUploadLastExam.File = args.Files.First().File;

        await Task.CompletedTask;
    }
    protected void OnRemoveUploaderGraduation(RemovingEventArgs args)
    {
        FileUploadGraduation = new();
        args.PostRawFile = false;
    }
    private async Task ValueChangeUploaderGraduation(UploadChangeEventArgs args)
    {
        FileUploadGraduation.FileName = args.Files.First().File.Name;
        FileUploadGraduation.FileSize = args.Files.First().File.Size;
        FileUploadGraduation.FileContentType = args.Files.First().File.ContentType;
        FileUploadGraduation.File = args.Files.First().File;

        await Task.CompletedTask;
    }
    protected void OnRemoveUploaderTranscript(RemovingEventArgs args)
    {
        FileUploadTranscript = new();
        args.PostRawFile = false;
    }
    private async Task ValueChangeUploaderTranscript(UploadChangeEventArgs args)
    {
        FileUploadTranscript.FileName = args.Files.First().File.Name;
        FileUploadTranscript.FileSize = args.Files.First().File.Size;
        FileUploadTranscript.FileContentType = args.Files.First().File.ContentType;
        FileUploadTranscript.File = args.Files.First().File;

        await Task.CompletedTask;
    }
    protected void OnRemoveUploaderCompletion(RemovingEventArgs args)
    {
        FileUploadCompletion = new();
        args.PostRawFile = false;
    }
    private async Task ValueChangeUploaderCompletion(UploadChangeEventArgs args)
    {
        FileUploadCompletion.FileName = args.Files.First().File.Name;
        FileUploadCompletion.FileSize = args.Files.First().File.Size;
        FileUploadCompletion.FileContentType = args.Files.First().File.ContentType;
        FileUploadCompletion.File = args.Files.First().File;

        await Task.CompletedTask;
    }
    protected void OnRemoveUploaderCourseFinal(RemovingEventArgs args)
    {
        FileUploadCourseFinal = new();
        args.PostRawFile = false;
    }
    private async Task ValueChangeUploaderCourseFinal(UploadChangeEventArgs args)
    {
        FileUploadCourseFinal.FileName = args.Files.First().File.Name;
        FileUploadCourseFinal.FileSize = args.Files.First().File.Size;
        FileUploadCourseFinal.FileContentType = args.Files.First().File.ContentType;
        FileUploadCourseFinal.File = args.Files.First().File;

        await Task.CompletedTask;
    }

    private async Task<Tuple<string, Guid>> UploadDocument(FileUploadResponse fileData, DocumentType docType, string fileName)
    {
        var memoryStream = new MemoryStream();
        await fileData.File.OpenReadStream(_maximumFileSizeInBytes).CopyToAsync(memoryStream);
        memoryStream.Position = 0; // Reset the stream position

        var formFile = new FormFile(memoryStream, 0, memoryStream.Length, fileName, fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = fileData.FileContentType
        };

        var requestFile = new CreateDocumentRequest
        {
            DocumentType = docType,
            File = formFile
        };

        var resultUpload = await _documentService.CreateDocumentAsync(requestFile);
        if (resultUpload.Error is not null)
        {
            return new Tuple<string, Guid>(string.Join(", ", resultUpload.Error.Details.ToList()), Guid.Empty);
        }

        return new Tuple<string, Guid>(string.Empty, resultUpload.Result!.DocumentId);
    }
    #endregion
}
