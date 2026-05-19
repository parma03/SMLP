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
using Psmp.Base.Studies.Enums;
using Psmp.Bsui.Common.Constants;
using Psmp.Bsui.Common.Extensions;
using Psmp.Shared.Common.Constants;
using Psmp.Shared.Common.Extensions;
using Psmp.Shared.Common.Responses;
using Psmp.Shared.Documents.Queries.GetDocumentFile;
using Psmp.Shared.Masters.Queries;
using Psmp.Shared.Scholarships.ScholarshipPosting.Queries;
using Psmp.Shared.StudyMonitorings.AfterProgram.Commands.Create;
using Psmp.Shared.StudyMonitorings.AfterProgram.Commands.Update;
using Psmp.Shared.StudyMonitorings.AfterProgram.Queries;
using Psmp.Shared.Universities.Admission.Queries;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Inputs;
using Syncfusion.XlsIO;

namespace Psmp.Bsui.Features.StudyMonitoring.AfterProgram;

public partial class Index
{
    #region Protected
    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;
    protected ClaimsPrincipal _user = default!;
    protected string _displayName = default!;
    protected ErrorResponse? _error;
    protected bool _isLoading = false;
    protected bool _isLoadingDialog = false;
    protected List<GetAfterProgramQueryResponse> _dataGrid = new();
    public List<DropdownItemResponse> _dropdownStatus = new();
    protected Guid _documentDeleteId = Guid.Empty;
    #endregion

    #region Card
    private int _countAll = 0;
    private string _countDone = "";
    private string _countNotStart = "";
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

    #region Upload Download
    protected long _maximumFileSizeInBytes = long.MaxValue;
    protected DataTable _tableExcel = new();
    protected List<GetAfterProgramQueryResponse> _workbookExcel = new();
    protected SfUploader? _uploadExcel;
    #endregion

    #region Add Data
    private string? ActionType { get; set; }
    private bool IsEnabled { get; set; }
    private bool IsEnabledAdmission { get; set; }
    private bool VisiblePopup { get; set; }
    private string TitlePopup { get; set; } = "Add";
    protected CreateAfterProgramRequest CreateData { get; set; } = new();
    public List<GetAdmissionQueryResponse> _dropdownAdmission = new();
    public List<DropdownEnumResponse<PresentationStatus>> _dropdownEnumStatus = new();
    #endregion

    #region Confirm
    private string TitlePopupConfirm { get; set; } = "";
    private bool VisiblePopupConfirm { get; set; } = false;
    #endregion

    protected override async Task OnInitializedAsync()
    {
        _user = (await AuthenticationStateTask).User;
        _displayName = _user.GetDisplayName() ?? string.Empty;
        _companyCode = _user.GetCompanyCode();
        _isLoading = true;
        await SetDataOnLoadAsync();

        await GetDropdownCategories();
        await GetDropdownCompanyGroup();
        await GetDropdownCompany();
        await GetDropdownDirectorate();
        await GetDropdownScholarshipCodes();

        await GetDropdownStatus();
        _isLoading = false;
        await Task.CompletedTask;
    }

    #region DataGrid
    private async Task SetDataOnLoadAsync()
    {
        var data = await _studyMonitoringAfterProgramService.GetAfterProgramAsync();
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
            var done = 0;
            var notStart = 0;
            done = _dataGrid.Where(x => x.Status == PresentationStatus.Done).Count();
            notStart = _dataGrid.Where(x => x.Status == PresentationStatus.NotStarted).Count();
            _countDone = string.Format(@"{0}/{1}", done, _countAll);
            _countNotStart = string.Format(@"{0}/{1}", notStart, _countAll);
        }
        else
        {
            _error = data.Error;
        }

        StateHasChanged();
        await Task.CompletedTask;
    }
    #endregion

    #region Dropdown
    private async Task GetDropdownStatus()
    {
        _dropdownStatus = EnumExtentions.ToDropdownItem<PresentationStatus>();
        _dropdownStatus = _dropdownStatus.OrderBy(x => x.Id).ToList();

        _dropdownEnumStatus = EnumExtentions.ToDropdownEnum<PresentationStatus>();
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

    #region Command Event
    public void RowBound(RowDataBoundEventArgs<GetAfterProgramQueryResponse> args)
    {
        if (args.Data.Status == PresentationStatus.Done)
        {
            args.Row.AddClass(new string[] { "e-removeDeletecommand" });
        }
    }

    public async Task OnCommandClicked(CommandClickEventArgs<GetAfterProgramQueryResponse> args)
    {
        await OnLoadAdmission();
        if (args.CommandColumn.Type == CommandButtonType.Delete)
        {
            _documentDeleteId = args.RowData.Id;
            VisiblePopupConfirm = true;
            TitlePopupConfirm = "<p style='padding:20px;'>Are you sure want to delete this data?</p>";
        }
        else if (args.CommandColumn.Type == CommandButtonType.Edit)
        {
            ActionType = "edit";
            TitlePopup = "Edit";
            IsEnabled = true;
            IsEnabledAdmission = false;
            VisiblePopup = true;
        }
        else if (args.CommandColumn.Type == CommandButtonType.None && args.CommandColumn.ButtonOption.Content.Equals("DownloadFile"))
        {
            if (args.RowData.PresentationDocumentId == Guid.Empty || args.RowData.PresentationDocumentId is null)
            {
                _snackbar.AddError("Error : Document not found!!");
                return;
            }

            await OnClickDownload((Guid)args.RowData.PresentationDocumentId, DocumentType.AfterProgram);
            return;

        }
        else
        {
            TitlePopup = "View";
            IsEnabled = false;
            IsEnabledAdmission = false;
            VisiblePopup = true;
        }

        CreateData.Id = args.RowData.Id;
        CreateData.AdmissionId = args.RowData.AdmissionId;
        var dataAdmission = _dropdownAdmission.Where(x => x.Id == CreateData.AdmissionId).FirstOrDefault();
        if (dataAdmission != null)
        {
            CreateData.Company = dataAdmission.CompanyName;
            CreateData.EmployeeNumber = dataAdmission.EmployeeNumber;
            CreateData.Email = dataAdmission.Email;
        }

        CreateData.PresentationLink = args.RowData.PresentationLink;
        CreateData.PresentationSchedule = args.RowData.PresentationSchedule;
        CreateData.PresentationScheduleTime = args.RowData.PresentationScheduleTime;
        CreateData.Status = args.RowData.Status;

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
        table.Columns.Add("Presentasion Schedule", typeof(string));
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
                    item.PresentationSchedule,
                    item.Status);
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
        commentF.Text = "Presentasion Schedule";
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
            string.IsNullOrEmpty(fileName) ? $"AfterProgram_{DateTime.Now:yyyyMMdd}.xlsx" : fileName,
            $"{ContentTypes.ApplicationOpenXmlExcel}",
            content);

    }
    #endregion

    #region Add Data
    private async Task OnClickAddAsync()
    {
        TitlePopup = "Add";
        ActionType = "add";
        IsEnabled = true;
        IsEnabledAdmission = true;
        CreateData = new CreateAfterProgramRequest { PresentationSchedule = DateOnly.FromDateTime(DateTime.Now), PresentationScheduleTime = TimeOnly.FromDateTime(DateTime.Now) };
        await OnLoadAdmission();
        DialogOpen();
    }

    private async Task OnLoadAdmission()
    {
        var data = await _universityAdmissionService.GetUniversityAdmissionAsync();
        if (data.Error is null)
        {
            _dropdownAdmission = data.Result!.Items.ToList();
            foreach (var item in _dropdownAdmission)
            {
                item.Name = string.Format("{0} ({1})", item.Name, item.University);
            }
        }

        await GetDropdownStatus();
    }
    private void DialogOpen()
    {
        VisiblePopup = true;
    }

    private void DialogClose()
    {
        ActionType = "";
        VisiblePopup = false;
        CreateData = new CreateAfterProgramRequest { PresentationSchedule = DateOnly.FromDateTime(DateTime.Now), PresentationScheduleTime = TimeOnly.FromDateTime(DateTime.Now) };
    }
    private void ClickedNo()
    {
        DialogClose();
    }
    private async Task ClickedYes()
    {
        if (ActionType == "add")
        {
            #region Create
            var createData = new CreateAfterProgramRequest
            {
                AdmissionId = CreateData.AdmissionId,
                PresentationLink = CreateData.PresentationLink,
                PresentationSchedule = CreateData.PresentationSchedule,
                PresentationScheduleTime = CreateData.PresentationScheduleTime,
                Status = CreateData.Status
            };
            var createDataResponse = await _studyMonitoringAfterProgramService.CreateAfterProgramAsync(createData);
            if (createDataResponse.Error is not null)
            {
                var result = string.Join(", ", createDataResponse.Error.Details.ToList());
                _snackbar.AddError("Error : " + result);
                _isLoadingDialog = false;
                StateHasChanged();
                return;
            }

            _snackbar.AddSuccess("Success add data");
            #endregion
        }
        else if (ActionType == "edit")
        {
            #region update
            var updateData = new UpdateAfterProgramRequest
            {
                Id = CreateData.Id,
                PresentationLink = CreateData.PresentationLink,
                PresentationSchedule = CreateData.PresentationSchedule,
                PresentationScheduleTime = CreateData.PresentationScheduleTime,
                Status = CreateData.Status
            };
            var updateDataResponse = await _studyMonitoringAfterProgramService.UpdateAfterProgramAsync(updateData);
            if (updateDataResponse.Error is not null)
            {
                var result = string.Join(", ", updateDataResponse.Error.Details.ToList());
                _snackbar.AddError("Error : " + result);
                _isLoadingDialog = false;
                StateHasChanged();
                return;
            }

            _snackbar.AddSuccess("Success update data");
            #endregion
        }

        await SetDataOnLoadAsync();
        StateHasChanged();
        DialogClose();
    }
    #endregion

    #region Modal Confirm
    private void DialogConfirmOpen()
    {
        VisiblePopupConfirm = true;
    }

    private void DialogConfirmClose()
    {
        VisiblePopupConfirm = false;
    }

    private void ClickedNoConfirm()
    {
        VisiblePopupConfirm = false;
    }

    private async void ClickedYesConfirm()
    {
        if (_documentDeleteId == Guid.Empty)
        {
            _snackbar.AddError("Please select data");
            return;
        }

        var response = await _studyMonitoringAfterProgramService.DeleteAfterProgramAsync(_documentDeleteId);
        if (response.Error is not null)
        {
            var result = string.Join(", ", response.Error.Details.ToList());
            _snackbar.AddError("Error : " + result);
            _isLoadingDialog = false;
            StateHasChanged();
            return;
        }

        _snackbar.AddSuccess("Success delete data");
        DialogConfirmClose();
        await SetDataOnLoadAsync();
        StateHasChanged();
        await Task.CompletedTask;
    }
    #endregion

    #region Document
    private async Task OnClickDownload(Guid documentId, DocumentType docType)
    {
        var request = new GetDocumentFileRequest
        {
            DocumentId = documentId,
            DocumentType = (DocumentType)docType,
        };

        var response = await _documentService.GetDocumentFileByDocumentTypeAsync(request);

        if (response.Error is not null)
        {
            StateHasChanged();
            var result = string.Join(", ", response.Error.Details.ToList());
            _snackbar.AddError("Error : " + result);
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
}
