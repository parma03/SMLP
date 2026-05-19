using System.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Smlp.Base.Scholarships.Enums;
using Smlp.Base.Selections.Enums;
using Smlp.Bsui.Common.Constants;
using Smlp.Bsui.Common.Extensions;
using Smlp.Bsui.Features.Scholarships.ScholarshipPlan.Constants;
using Smlp.Shared.Common.Constants;
using Smlp.Shared.Common.Extensions;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Documents.Commands.CreateDocument;
using Smlp.Shared.Documents.Queries.GetDocumentFile;
using Smlp.Shared.Scholarships.ScholarshipPlan.Commands.Update;
using Smlp.Shared.Scholarships.ScholarshipPlan.Queries;
using Smlp.Shared.Services.Authorization.Constants;
using Syncfusion.Blazor.DocumentEditor;
using Syncfusion.Blazor.Grids;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocIORenderer;
using Syncfusion.XlsIO;

namespace Smlp.Bsui.Features.Scholarships.ScholarshipPlan.Draft.Components;

public partial class DraftScholarshipPlan
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;
    protected ClaimsPrincipal _user = default!;
    protected ErrorResponse? _error;
    protected bool _isLoading = false;
    protected bool _isLoadingData = false;
    protected bool _isDrafting = false;
    protected int _companyGroupId = 9999;
    private SfGrid<GetScholarshipPlanResponse> Grid { get; set; } = new();
    protected List<GetScholarshipPlanResponse> _scholarshipPlans = new();
    protected GetScholarshipPlanResponse DataPlan { get; set; } = new();
    private List<GetScholarshipPlanResponse> _tampscholarshipPlan = new();
    protected bool _validationDataCheck = false;
    private List<int> SelectIndex { get; set; } = new();
    private bool VisiblePopup { get; set; }
    private bool VisiblePopupSubmit { get; set; }

    protected SfDocumentEditorContainer _containerDocument = default!;
    protected DocumentEditorSettingsModel _settingDocument = new() { ShowRuler = true };
    protected string[] _toolbarItems = DocumentEditorExtensions.ToolBarItems();
    protected Guid _documentDraftId = Guid.Empty;
    protected string _contentButton = "Update";
    protected bool _disableButton = false;
    protected bool _disableButtonDocument = false;
    protected bool _disableButtonReject = false;
    protected bool _disableButtonDownload = false;
    internal bool ReadOnly { get; set; } = true;
    internal bool Toolbar { get; set; } = true;
    internal bool ShowProperties { get; set; } = false;

    protected override async Task OnInitializedAsync()
    {
        _user = (await AuthenticationStateTask).User;
        if (_user.Identity!.IsAuthenticated)
        {
            var val = _user.Claims.Where(x => x.Type.ToLower().Trim() == "permission" && x.Value.ToLower().Trim().Contains(Permissions.Psmp_ScholarshipPlan_Crud)).ToList();
            if (val.Count() == 0)
            {
                _navigationManager.NavigateTo(RouteFor.Scholarships, true);
            }

            _isLoading = true;
            await SetDataOnLoadAsync();
            _isLoading = false;
        }
    }

    private async Task SetDataOnLoadAsync()
    {
        DialogClose();
        DialogCloseSubmit();
        _isDrafting = false;
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
            _scholarshipPlans = scholarshipPlan.Result!.Items.Where(x => x.Status is not Base.Scholarships.Enums.ScholarshipPlanStatus.Approved and not Base.Scholarships.Enums.ScholarshipPlanStatus.Rejected).ToList();
            var planDraft = _scholarshipPlans.Where(x => x.Status == ScholarshipPlanStatus.WaitingForApproval && x.DocumentDraftId != null).FirstOrDefault();
            if (planDraft is not null)
            {
                _isDrafting = true;
                _documentDraftId = (Guid)planDraft.DocumentDraftId;
            }

            StateHasChanged();
        }
        else
        {
            _error = scholarshipPlan.Error;
            _isLoadingData = false;
        }

        _isLoadingData = false;
    }

    #region Action
    public void OnCommandClicked(CommandClickEventArgs<GetScholarshipPlanResponse> args)
    {
        DataPlan = args.RowData;
        VisiblePopup = false;
        if (args.CommandColumn.Type == CommandButtonType.Edit)
        {
            VisiblePopup = true;
        }
    }

    public async Task OnClickSubmit()
    {
        if (_tampscholarshipPlan.Count() == 0)
        {
            _snackbar.AddError($"Please select plan for submit!!");
            return;
        }

        DialogOpenSubmit();
        await Task.CompletedTask;
    }
    #endregion

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
        var response = await _scholarshipPlanService.UpdateScholarshipPlanStatusAsync(new UpdateScholarshipPlanStatusRequest { Id = DataPlan.Id, Status = Base.Scholarships.Enums.ScholarshipPlanStatus.Rejected });
        if (response.Error is not null)
        {
            _snackbar.AddError(response.Error.Detail);
        }
        else
        {
            _snackbar.AddSuccess($"Success reject Scholarship Plan, Code : {response.Result!.Description} with ID {response.Result!.Id}");
            VisiblePopup = false;
            await SetDataOnLoadAsync();
            StateHasChanged();
            await Grid.Refresh();
        }
    }

    #endregion Popup Event

    #region Popup Event Submit

    private void DialogOpenSubmit()
    {
        VisiblePopupSubmit = true;
    }

    private void DialogCloseSubmit()
    {
        VisiblePopupSubmit = false;
    }

    private void ClickedNoSubmit()
    {
        VisiblePopupSubmit = false;
    }

    private async void ClickedYesSubmit()
    {
        VisiblePopupSubmit = false;
        await OnLoadDataGenerate();
        await OnUploadDocument();

        if (_documentDraftId == Guid.Empty)
        {
            _snackbar.AddWarning("Error upload file");
            StateHasChanged();
            return;
        }

        foreach (var item in _tampscholarshipPlan)
        {
            var param = new UpdateScholarshipPlanStatusRequest
            {
                Id = item.Id,
                DocumentDraftId = _documentDraftId,
                Status = ScholarshipPlanStatus.WaitingForApproval
            };

            await _scholarshipPlanService.UpdateScholarshipPlanStatusAsync(param);
        }

        await SetDataOnLoadAsync();
        await OnLoadExist();
        _snackbar.AddSuccess("Success generate document draft!");

        StateHasChanged();
        await Task.CompletedTask;
    }

    #endregion Popup EventSubmit

    #region Selection
    public async Task GetSelectedRecords(RowSelectEventArgs<GetScholarshipPlanResponse> args)
    {
        if (!_tampscholarshipPlan.Any(x => x.Id == args.Data.Id))
        {
            _validationDataCheck = true;
            _tampscholarshipPlan = args.Datas;
            _tampscholarshipPlan = _tampscholarshipPlan.Where(x => x.Status == ScholarshipPlanStatus.Requested).ToList();
            var source = await Grid.GetCurrentViewRecordsAsync();
            var indexNum = 0;
            SelectIndex = new List<int>();
            foreach (var record in source)
            {
                if (_tampscholarshipPlan.Any(x => x.Id == record.Id))
                {
                    SelectIndex.Add(indexNum);
                }

                indexNum++;
            }

            await Grid.SelectRowsAsync(SelectIndex.ToArray());
        }
    }
    public async Task CelearSelectedRecords(RowDeselectEventArgs<GetScholarshipPlanResponse> args)
    {
        if (!_validationDataCheck)
        {
            var a = _tampscholarshipPlan;
            var datas = args.Datas;
            _tampscholarshipPlan = _tampscholarshipPlan
                .Where(x => !datas.Select(x => x.Id).Contains(x.Id))
                .Where(x => x.Status == ScholarshipPlanStatus.Requested).ToList();
        }
        else
        {
            _validationDataCheck = false;
        }
    }
    #endregion

    #region Export word
    private async Task OnClickDownloadWordAsync()
    {
        if (_documentDraftId == Guid.Empty)
        {
            _snackbar.AddError("Please generate draft document!!");
            return;
        }

        var doc = await _documentService.GetDocumentFileByDocumentTypeAsync(new GetDocumentFileRequest { DocumentId = _documentDraftId, DocumentType = DocumentType.DraftScholarshipPlan });
        if (doc.Error is not null)
        {
            StateHasChanged();
            var result = string.Join(", ", doc.Error.Details.ToList());
            _snackbar.AddError("Error : " + result);
            return;
        }

        var dataDoc = doc.Result;
        await _jsRuntime.InvokeVoidAsync
            (JavaScriptIdentifierFor.DownloadFile,
            dataDoc!.FileName,
            $"{ContentTypes.ApplicationWord}",
            dataDoc!.Content);
    }

    private async Task OnClickDownloadPdfAsync()
    {
        await ExportAsPdf();
        StateHasChanged();
        await Task.CompletedTask;
    }
    public async Task ExportAsPdf()
    {
        if (_containerDocument == null)
        {
            _snackbar.AddError("Can not download PDF, document under development!!");
            return;
        }

        var editor = _containerDocument.DocumentEditor;
        var base64Data = await editor.SaveAsBlobAsync(FormatType.Docx);
        var data = Convert.FromBase64String(base64Data);
        base64Data = null;
        Stream stream = new MemoryStream(data);
        data = null;
        var doc = new Syncfusion.DocIO.DLS.WordDocument(stream, Syncfusion.DocIO.FormatType.Docx);
        var render = new DocIORenderer();
        var pdfDocument = render.ConvertToPDF(doc);
        var downloadStream = new MemoryStream();
        pdfDocument.Save(downloadStream);
        pdfDocument.Close();
        var content = downloadStream.ToArray();
        //download document
        await _jsRuntime.InvokeVoidAsync
            (JavaScriptIdentifierFor.DownloadFile,
            $"DraftScholarshipPlan_{DateTime.Now:yyyyMMdd_HHmmss}.pdf",
            $"{ContentTypes.ApplicationPdf}",
            content);
        downloadStream.Close();
        stream.Close();
    }

    private DataTable DataTableDraft()
    {
        var table = new DataTable();

        table.Columns.Add("Holding/Sub Holding", typeof(string));
        table.Columns.Add("Code", typeof(string));
        table.Columns.Add("BUSINESS ISSUES (5 yrs)", typeof(string));
        table.Columns.Add("REFERENCES OF BUSINESS ISSUES", typeof(string));
        table.Columns.Add("PERFORMANCE ISSUES", typeof(string));
        table.Columns.Add("COMPETENCY ISSUES", typeof(string));
        table.Columns.Add("PERFORMANCE IMPACT (a)", typeof(string));
        table.Columns.Add("Competency Gap (b)", typeof(string));
        table.Columns.Add("Resolution Impact (a x b)", typeof(string));
        table.Columns.Add("Priorities", typeof(string));
        table.Columns.Add("Major", typeof(string));
        table.Columns.Add("Faculty", typeof(string));
        table.Columns.Add("PREFERENCE UNIV (IF ANY)", typeof(string));
        table.Columns.Add("OVERSEAS/DOMESTIC ", typeof(string));
        table.Columns.Add("REQUIREMENT (IF ANY)", typeof(string));
        table.Columns.Add("TARGETED POSITIONS", typeof(string));
        table.Columns.Add("JOB PROJECTION", typeof(string));
        table.Columns.Add("NEEDED", typeof(string));

        if (_scholarshipPlans is not null)
        {
            foreach (var item in _scholarshipPlans.Where(x => x.Status == ScholarshipPlanStatus.WaitingForApproval))
            {
                table.Rows.Add(
                    item.CompanyGroupName,
                    item.Code,
                    item.BusinessIssue,
                    item.BusinessIssueReferences,
                    item.PerformanceIssue,
                    item.CompetencyIssue,
                    "?",
                    "?",
                    "?",
                    "?",
                    item.Major,
                    item.Faculty,
                    item.UniversityReference,
                    item.UniversityLocation.GetDescription(),
                    "?",
                    "?",
                    "?",
                    item.AwardeeQuota);
            }
        }

        return table;
    }

    protected async Task OnClickDownloadExcelAsync()
    {
        using var excelEngine = new ExcelEngine();
        var application = excelEngine.Excel;
        application.DefaultVersion = ExcelVersion.Xlsx;

        //Create a workbook
        var workbook = application.Workbooks.Create(1);
        var worksheet = workbook.Worksheets[0];
        var table = DataTableDraft();

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
            $"DraftScholarshipPlan_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
            $"{ContentTypes.ApplicationOpenXmlExcel}",
            content);
    }
    #endregion

    #region Processing Word
    private async Task OnLoadDataGenerate()
    {
        var request = new GetDocumentFileRequest
        {
            DocumentId = Guid.Parse(_documentOptions.Value.DocumentDraftId.ScholarshipPlan.Approval),
            DocumentType = DocumentType.Template
        };

        var getDraftDocument = await _documentService.GetDocumentFileByDocumentTypeAsync(request);
        if (getDraftDocument.Error is null && _containerDocument != null)
        {
            var document = new Syncfusion.DocIO.DLS.WordDocument();
            Stream stream = new MemoryStream(getDraftDocument.Result!.Content);
            document.Open(stream, Syncfusion.DocIO.FormatType.Docx);
            stream.Dispose();

            var mainTable = document.Sections[0].Tables[0].Clone() as WTable;
            var section = document.Sections[0];
            var table = section.Tables[0];

            var currentRow = 1;
            var idxDtl = 1;
            foreach (var dtl in _tampscholarshipPlan)
            {
                table.Rows[currentRow].Cells[0].AddParagraph().AppendText(idxDtl.ToString());
                table.Rows[currentRow].Cells[1].AddParagraph().AppendText(dtl.CompanyGroupName);
                table.Rows[currentRow].Cells[2].AddParagraph().AppendText(dtl.Code);
                table.Rows[currentRow].Cells[3].AddParagraph().AppendText(dtl.BusinessIssue);
                table.Rows[currentRow].Cells[4].AddParagraph().AppendText(dtl.BusinessIssueReferences);
                table.Rows[currentRow].Cells[5].AddParagraph().AppendText(dtl.PerformanceIssue);
                table.Rows[currentRow].Cells[6].AddParagraph().AppendText(dtl.CompetencyIssue);
                table.Rows[currentRow].Cells[7].AddParagraph().AppendText("");
                table.Rows[currentRow].Cells[8].AddParagraph().AppendText("");
                table.Rows[currentRow].Cells[9].AddParagraph().AppendText("");
                table.Rows[currentRow].Cells[10].AddParagraph().AppendText("");
                table.Rows[currentRow].Cells[11].AddParagraph().AppendText(dtl.Major);
                table.Rows[currentRow].Cells[12].AddParagraph().AppendText(dtl.Faculty);
                table.Rows[currentRow].Cells[13].AddParagraph().AppendText(dtl.UniversityReference);
                table.Rows[currentRow].Cells[14].AddParagraph().AppendText(dtl.UniversityLocation.GetDescription());
                table.Rows[currentRow].Cells[15].AddParagraph().AppendText("");
                table.Rows[currentRow].Cells[16].AddParagraph().AppendText("");
                table.Rows[currentRow].Cells[17].AddParagraph().AppendText("");
                table.Rows[currentRow].Cells[18].AddParagraph().AppendText(dtl.AwardeeQuota.ToString());

                if (idxDtl < _tampscholarshipPlan.Count())
                {
                    table.AddRow(true);
                }

                currentRow++;
                idxDtl++;
            }

            var saveStream = new MemoryStream();
            document.Save(saveStream, Syncfusion.DocIO.FormatType.Docx);
            document.Close();
            var editor = _containerDocument.DocumentEditor;
            await editor.OpenAsync(saveStream, ImportFormatType.Docx);
            StateHasChanged();
        }

        await Task.CompletedTask;
    }

    private async Task BtnActionDocument()
    {
        ReadOnly = !ReadOnly;
        Toolbar = !Toolbar;
        ShowProperties = !ShowProperties;
        await Task.CompletedTask;
    }

    private async Task OnClickUpdateAsync()
    {
        _contentButton = "Save";
        await BtnActionDocument();
        await OnLoadDocumentDraftAndMaster();
        _disableButtonDocument = true;
        _disableButtonDownload = true;
        await Task.CompletedTask;
    }

    private async Task OnClickSaveAsync()
    {
        _contentButton = "Update";
        await BtnActionDocument();
        await OnUploadDocument();

        if (_documentDraftId == Guid.Empty)
        {
            _snackbar.AddWarning("Error upload file");
            StateHasChanged();
            return;
        }

        // only update status waiting approval
        foreach (var item in _scholarshipPlans.Where(x => x.Status == ScholarshipPlanStatus.WaitingForApproval))
        {
            var param = new UpdateScholarshipPlanStatusRequest
            {
                Id = item.Id,
                DocumentDraftId = _documentDraftId,
                Status = ScholarshipPlanStatus.WaitingForApproval
            };

            await _scholarshipPlanService.UpdateScholarshipPlanStatusAsync(param);
        }

        _snackbar.AddSuccess($"Success update data!");
        await SetDataOnLoadAsync();
        await OnLoadExist();
        _disableButtonDocument = _documentDraftId == Guid.Empty && _disableButton;
        _disableButtonDownload = false;
        await Task.CompletedTask;
    }

    private async Task OnUploadDocument()
    {
        var timeStamp = DateTime.Now.ToString("ddMMyyyyHHmmss");
        var filename = $"DraftScholashipPlan_{timeStamp}.docx";
        if (_containerDocument != null)
        {
            var editor = _containerDocument.DocumentEditor;
            var base64Data = await editor.SaveAsBlobAsync(FormatType.Docx);
            var data = Convert.FromBase64String(base64Data);
            var stream = new MemoryStream(data);

            var formFile = new FormFile(stream, 0, stream.Length, filename, filename)
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
            };

            var requestFile = new CreateDocumentRequest
            {
                DocumentType = DocumentType.DraftScholarshipPlan,
                File = formFile
            };

            var resultUpload = await _documentService.CreateDocumentAsync(requestFile);
            if (resultUpload.Error is null)
            {
                _documentDraftId = resultUpload.Result!.DocumentId;
            }
        }

        await Task.CompletedTask;
    }

    private async Task OnLoadDocumentDraftAndMaster()
    {
        if (_containerDocument != null)
        {
            if (_documentDraftId != Guid.Empty)
            {
                await OnLoadDraft();
            }
            else
            {
                await OnLoadMaster();
            }
        }

        await Task.CompletedTask;
    }

    private async Task OnLoadExist()
    {
        var request = new GetDocumentFileRequest
        {
            DocumentId = _documentDraftId,
            DocumentType = DocumentType.DraftScholarshipPlan
        };

        var getDraftDocument = await _documentService.GetDocumentFileByDocumentTypeAsync(request);
        if (getDraftDocument.Error is null && _containerDocument != null)
        {
            var document = new Syncfusion.DocIO.DLS.WordDocument();
            Stream stream = new MemoryStream(getDraftDocument.Result!.Content);
            document.Open(stream, Syncfusion.DocIO.FormatType.Docx);
            stream.Dispose();

            var saveStream = new MemoryStream();
            document.Save(saveStream, Syncfusion.DocIO.FormatType.Docx);
            document.Close();
            var editor = _containerDocument.DocumentEditor;
            await editor.OpenAsync(saveStream, ImportFormatType.Docx);
            StateHasChanged();
        }

        await Task.CompletedTask;
    }

    private async Task OnLoadDraft()
    {
        var request = new GetDocumentFileRequest
        {
            DocumentId = _documentDraftId,
            DocumentType = DocumentType.DraftScholarshipPlan
        };

        var getDraftDocument = await _documentService.GetDocumentFileByDocumentTypeAsync(request);
        if (getDraftDocument.Error is null && _containerDocument != null)
        {
            Stream stream = new MemoryStream(getDraftDocument.Result!.Content);
            var editor = _containerDocument.DocumentEditor;
            await editor.OpenAsync(stream, ImportFormatType.Docx);
        }

        StateHasChanged();
        await Task.CompletedTask;
    }

    private async Task OnLoadMaster()
    {
        var request = new GetDocumentFileRequest
        {
            DocumentId = Guid.Parse(_documentOptions.Value.DocumentDraftId.ScholarshipPlan.Approval),
            DocumentType = DocumentType.Template
        };

        var getDraftDocument = await _documentService.GetDocumentFileByDocumentTypeAsync(request);
        if (getDraftDocument.Error is null && _containerDocument != null)
        {
            Stream stream = new MemoryStream(getDraftDocument.Result!.Content);
            var editor = _containerDocument.DocumentEditor;
            await editor.OpenAsync(stream, ImportFormatType.Docx);
        }

        StateHasChanged();
        await Task.CompletedTask;
    }

    private async Task OnLoadDocument()
    {
        if (_containerDocument != null)
        {
            _disableButtonDocument = _documentDraftId == Guid.Empty && !_disableButton;
            if (DataPlan.Status == ScholarshipPlanStatus.Approved)
            {
                _disableButtonDocument = true;
            }

            if (_documentDraftId != Guid.Empty)
            {
                await OnLoadExist();
            }
            else if (DataPlan.Status != ScholarshipPlanStatus.Approved)
            {
                await OnLoadMaster();
            }
            else
            {
                _disableButtonDownload = true;
            }
        }

        await Task.CompletedTask;
    }

    public async void OnCreated()
    {
        Toolbar = false;
        await OnLoadDocument();
    }
    #endregion
}
