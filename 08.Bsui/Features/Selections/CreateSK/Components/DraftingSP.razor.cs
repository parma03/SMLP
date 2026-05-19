using System.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Psmp.Base.Selections.Enums;
using Psmp.Bsui.Common.Constants;
using Psmp.Bsui.Common.Extensions;
using Psmp.Shared.Common.Constants;
using Psmp.Shared.Common.Extensions;
using Psmp.Shared.Common.Responses;
using Psmp.Shared.Documents.Commands.CreateDocument;
using Psmp.Shared.Documents.Queries.GetDocumentFile;
using Psmp.Shared.Selections.CreateSK.Commands.Create;
using Psmp.Shared.Selections.CreateSK.Commands.Update;
using Psmp.Shared.Selections.CreateSK.Queries;
using Syncfusion.Blazor.DocumentEditor;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocIORenderer;

namespace Psmp.Bsui.Features.Selections.CreateSK.Components;

public partial class DraftingSP
{
    [Parameter]
    public Guid Id { get; set; }

    [Parameter]
    public int Batch { get; set; }

    [Parameter]
    public int Year { get; set; }

    protected ErrorResponse? _error;
    protected ErrorResponse? _errorDraft;

    protected bool _isLoadingData = false;
    protected bool _isLoadingDialog = false;
    protected bool _isEnabled = false;
    internal string? DocumentName { get; set; }
    protected long _maximumFileSizeInBytes;
    public FileUploadResponse FileUploads { get; set; } = new();

    protected bool _disableButton = false;
    protected bool _disableButtonDocument = false;
    protected bool _disableButtonReject = false;
    protected bool _disableButtonDownload = false;

    protected GetSelectionWarrantDraftQueryResponse _dataItem = new();
    protected GetSelectionCreateSKQueryResponse _dataApplicant = new();
    protected IList<GetSelectionDecreeByBatchQueryResponse> _dataAllApplicant = new List<GetSelectionDecreeByBatchQueryResponse>();

    protected bool _isVisibleConfirmation = false;
    protected string? _typeConfirmation = "";
    protected string? _titleConfirmation = "";
    protected string? _contentConfirmation = "";

    protected string? _contentTooltipSuperior = "";
    protected string? _contentTooltipStudyStartDate = "";
    protected string? _contentTooltipStudyEndDate = "";

    #region Document Editor
    protected SfDocumentEditorContainer _containerDocument = new();
    protected DocumentEditorSettingsModel _settingDocument = new() { ShowRuler = true };
    protected string[] _toolbarItems = DocumentEditorExtensions.ToolBarItems();
    internal bool ReadOnly { get; set; } = true;
    internal bool Toolbar { get; set; } = true;
    internal bool ShowProperties { get; set; } = false;

    protected Guid _documentDraftId = Guid.Empty;
    protected string _contentButton = "Update";
    #endregion

    protected override async Task OnInitializedAsync()
    {
        //_isLoadingDialog = true;
        _maximumFileSizeInBytes = _documentOptions.Value.MaximumFileSizeInBytes;
        await OnLoadData();
        //_isLoadingDialog = false;
        StateHasChanged();
    }

    public async Task OnClickBack()
    {
        _navigationManager.NavigateTo(Psmp.Bsui.Features.Selections.CreateSK.Constants.RouteFor.Selection, true);
        await Task.CompletedTask;
    }

    private async Task OnLoadData()
    {
        _isLoadingData = true;

        var param = new GetSelectionDecreeByBatchFilterRequest
        {
            SelectionBatch = Convert.ToInt32(Batch),
            Year = Year
        };

        var getAllApplicant = await _selectionDecreesService.GetSelectionDecreeByBatchAsync(param);
        _error = getAllApplicant.Error;
        if (getAllApplicant.Error is null)
        {
            _dataAllApplicant = getAllApplicant.Result!.Items;
            if (_dataAllApplicant.Count() > 0)
            {
                // get first applicant
                var applicant = _dataAllApplicant.OrderByDescending(x => x.CreatedDraftWarrant).FirstOrDefault();
                var response = await _selectionDecreesService.GetSelectionWarrantDraftByApplicantIdAsync(applicant!.ApplicantId);
                _error = response.Error;
                if (response.Error is null)
                {
                    if (response.Result!.Id != Guid.Empty)
                    {
                        _dataItem.Id = response.Result!.Id;
                        _dataItem.SuperiorLetter = response.Result!.SuperiorLetter;
                        _dataItem.StudyStartDate = response.Result!.StudyStartDate ?? DateTime.Now.ToDateOnly();
                        _dataItem.StudyEndDate = response.Result!.StudyEndDate ?? DateTime.Now.ToDateOnly();
                        _dataItem.ApplicantId = response.Result!.ApplicantId;
                        _dataItem.DocumentId = response.Result!.DocumentId;
                        _dataItem.EmployeeNumber = response.Result!.EmployeeNumber;
                        _dataItem.Status = response.Result!.Status;

                        // get status from admission agreement
                        _disableButton = _dataItem.Status switch
                        {
                            DraftWarrantStatus.ProcessToPOffice or DraftWarrantStatus.ProcessSign => true,
                            _ => false,
                        };

                        _disableButtonReject = _dataItem.Status switch
                        {
                            DraftWarrantStatus.ProcessToPOffice or DraftWarrantStatus.ProcessSign => false,
                            _ => true,
                        };

                        _documentDraftId = response.Result!.DocumentId! ?? Guid.Empty;

                        var getApplicant = await _selectionDecreesService.GetSelectionDecreeByApplicantIdAsync(applicant!.ApplicantId);
                        _error = getApplicant.Error;
                        if (getApplicant.Error is null)
                        {
                            _dataApplicant = getApplicant.Result!;

                            if (getApplicant.Result!.Status is ApplicantStatus.SKCreated || getApplicant.Result.Status == ApplicantStatus.SKSend)
                            {
                                _disableButton = true;
                                _disableButtonDocument = true;
                                _disableButtonReject = true;
                            }
                        }

                        await OnLoadDocument();
                    }
                    else
                    {
                        _dataItem.StudyStartDate = DateTime.Now.ToDateOnly();
                        _dataItem.StudyEndDate = DateTime.Now.ToDateOnly();

                        var getApplicant = await _selectionDecreesService.GetSelectionDecreeByApplicantIdAsync(applicant!.ApplicantId);
                        _error = getApplicant.Error;
                        if (getApplicant.Error is null)
                        {
                            _dataApplicant = getApplicant.Result!;
                            _dataItem.EmployeeNumber = getApplicant.Result!.EmployeeNumber;
                        }

                        _disableButtonDocument = true;
                        _disableButtonDownload = true;
                        _disableButtonReject = true;
                    }
                }
            }
        }

        _isLoadingData = false;
    }

    private async Task OnClickSaveAsync()
    {
        _isLoadingDialog = true;
        if (_dataItem.SuperiorLetter is "" or null)
        {
            _snackbar.AddWarning("Please input superior letter");
            _isLoadingDialog = false;
            StateHasChanged();
            return;
        }

        await OnUploadDocument();

        if (_documentDraftId == Guid.Empty)
        {
            _snackbar.AddWarning("Error upload file");
            _isLoadingDialog = false;
            StateHasChanged();
            return;
        }

        if (_dataItem.Id == Guid.Empty)
        {
            var request = new CreateSelectionWarrantDraftRequest
            {
                DocumentId = _documentDraftId,
                SuperiorLetter = _dataItem.SuperiorLetter,
                StudyStartDate = _dataItem.StudyStartDate,
                StudyEndDate = _dataItem.StudyEndDate,
                ApplicantId = Id,
                Status = DraftWarrantStatus.Draft
            };

            var saveDecreeDraft = await _selectionDecreesService.CreateSelectionWarrantDraftAsync(request);
            if (saveDecreeDraft.Error is not null)
            {
                _snackbar.AddError("Decree Draft Error : " + saveDecreeDraft.Error.ToDetail());
                _isLoadingDialog = false;
                StateHasChanged();
                return;
            }

            Id = saveDecreeDraft.Result!.Id;
        }
        else
        {
            // update all status on applicant
            foreach (var item in _dataAllApplicant)
            {
                var request = new UpdateSelectionWarrantDraftRequest
                {
                    DocumentId = _documentDraftId,
                    SuperiorLetter = "-",
                    StudyStartDate = _dataItem.StudyStartDate,
                    StudyEndDate = _dataItem.StudyEndDate,
                    ApplicantId = item.ApplicantId,
                    Status = DraftWarrantStatus.Draft
                };

                await _selectionDecreesService.UpdateSelectionWarrantDraftAsync(request);
            }
        }

        await OnLoadData();
        await OnLoadExist();
        _contentButton = "Update";
        _disableButtonDocument = _documentDraftId == Guid.Empty && _disableButton;
        _disableButtonDownload = false;
        _disableButtonReject = true;
        await BtnActionDocument();
        _isLoadingDialog = false;
        StateHasChanged();
        _snackbar.AddSuccess("Success save data");
    }

    private async Task OnUploadDocument()
    {
        var timeStamp = DateTime.Now.ToString("ddMMyyyyHHmmss");
        var filename = $"Draft Surat Perintah_{Batch}_{Year}_{timeStamp}.docx";
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
                DocumentType = DocumentType.ScholarshipWarrant,
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

    #region Action Document
    public async Task OnClickGenerateAsync()
    {
        _isLoadingDialog = true;
        await OnLoadDataGenerate();
        await OnUploadDocument();

        if (_documentDraftId == Guid.Empty)
        {
            _snackbar.AddWarning("Error upload file");
            _isLoadingDialog = false;
            StateHasChanged();
            return;
        }

        foreach (var item in _dataAllApplicant)
        {
            var request = new CreateSelectionWarrantDraftRequest
            {
                DocumentId = _documentDraftId,
                SuperiorLetter = "-",
                StudyStartDate = _dataItem.StudyStartDate,
                StudyEndDate = _dataItem.StudyEndDate,
                ApplicantId = item.ApplicantId,
                Status = DraftWarrantStatus.Draft
            };

            await _selectionDecreesService.CreateSelectionWarrantDraftAsync(request);
        }

        await OnLoadData();
        await OnLoadExist();
        _snackbar.AddSuccess("Success generate document draft!");
        _isLoadingDialog = false;
        await Task.CompletedTask;
    }

    public async Task OnClickReGenerateAsync()
    {
        _isLoadingDialog = true;
        await OnLoadDataGenerate();
        await OnUploadDocument();

        if (_documentDraftId == Guid.Empty)
        {
            _snackbar.AddWarning("Error upload file");
            _isLoadingDialog = false;
            StateHasChanged();
            return;
        }

        foreach (var item in _dataAllApplicant)
        {
            if (item.DocumentIdDraftWarrant == Guid.Empty)
            {
                var request = new CreateSelectionWarrantDraftRequest
                {
                    DocumentId = _documentDraftId,
                    SuperiorLetter = "-",
                    StudyStartDate = _dataItem.StudyStartDate,
                    StudyEndDate = _dataItem.StudyEndDate,
                    ApplicantId = item.ApplicantId,
                    Status = DraftWarrantStatus.Draft
                };

                await _selectionDecreesService.CreateSelectionWarrantDraftAsync(request);
            }
            else
            {
                var request = new UpdateSelectionWarrantDraftRequest
                {
                    DocumentId = _documentDraftId,
                    SuperiorLetter = "-",
                    StudyStartDate = _dataItem.StudyStartDate,
                    StudyEndDate = _dataItem.StudyEndDate,
                    ApplicantId = item.ApplicantId,
                    Status = DraftWarrantStatus.Draft
                };

                await _selectionDecreesService.UpdateSelectionWarrantDraftAsync(request);
            }
        }

        await OnLoadData();
        await OnLoadExist();
        _snackbar.AddSuccess("Success generate document draft!");
        _isLoadingDialog = false;
        await Task.CompletedTask;
    }

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
        _contentButton = "Save";
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

        var doc = await _documentService.GetDocumentFileByDocumentTypeAsync(new GetDocumentFileRequest { DocumentId = _documentDraftId, DocumentType = DocumentType.ScholarshipWarrant });
        if (doc.Error is not null)
        {
            _isLoadingDialog = false;
            StateHasChanged();
            _snackbar.AddError("Error : " + doc.Error.ToDetail());
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
            $"Draft Surat Perintah_{_dataItem.EmployeeNumber}_{DateTime.Now:yyyyMMdd}.pdf",
            $"{ContentTypes.ApplicationPdf}",
            content);
        downloadStream.Close();
        stream.Close();
    }
    #endregion

    #region Modal Confirm
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
        var status = DraftWarrantStatus.Draft;
        if (_typeConfirmation is not null)
        {
            switch (_typeConfirmation)
            {
                case "Psign":
                    status = DraftWarrantStatus.ProcessSign;
                    break;

                case "Poffice":
                    status = DraftWarrantStatus.ProcessToPOffice;
                    break;

                case "Rejected":
                    status = DraftWarrantStatus.Rejected;
                    break;
            }

            var updateData = new UpdateSelectionWarrantDraftStatusRequest
            {
                ApplicantId = Id,
                Status = status
            };

            var updateDataResponse = await _selectionDecreesService.UpdateSelectionWarrantDraftStatusAsync(updateData);
            _error = updateDataResponse.Error;
            if (updateDataResponse.Error is not null)
            {
                _isLoadingDialog = false;
                _isVisibleConfirmation = false;
                StateHasChanged();
                _snackbar.AddError("Error : " + updateDataResponse.Error.ToDetail());
                return;
            }

            if (status is DraftWarrantStatus.ProcessSign or DraftWarrantStatus.ProcessToPOffice)
            {
                _disableButtonDocument = true;
            }

            _dataItem.Status = status;
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
    private async Task OnLoadDataGenerate()
    {
        var request = new GetDocumentFileRequest
        {
            DocumentId = Guid.Parse(_documentOptions.Value.DocumentDraftId.SelectionOpen.SP),
            DocumentType = DocumentType.Template
        };

        var getDraftDocument = await _documentService.GetDocumentFileByDocumentTypeAsync(request);
        _errorDraft = getDraftDocument.Error;
        if (getDraftDocument.Error is null && _containerDocument != null)
        {
            var document = new Syncfusion.DocIO.DLS.WordDocument();
            Stream stream = new MemoryStream(getDraftDocument.Result!.Content);
            document.Open(stream, Syncfusion.DocIO.FormatType.Docx);
            stream.Dispose();

            document.Replace("{{Periode}}", string.Format("{0}/{1}", Year, Year + 1), true, true);
            document.Replace("{{Batch}}", string.Format("{0}", Batch.ToRoman()), true, true);

            // get section for table data
            var textHeader = document.Find("{{FirstHeader}}", true, true);
            if (textHeader != null)
            {
                var idxTable = 0;
                var mainTable = document.Sections[1].Tables[0].Clone() as WTable;
                var section = document.Sections[1];
                var table = section.Tables[idxTable];
                var mainRow = table.Rows[2].Clone() as WTableRow;
                var mainRowData = table.Rows[3].Clone() as WTableRow;

                // helper
                var tableDataSelection = document.Find("{{TableData}}", true, true);
                var mainHeader = tableDataSelection.GetAsOneRange().OwnerParagraph.Clone() as WParagraph;
                var textTableData = tableDataSelection.GetAsOneRange();
                var lParagraph = tableDataSelection.GetAsOneRange().OwnerParagraph;
                var lParagraphIdx = lParagraph.OwnerTextBody.ChildEntities.IndexOf(lParagraph);

                var applicantGroupBy = _dataAllApplicant.GroupBy(x => new { x.CompanyGroupId, x.FungsiId, x.FungsiName }).Select(x => new { CompanyGroupId = x.Key.CompanyGroupId, FungsiId = x.Key.FungsiId, FungsiName = x.Key.FungsiName }).ToList();
                var companyGroup = _dataAllApplicant.OrderBy(x => x.CompanyGroupId).GroupBy(x => new { x.CompanyGroupId, x.CompanyGroupName }).Select(x => new { CompanyGroupId = x.Key.CompanyGroupId, CompanyGroupName = x.Key.CompanyGroupName }).ToList();

                // delete table data
                lParagraph.ChildEntities.Remove(textTableData);

                var numberApplicant = 1;
                var idxGrp = 1;
                foreach (var grp in companyGroup)
                {
                    if (idxGrp == 1)
                    {
                        document.Replace("{{FirstHeader}}", $"TUGAS BELAJAR {grp.CompanyGroupName.ToUpper()}", true, true);
                    }
                    else
                    {
                        if (idxGrp > 2)
                        {
                            lParagraphIdx++;
                            lParagraphIdx++;
                        }

                        lParagraph.OwnerTextBody.ChildEntities.Insert(lParagraphIdx, mainHeader);
                        tableDataSelection = document.Find("{{TableData}}", true, true);
                        var newMainHeader = tableDataSelection.GetAsOneRange().OwnerParagraph.Clone() as WParagraph;
                        document.Replace("{{TableData}}", $"TUGAS BELAJAR {grp.CompanyGroupName.ToUpper()}", true, true);
                        lParagraphIdx++;
                        lParagraphIdx++;
                        lParagraph.OwnerTextBody.ChildEntities.Insert(lParagraphIdx, mainTable);
                        idxTable++;
                        mainHeader = newMainHeader;
                        table = section.Tables[idxTable];
                        mainTable = section.Tables[idxTable].Clone() as WTable;
                    }

                    var currentRow = 1;
                    foreach (var item in applicantGroupBy.Where(x => x.CompanyGroupId == grp.CompanyGroupId))
                    {
                        if (currentRow == 1)
                        {
                            currentRow++;
                            table.Rows[currentRow].Cells[0].AddParagraph().AppendText(item.FungsiName);
                            currentRow++;
                        }
                        else
                        {
                            table.Rows.Insert(currentRow, mainRow);
                            mainRow = table.Rows[currentRow].Clone() as WTableRow;
                            table.Rows[currentRow].Cells[0].AddParagraph().AppendText(item.FungsiName);
                            currentRow++;
                            table.Rows.Insert(currentRow, mainRowData);
                            mainRowData = table.Rows[currentRow].Clone() as WTableRow;
                        }

                        var idxDtl = 1;
                        var dtlApplicant = _dataAllApplicant.Where(x => x.CompanyGroupId == item.CompanyGroupId && x.FungsiId == item.FungsiId).ToList();
                        foreach (var dtl in dtlApplicant)
                        {
                            var universityLocation = dtl.Category == Base.Scholarships.Enums.ScholarshipLocation.Domestic ? "DN" : "LN";
                            table.Rows[currentRow].Cells[0].AddParagraph().AppendText(numberApplicant.ToString());
                            table.Rows[currentRow].Cells[1].AddParagraph().AppendText($"{dtl.MentorName}/{dtl.MentorPosition}, {dtl.MentorName2}/{dtl.MentorPosition2}");
                            table.Rows[currentRow].Cells[2].AddParagraph().AppendText(dtl.EmployeeNumber);
                            table.Rows[currentRow].Cells[3].AddParagraph().AppendText(dtl.Name);
                            table.Rows[currentRow].Cells[4].AddParagraph().AppendText(dtl.Code);
                            table.Rows[currentRow].Cells[5].AddParagraph().AppendText(universityLocation);
                            table.Rows[currentRow].Cells[6].AddParagraph().AppendText(dtl.BusinessIssue);

                            if (idxDtl < dtlApplicant.Count())
                            {
                                table.AddRow(true);
                            }

                            currentRow++;
                            numberApplicant++;
                            idxDtl++;
                        }
                    }

                    idxGrp++;
                }
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

    private async Task OnLoadDocument()
    {
        _disableButtonDocument = _documentDraftId == Guid.Empty && !_disableButton;
        if (_dataApplicant.Status is ApplicantStatus.SKCreated or ApplicantStatus.SKSend)
        {
            _disableButton = true;
            _disableButtonDocument = true;
            if (_documentDraftId == Guid.Empty)
            {
                _dataItem.Status = DraftWarrantStatus.NoDraft;
            }
        }
        else if (_dataItem.Status is DraftWarrantStatus.ProcessSign or DraftWarrantStatus.ProcessToPOffice)
        {
            _disableButtonDocument = true;
        }

        if (_documentDraftId != Guid.Empty)
        {
            await OnLoadExist();
        }
        else if (_dataApplicant.Status is not ApplicantStatus.SKCreated and not ApplicantStatus.SKSend)
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
            DocumentType = DocumentType.ScholarshipWarrant
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
            DocumentType = DocumentType.ScholarshipWarrant
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
            DocumentId = Guid.Parse(_documentOptions.Value.DocumentDraftId.SelectionOpen.SP),
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
}
