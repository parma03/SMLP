using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Smlp.Base.Payments.Enums;
using Smlp.Base.Selections.Enums;
using Smlp.Bsui.Common.Constants;
using Smlp.Bsui.Common.Extensions;
using Smlp.Shared.Applicants.Queries.GetApplicant;
using Smlp.Shared.Common.Constants;
using Smlp.Shared.Common.Extensions;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Documents.Commands.CreateDocument;
using Smlp.Shared.Documents.Queries.GetDocumentFile;
using Smlp.Shared.Payments.Commands;
using Smlp.Shared.Payments.Queries;
using Syncfusion.Blazor.DocumentEditor;
using Syncfusion.DocIORenderer;
namespace Smlp.Bsui.Features.Applicants.Payments.Reimbursements;
public partial class DraftPayment
{
    [Parameter]
    public Guid Id { get; set; }
    [Parameter]
    public Guid ApplicantId { get; set; }

    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;
    protected ClaimsPrincipal _user = default!;
    protected ErrorResponse? _error;
    protected ErrorResponse? _errorDraft;
    protected bool _isLoading = false;
    protected bool _isLoadingDialog = false;
    protected ReimbursementRequest _requestReimbursement = new();
    protected GetApplicantReimbursementResponse _data = new();
    protected GetApplicantByEmployeeIdResponse _dataApplicant = new();

    #region Document Editor
    protected SfDocumentEditorContainer _containerDocument = default!;
    protected DocumentEditorSettingsModel _settingDocument = new() { ShowRuler = true };
    protected string[] _toolbarItems = DocumentEditorExtensions.ToolBarItems();
    internal bool ReadOnly { get; set; } = true;
    internal bool Toolbar { get; set; } = true;
    internal bool ShowProperties { get; set; } = false;
    protected Guid _documentDraftId = Guid.Empty;
    protected string _contentButton = "Generate";
    #endregion
    #region Confirm
    private string TitlePopupConfirm { get; set; } = "";
    private bool VisiblePopupConfirm { get; set; } = false;
    private string TypeConfirm { get; set; } = "";
    #endregion
    protected override async Task OnInitializedAsync()
    {
        _isLoading = true;
        _user = (await AuthenticationStateTask).User;
        await LoadData();
        _isLoading = false;
        StateHasChanged();
        await Task.CompletedTask;
    }
    private async Task LoadData()
    {
        var request = new GetApplicantReimbursementRequest
        {
            Id = Id,
            ApplicantId = ApplicantId
        };

        var response = await _paymentService.GetReimbursementByIdAsync(request);
        if (response.Error == null)
        {
            _data = response.Result;
            _requestReimbursement.CurrencyType = (int)_data.CurrencyType;
            _requestReimbursement.Amount = _data.Amount;
            _requestReimbursement.Category = _data.CategoryId;
            _requestReimbursement.Notes = _data.Notes;
            _requestReimbursement.RequestDate = _data.RequestDate;
            _requestReimbursement.ApplicantId = _data.ApplicantId;
            _requestReimbursement.Id = _data.Id;
            _requestReimbursement.IBANCode = _data.IBANCode;
            _requestReimbursement.DueDate = _data.DueDate;
            _requestReimbursement.UniversityBank = _data.UniversityBank;
            _requestReimbursement.UniversityBankAccount = _data.UniversityBankAccount;
            _requestReimbursement.DocumentId = _data.Document.Id;
            _requestReimbursement.PONumber = _data.PONumber;
            _requestReimbursement.BADate = _data.BADate;
            _requestReimbursement.BASTNumber = _data.BASTNumber;
            _requestReimbursement.PaymentStatus = (int)_data.PaymentStatus;
            _documentDraftId = _data.DocumentDraftId == null ? Guid.Empty : (Guid)_data.DocumentDraftId;
        }
    }
    public async Task OnClickGenerateAsync()
    {
        _isLoadingDialog = true;
        await OnLoadDataGenerate();
        _contentButton = "Update";
        _snackbar.AddSuccess("Success generate document draft!");
        _isLoadingDialog = false;
        await Task.CompletedTask;
    }

    private async Task OnLoadDataGenerate()
    {
        var responseDataApplicant = await _applicantService.GetApplicantByIdAsync(_data.ApplicantId);
        if (responseDataApplicant.Error is not null)
        {
            _snackbar.AddError("Applicant not found!");
            return;
        }

        _dataApplicant = responseDataApplicant.Result!.Items.FirstOrDefault();

        var request = new GetDocumentFileRequest
        {
            DocumentId = Guid.Parse(_documentOptions.Value.DocumentDraftId.Payment.TuitionFee),
            DocumentType = DocumentType.Template
        };

        var employeeId = $"00000000{_dataApplicant!.EmployeeNumber}";
        employeeId = employeeId[^8..];
        var dataHRIS = await _hrisService.GetEmployeeByIdAsync(employeeId);
        if (dataHRIS is null)
        {
            _snackbar.AddError("Employee not found in HRIS!");
            return;
        }

        var getDraftDocument = await _documentService.GetDocumentFileByDocumentTypeAsync(request);
        _errorDraft = getDraftDocument.Error;
        if (getDraftDocument.Error is null && _containerDocument != null)
        {
            var document = new Syncfusion.DocIO.DLS.WordDocument();
            Stream stream = new MemoryStream(getDraftDocument.Result!.Content);
            document.Open(stream, Syncfusion.DocIO.FormatType.Docx);
            stream.Dispose();

            var gender = dataHRIS.Gender;
            var currentPeriod = DateTime.Now.ToLongDateDisplayText();
            document.Replace("{{CurrentPeriod}}", currentPeriod, true, true);

            if (!string.IsNullOrEmpty(dataHRIS.EmployeeName))
            {
                document.Replace("{Nama_Pekerja}", dataHRIS.EmployeeName.ToCapitalizeEachWord(), true, true);
            }

            if (!string.IsNullOrEmpty(dataHRIS.EmployeeId))
            {
                document.Replace("{Nomor Pekerja}", dataHRIS.EmployeeId.ToCapitalizeEachWord(), true, true);
            }

            if (!string.IsNullOrEmpty(dataHRIS.Email))
            {
                document.Replace("{Email_Pekerja}", dataHRIS.Email.ToCapitalizeEachWord(), true, true);
            }

            if (!string.IsNullOrEmpty(_requestReimbursement.BASTNumber))
            {
                document.Replace("{Nomor_BAST}", _requestReimbursement.BASTNumber.ToCapitalizeEachWord(), true, true);
            }

            if (!string.IsNullOrEmpty(_requestReimbursement.PONumber))
            {
                document.Replace("{Nomor_PO}", _requestReimbursement.PONumber.ToCapitalizeEachWord(), true, true);
            }

            if (_requestReimbursement.BADate != null)
            {
                document.Replace("{Tanggal_BA}", _requestReimbursement.BADate!.Value.ToString("dd MMMM yyyy"), true, true);
            }

            if (_requestReimbursement.DueDate != null)
            {
                document.Replace("{Due_Date}", _requestReimbursement.DueDate!.Value.ToString("dd MMMM yyyy"), true, true);
            }

            if (!string.IsNullOrEmpty(_dataApplicant.UniversityApplicant))
            {
                document.Replace("{University}", _dataApplicant.UniversityApplicant.ToCapitalizeEachWord(), true, true);
            }

            if (!string.IsNullOrEmpty(_dataApplicant.UniversityLocation))
            {
                document.Replace("{Location}", _dataApplicant.UniversityLocation.ToCapitalizeEachWord(), true, true);
            }

            //document.Replace("{Amount}", string.Format("{0:0,0}", _data.Amount), true, true);
            document.Replace("{Amount}", _data.Amount.ToString("n0"), true, true);

            if (!string.IsNullOrEmpty(_data.CurrencyType.GetDescription()))
            {
                document.Replace("{Curancy}", _data.CurrencyType.GetDescription().ToCapitalizeEachWord(), true, true);
            }

            if (!string.IsNullOrEmpty(_data.PONumber))
            {
                document.Replace("{Periode}", _data.BASTNumber.ToCapitalizeEachWord(), true, true);
            }

            if (!string.IsNullOrEmpty(_data.UniversityBankAccount))
            {
                document.Replace("{University_Rekening}", _data.UniversityBankAccount.ToCapitalizeEachWord(), true, true);
            }

            if (!string.IsNullOrEmpty(_data.UniversityBank))
            {
                document.Replace("{Bank}", _data.UniversityBank.ToCapitalizeEachWord(), true, true);
            }

            if (!string.IsNullOrEmpty(_data.IBANCode))
            {
                document.Replace("{IBAN_Code}", _data.IBANCode.ToCapitalizeEachWord(), true, true);
            }

            if (!string.IsNullOrEmpty(_dataApplicant.PhoneNumber))
            {
                document.Replace("{No_Telp}", _dataApplicant.PhoneNumber.ToCapitalizeEachWord(), true, true);
            }

            if (_dataApplicant.StartDate != null)
            {
                document.Replace("{Periode}", $"{_dataApplicant.StartDate!.Value.Year} - {_dataApplicant.EndDate!.Value.Year}", true, true);
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
    private async Task OnUploadDocument()
    {
        var timeStamp = DateTime.Now.ToString("ddMMyyyyHHmmss");
        var filename = $"DraftTuitionFee_{_dataApplicant.EmployeeNumber}_{timeStamp}.docx";
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
                DocumentType = DocumentType.DraftTuitionFee,
                File = formFile,
                EmployeeNumber = _dataApplicant.EmployeeNumber
            };

            var resultUpload = await _documentService.CreateDocumentAsync(requestFile);
            if (resultUpload.Error is null)
            {
                _documentDraftId = resultUpload.Result!.DocumentId;
            }
        }

        await Task.CompletedTask;
    }
    private async Task OnClickUpdateAsync()
    {
        await BtnActionDocument();
        _contentButton = "Save";
    }
    private async Task BtnActionDocument()
    {
        ReadOnly = !ReadOnly;
        Toolbar = !Toolbar;
        ShowProperties = !ShowProperties;
        await Task.CompletedTask;
    }

    private async Task OnClickSaveAsync()
    {
        _isLoadingDialog = true;
        _contentButton = "Update";
        if (_requestReimbursement.BASTNumber == null)
        {
            _snackbar.AddWarning("BAST Number is not null");
            return;
        }

        if (_requestReimbursement.BADate == null)
        {
            _snackbar.AddWarning("BA Date is not null");
            return;
        }

        if (_requestReimbursement.PONumber == null)
        {
            _snackbar.AddWarning("PO Number is not null");
            return;
        }

        await BtnActionDocument();
        await OnUploadDocument();

        if (_documentDraftId == Guid.Empty)
        {
            _snackbar.AddWarning("Error upload file");
            _isLoadingDialog = false;
            StateHasChanged();
            return;
        }

        _requestReimbursement.DocumentDraft = _documentDraftId;
        var updateDataResponse = await _paymentService.UpdateReimbursementAsync(_requestReimbursement);
        if (updateDataResponse.Error is not null)
        {
            _isLoadingDialog = false;
            StateHasChanged();
            var result = string.Join(", ", updateDataResponse.Error.Details.ToList());
            _snackbar.AddError("Error : " + result);
            return;
        }

        await LoadData();
        await OnLoadExist();
        _snackbar.AddSuccess("Success update data!");
        //_disableButtonDocument = _documentDraftId == Guid.Empty && _disableButton;
        //_disableButtonDownload = false;
        _isLoadingDialog = false;
        await Task.CompletedTask;
    }
    private async Task OnLoadExist()
    {
        var request = new GetDocumentFileRequest
        {
            DocumentId = _documentDraftId,
            DocumentType = DocumentType.DraftTuitionFee
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
    private async Task OnLoadDocument()
    {
        //_disableButtonDocument = _documentDraftId == Guid.Empty && !_disableButton;
        //if (_dataItem.Status == SponsorshipLetterStatus.Completed)
        //{
        //    _disableButtonDocument = true;
        //}
        //else if (_dataItem.Status is SponsorshipLetterStatus.ProcessSign or SponsorshipLetterStatus.ProcessToPOffice)
        //{
        //    _disableButtonDocument = true;
        //}

        if (_documentDraftId != Guid.Empty)
        {
            await OnLoadExist();
        }
        else if (_data.PaymentStatus != Base.Payments.Enums.PaymentStatus.Completed)
        {
            await OnLoadMaster();
        }
        //else
        //{
        //    _disableButtonDownload = true;
        //}

        await Task.CompletedTask;
    }
    private async Task OnLoadMaster()
    {
        var request = new GetDocumentFileRequest
        {
            DocumentId = Guid.Parse(_documentOptions.Value.DocumentDraftId.Payment.TuitionFee),
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
    private async Task OnClickPsignAsync()
    {
        VisiblePopupConfirm = true;
        TitlePopupConfirm = "<p style='padding:20px;'>Are you send to P-Sign this data?";
        TypeConfirm = "P-Sign";
        await Task.CompletedTask;
    }

    private async Task OnClickPofficeAsync()
    {
        VisiblePopupConfirm = true;
        TitlePopupConfirm = "<p style='padding:20px;'>Are you send to P-Office this data?";
        TypeConfirm = "P-Office";
        await Task.CompletedTask;
    }

    private async Task OnClickDownloadWordAsync()
    {
        if (_documentDraftId == Guid.Empty)
        {
            _snackbar.AddError("Please generate draft document!!");
            return;
        }

        var doc = await _documentService.GetDocumentFileByDocumentTypeAsync(new GetDocumentFileRequest { DocumentId = _documentDraftId, DocumentType = DocumentType.DraftTuitionFee });
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
            $"DraftTuitionFee_{_dataApplicant.EmployeeNumber}_{DateTime.Now:yyyyMMdd}.pdf",
            $"{ContentTypes.ApplicationPdf}",
            content);
        downloadStream.Close();
        stream.Close();
    }

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
        var status = new PaymentStatus();
        switch (TypeConfirm)
        {
            case "P-Sign":
                status = PaymentStatus.ProcessSign;
                break;

            case "P-Office":
                status = PaymentStatus.ProcessToPOffice;
                break;

        }

        var updateData = new ReimburdementStatusRequest
        {
            Id = _data.Id,
            Status = status.GetIntValue(),
        };

        var updateDataResponse = await _paymentService.UpdateReimbursementStatusAsync(updateData);
        if (updateDataResponse.Error is not null)
        {
            _isLoadingDialog = false;
            VisiblePopupConfirm = false;
            StateHasChanged();
            var result = string.Join(", ", updateDataResponse.Error.Details.ToList());
            _snackbar.AddError("Error : " + result);
            return;
        }

        _snackbar.AddSuccess($"Success action to {TypeConfirm} data");
        await LoadData();
        DialogConfirmClose();
        StateHasChanged();
        await Task.CompletedTask;
    }
    #endregion
}
