using System.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Psmp.Base.Scholarships.Enums;
using Psmp.Base.Selections.Enums;
using Psmp.Bsui.Common.Constants;
using Psmp.Bsui.Common.Extensions;
using Psmp.Shared.Common.Constants;
using Psmp.Shared.Common.Extensions;
using Psmp.Shared.Common.Responses;
using Psmp.Shared.Documents.Commands.CreateDocument;
using Psmp.Shared.Documents.Queries.GetDocumentFile;
using Psmp.Shared.Selections.BeritaAcara.Commands.Create;
using Psmp.Shared.Selections.BeritaAcara.Commands.Update;
using Psmp.Shared.Selections.BeritaAcara.Queries;
using Psmp.Shared.Selections.CreateSK.Commands.Create;
using Psmp.Shared.Selections.SelectionOverviews.Commands.Update;
using Psmp.Shared.Selections.SelectionTalentReviews.Commands.Update;
using Syncfusion.Blazor.DocumentEditor;
using Syncfusion.DocIORenderer;
using Syncfusion.XlsIO;
using RouteFor = Psmp.Bsui.Features.Selections.BeritaAcara.Constants.RouteFor;

namespace Psmp.Bsui.Features.Selections.BeritaAcara.Components;

public partial class DraftingBeritaAcara
{
    [Parameter]
    public string? Type { get; set; }
    [Parameter]
    public int Batch { get; set; }
    [Parameter]
    public int Year { get; set; }

    protected long _maximumFileSizeInBytes;

    protected ErrorResponse? _error;
    protected ErrorResponse? _errorDraft;
    protected bool _isLoadingData = false;
    protected bool _isLoadingDialog = false;
    protected bool _isEnabled = false;

    protected List<GetSelectionApplicantBeritaAcaraByBatchQueryResponse> _selectionApplicantBeritaAcara = new();
    protected List<GetSelectionBeritaAcaraByBatchQueryResponse> _selectionBeritaAcara = new();
    protected GetSelectionBeritaAcaraByBatchQueryResponse _dataItem = new();
    private List<Guid>? PostingIds { get; set; } = new();

    #region Dcoument

    protected SfDocumentEditorContainer _containerDocument = new();
    protected DocumentEditorSettingsModel _settingDocument = new() { ShowRuler = true };
    protected string[] _toolbarItems = DocumentEditorExtensions.ToolBarItems();
    internal bool ReadOnly { get; set; } = true;
    internal bool Toolbar { get; set; } = true;
    internal bool ShowProperties { get; set; } = false;

    protected Guid _documentDraftId = Guid.Empty;
    protected Guid _documentFinalId = Guid.Empty;
    protected string _contentButtonSave = "Update";
    #endregion

    #region Filter
    protected ScholarshipBatch _batch = default!;
    protected int _year = 0;
    #endregion

    #region Model
    protected string _title = "";
    protected bool _isDisabledButtonSubmit = true;
    public FileUploadResponse FileUploads { get; set; } = new();
    #endregion

    protected bool _isVisibleConfirmation = false;
    protected string? _typeConfirmation = "";
    protected string? _titleConfirmation = "";
    protected string? _contentConfirmation = "";

    protected bool _disableButtonSave = false;
    protected bool _disableButtonDocument = true;
    protected bool _disableButtonReject = false;
    protected bool _disableButtonDownload = false;

    protected override async Task OnInitializedAsync()
    {
        _maximumFileSizeInBytes = _documentOptions.Value.MaximumFileSizeInBytes;
        _batch = (ScholarshipBatch)Batch;
        _year = Year;

        await OnLoadData();
        await SetDataOnLoadBeritaAcaraApplicantAsync();
    }

    public async Task OnLoadData()
    {
        var param = new GetSelectionBeritaAcaraFilterByBatchRequest
        {
            SelectionBatch = Convert.ToInt32(_batch),
            Year = _year
        };

        var getBeritaAcara = await _selectionBeritaAcaraService.GetSelectionBeritaAcaraByBatchAsync(param);
        _error = getBeritaAcara.Error;
        if (getBeritaAcara.Error is null && getBeritaAcara.Result is not null)
        {
            var data = getBeritaAcara.Result.Items;
            if (data.Count() > 0)
            {
                foreach (var item in data)
                {
                    PostingIds!.Add(item.ScholarshipPostingId);
                }

                PostingIds = PostingIds!.Distinct().ToList();

                _dataItem = data.OrderBy(x => x.Created).LastOrDefault() ?? new();
                _title = _dataItem!.Title;
                _documentDraftId = _dataItem!.DraftDocumentId ?? Guid.Empty;
                _documentFinalId = _dataItem!.FinalDocumentId ?? Guid.Empty;

                _disableButtonSave = _dataItem.Status switch
                {
                    BeritaAcaraStatus.Completed or BeritaAcaraStatus.ProcessToPSign or BeritaAcaraStatus.ProcessToPOffice => true,
                    _ => false,
                };

                _disableButtonReject = _dataItem.Status switch
                {
                    BeritaAcaraStatus.ProcessToPSign or BeritaAcaraStatus.ProcessToPOffice => false,
                    _ => true,
                };

                if (_documentFinalId != Guid.Empty)
                {
                    _isDisabledButtonSubmit = false;
                    _disableButtonDocument = true;
                    _disableButtonReject = true;
                    _disableButtonSave = true;
                }
            }
            else
            {
                _contentButtonSave = "Save";
                _isEnabled = true;
                _disableButtonDocument = true;
                _disableButtonDownload = true;
                _disableButtonReject = true;
            }
        }
    }

    private async Task SetDataOnLoadBeritaAcaraApplicantAsync()
    {
        _isLoadingData = true;
        var param = new GetSelectionBeritaAcaraFilterByBatchRequest
        {
            SelectionBatch = Convert.ToInt32(_batch),
            Year = _year
        };

        var response = await _selectionBeritaAcaraService.GetSelectionApplicantBeritaAcaraByBatchAsync(param);
        if (response.Error is null)
        {
            _selectionApplicantBeritaAcara = response.Result!.Items.Where(x => x.Status is not ApplicantStatus.Failed and not ApplicantStatus.Canceled).ToList();
            foreach (var item in _selectionApplicantBeritaAcara)
            {
                PostingIds!.Add(item.PostingId);
            }

            PostingIds = PostingIds!.Distinct().ToList();

            var applicantPass = _selectionApplicantBeritaAcara.Where(x => x.Status == ApplicantStatus.Passed).ToList();
            if (applicantPass.Count() > 0)
            {
                _contentButtonSave = "Update";
                _disableButtonSave = false;
                _isEnabled = false;
            }
        }
        else
        {
            _isLoadingData = false;
        }

        _isLoadingData = false;
    }

    private async Task OnClickSaveAsync()
    {
        _isLoadingDialog = true;
        _disableButtonSave = true;
        if (_title == "")
        {
            _snackbar.AddWarning("Please input title");
            _isLoadingDialog = false;
            _disableButtonSave = false;
            StateHasChanged();
            return;
        }

        await OnUploadDocument();
        if (_documentDraftId == Guid.Empty)
        {
            _snackbar.AddWarning("Error upload file");
            _isLoadingDialog = false;
            _disableButtonSave = false;
            StateHasChanged();
            return;
        }

        if (Type == "Add")
        {
            var request = new CreateSelectionBeritaAcaraByBatchRequest
            {
                Type = "Draft",
                DraftDocumentId = _documentDraftId,
                Title = _title,
                Status = BeritaAcaraStatus.Saved,
                ScholarshipPostingIds = PostingIds

            };
            var saveBeritaCara = await _selectionBeritaAcaraService.CreateSelectionBeritaAcaraByBatchAsync(request);
            if (saveBeritaCara.Error is not null)
            {
                var result = string.Empty;
                for (var i = 0; i < saveBeritaCara.Error.Details.Count; i++)
                {
                    result = result + saveBeritaCara.Error.Details[i] + ",";
                }

                _snackbar.AddError("Berita Acara Error : " + result);
                _isLoadingDialog = false;
                StateHasChanged();
                return;
            }

            // Move to type Edit
            _snackbar.AddSuccess("Success save data");
            _navigationManager.NavigateTo(RouteFor.AddByBatch("Edit", Convert.ToInt32(Batch), Year), true);
            return;
        }
        else
        {
            var request = new UpdateSelectionBeritaAcaraByBatchRequest
            {
                Type = "Draft",
                DraftDocumentId = _documentDraftId,
                Title = _title,
                ScholarshipPostingIds = PostingIds

            };
            var updateBeritaCara = await _selectionBeritaAcaraService.UpdateSelectionBeritaAcaraByBatchAsync(request);
            if (updateBeritaCara.Error is not null)
            {
                var result = string.Empty;
                for (var i = 0; i < updateBeritaCara.Error.Details.Count; i++)
                {
                    result = result + updateBeritaCara.Error.Details[i] + ",";
                }

                _snackbar.AddError("Berita Acara Error : " + result);
                _isLoadingDialog = false;
                StateHasChanged();
                return;
            }
        }

        await OnLoadData();
        await OnLoadExist();
        _contentButtonSave = "Update";
        _disableButtonDocument = _documentDraftId == Guid.Empty && _disableButtonSave;
        _disableButtonDownload = false;
        await BtnActionDocument();
        _isLoadingDialog = false;
        StateHasChanged();
        _snackbar.AddSuccess("Success save data");
    }

    private async Task OnUploadDocument()
    {
        var timeStamp = DateTime.Now.ToString("ddMMyyyyHHmmss");
        var filename = $"Draft Berita Acara_{timeStamp}.docx";
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
                DocumentType = DocumentType.BeritaAcara,
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

    #region Modal Confirm
    private async Task BtnActionDocument()
    {
        ReadOnly = !ReadOnly;
        Toolbar = !Toolbar;
        ShowProperties = !ShowProperties;
        _isEnabled = !_isEnabled;
        await Task.CompletedTask;
    }

    private async Task OnClickUpdateAsync()
    {
        _contentButtonSave = "Save";
        await BtnActionDocument();
        await OnLoadDocumentDraftAndMaster();
        _disableButtonDocument = true;
        _disableButtonDownload = true;
        await Task.CompletedTask;
    }
    private async Task OnClickPsignAsync()
    {
        _isVisibleConfirmation = true;
        _typeConfirmation = "Psign";
        _titleConfirmation = "P-Sign";
        _contentConfirmation = "<p style='padding:20px;'>Are you sure send to P-Sign this data?</p>";
        await Task.CompletedTask;
    }

    private async Task OnClickPofficeAsync()
    {
        _isVisibleConfirmation = true;
        _typeConfirmation = "Poffice";
        _titleConfirmation = "P-Office";
        _contentConfirmation = "<p style='padding:20px;'>Are you sure send to P-Office this data?</p>";
        await Task.CompletedTask;
    }

    private async Task OnClickRejectAsync()
    {
        _isVisibleConfirmation = true;
        _typeConfirmation = "Rejected";
        _titleConfirmation = "Reject";
        _contentConfirmation = "<p style='padding:20px;'>Are you sure reject this data?</p>";
        await Task.CompletedTask;
    }

    private async Task OnClickDownloadWordAsync()
    {
        if (_documentDraftId == Guid.Empty)
        {
            _snackbar.AddError("Please generate draft document!!");
            return;
        }

        var doc = await _documentService.GetDocumentFileByDocumentTypeAsync(new GetDocumentFileRequest { DocumentId = _documentDraftId, DocumentType = DocumentType.BeritaAcara });
        if (doc.Error is not null)
        {
            _isLoadingDialog = false;
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
        _isLoadingDialog = true;
        await ExportAsPdf();
        _isLoadingDialog = false;
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
            $"Draft Berita Acara_{DateTime.Now:ddMMyyyyHHmmss}.pdf",
            $"{ContentTypes.ApplicationPdf}",
            content);
        downloadStream.Close();
        stream.Close();
    }

    private void DialogConfirmationOpen()
    {
        _isVisibleConfirmation = true;
    }

    private void DialogConfirmationClose()
    {
        _isVisibleConfirmation = false;
    }

    private void ClickedNoConfirmation()
    {
        _isVisibleConfirmation = false;
    }

    private async Task ClickedYesConfirmation()
    {
        _isLoadingDialog = true;
        var status = BeritaAcaraStatus.Saved;
        if (_typeConfirmation is not null)
        {
            switch (_typeConfirmation)
            {
                case "Psign":
                    status = BeritaAcaraStatus.ProcessToPSign;
                    break;

                case "Poffice":
                    status = BeritaAcaraStatus.ProcessToPOffice;
                    break;

                case "Rejected":
                    status = BeritaAcaraStatus.Rejected;
                    break;
            }

            var updateData = new UpdateSelectionBeritaAcaraStatusByBatchRequest
            {
                ScholarshipPostingIds = PostingIds,
                Status = status
            };

            var updateDataResponse = await _selectionBeritaAcaraService.UpdateSelectionBeritaAcaraStatusByBatchAsync(updateData);
            _error = updateDataResponse.Error;
            if (updateDataResponse.Error is not null)
            {
                _isLoadingDialog = false;
                _isVisibleConfirmation = false;
                StateHasChanged();
                var result = string.Join(", ", updateDataResponse.Error.Details.ToList());
                _snackbar.AddError("Error : " + result);
                return;
            }

            if (status is BeritaAcaraStatus.ProcessToPSign or BeritaAcaraStatus.ProcessToPOffice)
            {
                _disableButtonDocument = true;
            }
        }

        _isVisibleConfirmation = false;
        _typeConfirmation = "";
        _titleConfirmation = "";
        _contentConfirmation = "";
        _isLoadingDialog = false;
        _snackbar.AddSuccess($"Success update data to {status.GetDescription()}!");
        await OnLoadData();
        StateHasChanged();
        await Task.CompletedTask;
    }
    #endregion

    #region Word Processing
    private async Task OnLoadDocument()
    {
        _disableButtonDocument = _documentDraftId == Guid.Empty && !_disableButtonSave;
        if (_dataItem.Status is BeritaAcaraStatus.Completed)
        {
            _disableButtonDocument = true;
        }
        else if (_dataItem.Status is BeritaAcaraStatus.ProcessToPSign or BeritaAcaraStatus.ProcessToPOffice)
        {
            _disableButtonDocument = true;
        }

        if (_documentDraftId != Guid.Empty)
        {
            await OnLoadExist();
        }
        else if (_dataItem.Status is not BeritaAcaraStatus.Completed)
        {
            await OnLoadMaster();
        }
        else
        {
            _disableButtonDownload = true;
        }

        await Task.CompletedTask;
    }

    private async Task OnLoadDocumentDraftAndMaster()
    {
        if (_documentDraftId != Guid.Empty)
        {
            await OnLoadDraft();
        }
        else
        {
            await OnLoadMaster();
        }

        await Task.CompletedTask;
    }

    private async Task OnLoadExist()
    {
        var request = new GetDocumentFileRequest
        {
            DocumentId = _documentDraftId,
            DocumentType = DocumentType.BeritaAcara
        };

        var getDraftDocument = await _documentService.GetDocumentFileByDocumentTypeAsync(request);
        _errorDraft = getDraftDocument.Error;
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
            DocumentType = DocumentType.BeritaAcara
        };

        var getDraftDocument = await _documentService.GetDocumentFileByDocumentTypeAsync(request);
        _errorDraft = getDraftDocument.Error;
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
            DocumentId = Guid.Parse(_documentOptions.Value.DocumentDraftId.SelectionOpen.BeritaAcara),
            DocumentType = DocumentType.Template
        };

        var getDraftDocument = await _documentService.GetDocumentFileByDocumentTypeAsync(request);
        _errorDraft = getDraftDocument.Error;
        if (getDraftDocument.Error is null && _containerDocument != null)
        {
            Stream stream = new MemoryStream(getDraftDocument.Result!.Content);
            var editor = _containerDocument.DocumentEditor;
            await editor.OpenAsync(stream, ImportFormatType.Docx);
        }

        StateHasChanged();
        await Task.CompletedTask;
    }

    public async void OnCreated()
    {
        Toolbar = false;
        await OnLoadDocument();
    }
    #endregion

    public async Task OnClickBack()
    {
        _navigationManager.NavigateTo(Psmp.Bsui.Features.Selections.CreateSK.Constants.RouteFor.Selection, true);
        await Task.CompletedTask;
    }

    #region Action Final
    private async Task OnClickSubmitAsync()
    {
        if (_selectionApplicantBeritaAcara.Count() > 0)
        {
            var applicantPass = _selectionApplicantBeritaAcara.Where(x => x.Status == ApplicantStatus.Passed).ToList();
            foreach (var item in applicantPass)
            {
                var getDecree = await _selectionDecreesService.GetSelectionDecreeByApplicantIdAsync(item.ApplicantId);
                if (getDecree.Error is null)
                {
                    if (getDecree.Result!.Id == Guid.Empty)
                    {
                        _isLoadingDialog = true;
                        var getApplicant = await _selectionTalentReviewService.GetSelectionTalentReviewByApplicantIdAsync(item.ApplicantId);

                        if (getApplicant != null)
                        {
                            var decreeRequest = new CreateSelectionDecreeRequest
                            {
                                ApplicantId = item.ApplicantId,
                                Status = ApplicantStatus.InProgress,
                                PsychotestQualification = item.QualificationStatus,
                                MentorId = getApplicant.Result!.MentorId,
                                MentorName = getApplicant.Result!.MentorName,
                                MentorEmail = getApplicant.Result!.MentorEmail,
                                MentorPosition = getApplicant.Result!.MentorPosition,
                                MentorId2 = getApplicant.Result!.MentorId2,
                                MentorName2 = getApplicant.Result!.MentorName2,
                                MentorEmail2 = getApplicant.Result!.MentorEmail2,
                                MentorPosition2 = getApplicant.Result!.MentorPosition2,
                                DecreeStage = DecreeStage.Manager
                            };
                            var saveDecree = await _selectionDecreesService.CreateSelectioDecreeAsync(decreeRequest);
                            if (saveDecree.Error is not null)
                            {
                                var result = string.Empty;
                                for (var i = 0; i < saveDecree.Error.Details.Count; i++)
                                {
                                    result = result + saveDecree.Error.Details[i] + ",";
                                }

                                _snackbar.AddError("Decree - Error : " + result);
                                _isLoadingDialog = false;
                                StateHasChanged();
                                return;
                            }

                            var updateOverviewRequest = new UpdateSelectionOverviewByApplicantIdRequest
                            {
                                ApplicantId = item.ApplicantId,
                                Stage = SelectionStage.Decree,
                                Status = ApplicantStatus.InProgressSK,
                            };

                            await _selectionOverviewService.UpdateSelectionOverviewByApplicantIdAsync(updateOverviewRequest);

                            var request = new UpdateSelectionTalentReviewRequest
                            {
                                Id = item.TalentReviewId,
                                ApplicantId = item.ApplicantId,
                                Status = Base.Selections.Enums.ApplicantStatus.InProgressSK,
                                InnovationProject = getApplicant.Result.InovationProject,
                                MentorId = getApplicant.Result.MentorId,
                                MentorName = getApplicant.Result.MentorName,
                                MentorEmail = getApplicant.Result.MentorEmail,
                                MentorPosition = getApplicant.Result.MentorPosition,
                                MentorId2 = getApplicant.Result!.MentorId2,
                                MentorName2 = getApplicant.Result!.MentorName2,
                                MentorEmail2 = getApplicant.Result!.MentorEmail2,
                                MentorPosition2 = getApplicant.Result!.MentorPosition2,
                            };

                            await _selectionTalentReviewService.UpdateSelectionTalentReviewAsync(request);

                            var requestStatus = new UpdateSelectionBeritaAcaraApplicantRequest
                            {
                                ScholarshipPostingApplicantId = item.ApplicantId,
                                Status = ApplicantStatus.InProgressSK
                            };
                            var updateStatus = await _selectionBeritaAcaraService.UpdateSelectionApplicantBeritAcaraApplicantAsync(requestStatus);
                            if (updateStatus.Error is not null)
                            {
                                var result = string.Empty;
                                for (var i = 0; i < updateStatus.Error.Details.Count; i++)
                                {
                                    result = result + updateStatus.Error.Details[i] + ",";
                                }

                                _snackbar.AddError("Berita Acara Error : " + result);
                                _isLoadingDialog = false;
                                StateHasChanged();
                                return;
                            }
                        }
                    }
                }
            }
        }

        _isLoadingDialog = false;
        _snackbar.AddSuccess("Submit data success");
    }
    #endregion

    #region Download Applicant
    private async Task OnDownloadApplicant()
    {
        var table = new DataTable();

        table.Columns.Add("Code", typeof(string));
        table.Columns.Add("Batch", typeof(string));
        table.Columns.Add("Posting Year", typeof(string));
        table.Columns.Add("Employee Number", typeof(string));
        table.Columns.Add("Name", typeof(string));
        table.Columns.Add("Faculty", typeof(string));
        table.Columns.Add("Company", typeof(string));
        table.Columns.Add("Directorate", typeof(string));
        table.Columns.Add("Mentor", typeof(string));
        table.Columns.Add("TOEFL", typeof(string));
        table.Columns.Add("Psychotest", typeof(string));
        table.Columns.Add("Status", typeof(string));

        if (_selectionApplicantBeritaAcara is not null)
        {
            foreach (var item in _selectionApplicantBeritaAcara)
            {
                table.Rows.Add(
                    item.Code,
                    item.Batch,
                    item.DueDate!.Value.Date.Year,
                    item.EmployeeNumber,
                    item.Name,
                    item.Faculty,
                    item.CompanyName,
                    item.DirectorateName,
                    item.Mentor,
                    item.TOEFLScore,
                    item.QualificationStatus,
                    item.Status.GetDescription());
            }

            await GenerateExcel(table);
        }

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
        worksheet.Range[$"H1"].ColumnWidth = 40;
        worksheet.Range[$"I1"].ColumnWidth = 40;
        worksheet.Range[$"J1"].ColumnWidth = 40;
        worksheet.Range[$"K1"].ColumnWidth = 40;
        worksheet.Range[$"L1"].ColumnWidth = 40;

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
            string.IsNullOrEmpty(fileName) ? $"BeritaAcaraApplicant_{DateTime.Now:yyyyMMdd}.xlsx" : fileName,
            $"{ContentTypes.ApplicationOpenXmlExcel}",
            content);

    }
    #endregion
}
