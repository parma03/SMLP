using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Smlp.Base.Selections.Enums;
using Smlp.Base.Studies.Enums;
using Smlp.Bsui.Common.Constants;
using Smlp.Bsui.Common.Extensions;
using Smlp.Shared.Common.Constants;
using Smlp.Shared.Common.Extensions;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Documents.Commands.CreateDocument;
using Smlp.Shared.Documents.Queries.GetDocumentFile;
using Smlp.Shared.StudyPreparations.SponsorshipLetter.Commands.Update;
using Smlp.Shared.StudyPreparations.SponsorshipLetter.Queries;
using Syncfusion.Blazor.DocumentEditor;
using Syncfusion.DocIORenderer;

namespace Smlp.Bsui.Features.StudyPreparations.SponsorshipLetter.Components;

public partial class DraftSponsorshipLetter
{
    #region Parameter
    [Parameter]
    public Guid Id { get; set; }

    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;
    protected ClaimsPrincipal _user = default!;
    #endregion

    #region Protected Properties
    protected ErrorResponse? _error;
    protected ErrorResponse? _errorDraft;
    protected bool _isLoading = false;
    protected bool _isLoadingDialog = false;
    protected bool _isEnabledItem = false;
    protected bool _isEnabled = false;
    protected bool _disableButton = false;
    protected bool _disableButtonDocument = false;
    protected bool _disableButtonReject = false;
    protected bool _disableButtonDownload = false;
    protected string? _supportingDocument;
    protected string? _bringFamily = "";

    protected GetSponsorshipLetterByIdQueryResponse _dataItem = new();
    protected UpdateSponsorshipLetterRequest _update = new();
    #endregion

    #region Document Editor
    protected SfDocumentEditorContainer _containerDocument = default!;
    protected DocumentEditorSettingsModel _settingDocument = new() { ShowRuler = true };
    protected string[] _toolbarItems = DocumentEditorExtensions.ToolBarItems();
    internal bool ReadOnly { get; set; } = true;
    internal bool Toolbar { get; set; } = true;
    internal bool ShowProperties { get; set; } = false;
    protected Guid _documentDraftId = Guid.Empty;
    protected string _contentButton = "Update";
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
        await OnLoadData();
        _isLoading = false;
        StateHasChanged();
        await Task.CompletedTask;
    }

    private async Task OnLoadData()
    {
        _user = (await AuthenticationStateTask).User;
        var dataResponse = await _studyPreparationSponsorshipLetterService.GetStudyPreparationSponsorshipLetterByIdAsync(Id);
        if (dataResponse.Result == null || dataResponse.Result.Id == Guid.Empty)
        {
            _snackbar.AddError("Data not found!!");
            _navigationManager.NavigateTo(Smlp.Bsui.Features.StudyPreparations.Constants.RouteFor.StudyPreparationTab(4), true);
            StateHasChanged();
            return;
        }

        _dataItem = dataResponse.Result;
        _documentDraftId = _dataItem.DocumentDraftId ?? Guid.Empty;
        _bringFamily = _dataItem.IsBringFamily ? "Yes" : "No";

        _update.PlaceDateOfLetter = _dataItem.PlaceDateOfLetter;
        _update.NumberOfLetter = _dataItem.NumberOfLetter;
        _update.InvoiceAddressPosition = _dataItem.InvoiceAddressPosition;
        _update.InvoiceAddressName = _dataItem.InvoiceAddressName;
        _update.InvoiceAddressCompany = _dataItem.InvoiceAddressCompany;
        _update.MainPicPosition = _dataItem.MainPicPosition;
        _update.MainPicName = _dataItem.MainPicName;
        _update.MainPicEmail = _dataItem.MainPicEmail;
        _update.CarbonCopy = _dataItem.CarbonCopy;

        _disableButton = _dataItem.Status switch
        {
            SponsorshipLetterStatus.Accepted or SponsorshipLetterStatus.Draft or SponsorshipLetterStatus.Rejected => false,
            _ => true,
        };

        _disableButtonReject = _dataItem.Status switch
        {
            SponsorshipLetterStatus.ProcessToPOffice or SponsorshipLetterStatus.ProcessSign => false,
            _ => true,
        };

        if (_dataItem.DocumentId != Guid.Empty)
        {
            var resDoc = await _documentService.GetDocumentFileByDocumentTypeAsync(new GetDocumentFileRequest { DocumentId = _dataItem.DocumentId ?? Guid.Empty, DocumentType = DocumentType.SponsorshipLetter });
            if (resDoc.Error is null)
            {
                var dataDoc = resDoc.Result;
                _supportingDocument = "data:application/pdf;base64," + Convert.ToBase64String(dataDoc!.Content);
            }
        }
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

        var updateData = new UpdateSponsorshipLetterRequest
        {
            Id = _dataItem.Id,
            PlaceDateOfLetter = _update.PlaceDateOfLetter ?? "",
            NumberOfLetter = _update.NumberOfLetter ?? "",
            InvoiceAddressPosition = _update.InvoiceAddressPosition ?? "",
            InvoiceAddressName = _update.InvoiceAddressName ?? "",
            InvoiceAddressCompany = _update.InvoiceAddressCompany ?? "",
            MainPicPosition = _update.MainPicPosition ?? "",
            MainPicName = _update.MainPicName ?? "",
            MainPicEmail = _update.MainPicEmail ?? "",
            CarbonCopy = _update.CarbonCopy ?? "",
            DocumentDraftId = _documentDraftId,
            Status = SponsorshipLetterStatus.Draft
        };
        var updateDataResponse = await _studyPreparationSponsorshipLetterService.UpdateStudyPreparationSponsorshipLetterAsync(updateData);
        if (updateDataResponse.Error is not null)
        {
            _isLoadingDialog = false;
            StateHasChanged();
            var result = string.Join(", ", updateDataResponse.Error.Details.ToList());
            _snackbar.AddError("Error : " + result);
            return;
        }

        await OnLoadData();
        await OnLoadExist();
        _snackbar.AddSuccess("Success generate document draft!");
        _isLoadingDialog = false;
        await Task.CompletedTask;
    }

    public async Task OnClickBack()
    {
        _navigationManager.NavigateTo(Smlp.Bsui.Features.StudyPreparations.Constants.RouteFor.StudyPreparationTab(4), true);
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

    private async Task OnClickSaveAsync()
    {
        _isLoadingDialog = true;
        _contentButton = "Update";
        await BtnActionDocument();
        await OnUploadDocument();

        if (_documentDraftId == Guid.Empty)
        {
            _snackbar.AddWarning("Error upload file");
            _isLoadingDialog = false;
            StateHasChanged();
            return;
        }

        var updateData = new UpdateSponsorshipLetterRequest
        {
            Id = _dataItem.Id,
            PlaceDateOfLetter = _update.PlaceDateOfLetter ?? "",
            NumberOfLetter = _update.NumberOfLetter ?? "",
            InvoiceAddressPosition = _update.InvoiceAddressPosition ?? "",
            InvoiceAddressName = _update.InvoiceAddressName ?? "",
            InvoiceAddressCompany = _update.InvoiceAddressCompany ?? "",
            MainPicPosition = _update.MainPicPosition ?? "",
            MainPicName = _update.MainPicName ?? "",
            MainPicEmail = _update.MainPicEmail ?? "",
            CarbonCopy = _update.CarbonCopy ?? "",
            DocumentDraftId = _documentDraftId,
            Status = SponsorshipLetterStatus.Draft
        };
        var updateDataResponse = await _studyPreparationSponsorshipLetterService.UpdateStudyPreparationSponsorshipLetterAsync(updateData);
        if (updateDataResponse.Error is not null)
        {
            _isLoadingDialog = false;
            StateHasChanged();
            var result = string.Join(", ", updateDataResponse.Error.Details.ToList());
            _snackbar.AddError("Error : " + result);
            return;
        }

        await OnLoadData();
        await OnLoadExist();
        _snackbar.AddSuccess("Success update data!");
        _disableButtonDocument = _documentDraftId == Guid.Empty && _disableButton;
        _disableButtonDownload = false;
        _isLoadingDialog = false;
        await Task.CompletedTask;
    }

    private async Task OnUploadDocument()
    {
        var timeStamp = DateTime.Now.ToString("ddMMyyyyHHmmss");
        var filename = $"Sponsoship Letter_{_dataItem.EmployeeNumber}_{timeStamp}.docx";
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
                DocumentType = DocumentType.DraftSponsorshipLetter,
                File = formFile,
                EmployeeNumber = _dataItem.EmployeeNumber
            };

            var resultUpload = await _documentService.CreateDocumentAsync(requestFile);
            if (resultUpload.Error is null)
            {
                _documentDraftId = resultUpload.Result!.DocumentId;
            }
        }

        await Task.CompletedTask;
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

    private async Task OnClickRejectAsync()
    {
        VisiblePopupConfirm = true;
        TitlePopupConfirm = "<p style='padding:20px;'>Are you sure rejected this data?";
        TypeConfirm = "Rejected";
        await Task.CompletedTask;
    }

    private async Task OnClickDownloadWordAsync()
    {
        if (_documentDraftId == Guid.Empty)
        {
            _snackbar.AddError("Please generate draft document!!");
            return;
        }

        var doc = await _documentService.GetDocumentFileByDocumentTypeAsync(new GetDocumentFileRequest { DocumentId = _documentDraftId, DocumentType = DocumentType.DraftSponsorshipLetter });
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
            $"DraftSponsorshipLetter_{_dataItem.EmployeeNumber}_{DateTime.Now:yyyyMMdd}.pdf",
            $"{ContentTypes.ApplicationPdf}",
            content);
        downloadStream.Close();
        stream.Close();
    }
    #endregion

    #region Word Processing
    private async Task OnLoadDataGenerate()
    {
        var request = new GetDocumentFileRequest
        {
            DocumentId = Guid.Parse(_documentOptions.Value.DocumentDraftId.SturyPreparation.SponshorshipLetter),
            DocumentType = DocumentType.Template
        };

        var employeeId = $"00000000{_dataItem.EmployeeNumber}";
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

            var mrMs = gender.Equals("Male") ? "mr" : "ms";
            var herHis = gender.Equals("Male") ? "his" : "her";
            var heShe = gender.Equals("Male") ? "he" : "she";

            document.Replace("{{HerHisCapital}}", herHis.ToCapitalizeEachWord(), true, true);
            document.Replace("{{HerHis}}", herHis, true, true);
            document.Replace("{{HeSheCapital}}", heShe.ToCapitalizeEachWord(), true, true);
            document.Replace("{{HeShe}}", heShe, true, true);

            if (!string.IsNullOrEmpty(_dataItem.Name))
            {
                document.Replace("{{EmployeeName}}", $"{mrMs.ToCapitalizeEachWord()}. {_dataItem.Name.ToCapitalizeEachWord()}", true, true);
            }

            if (!string.IsNullOrEmpty(_dataItem.UniversityName))
            {
                document.Replace("{{UniversityName}}", _dataItem.UniversityName.ToCapitalizeEachWord(), true, true);
            }

            if (!string.IsNullOrEmpty(_dataItem.AddressTo))
            {
                document.Replace("{{AddressTo}}", _dataItem.AddressTo.ToCapitalizeEachWord(), true, true);
            }

            if (!string.IsNullOrEmpty(_dataItem.Study))
            {
                document.Replace("{{Study}}", _dataItem.Study.ToCapitalizeEachWord(), true, true);
            }

            if (!string.IsNullOrEmpty(_dataItem.AcademicYear))
            {
                document.Replace("{{AcademicYear}}", _dataItem.AcademicYear, true, true);
            }

            if (!string.IsNullOrEmpty(_dataItem.Country))
            {
                document.Replace("{{Country}}", _dataItem.Country.ToCapitalizeEachWord(), true, true);
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
        if (_dataItem.Status == SponsorshipLetterStatus.Completed)
        {
            _disableButtonDocument = true;
        }
        else if (_dataItem.Status is SponsorshipLetterStatus.ProcessSign or SponsorshipLetterStatus.ProcessToPOffice)
        {
            _disableButtonDocument = true;
        }

        if (_documentDraftId != Guid.Empty)
        {
            await OnLoadExist();
        }
        else if (_dataItem.Status != SponsorshipLetterStatus.Completed)
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
            DocumentType = DocumentType.DraftSponsorshipLetter
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
            DocumentType = DocumentType.DraftSponsorshipLetter
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
            DocumentId = Guid.Parse(_documentOptions.Value.DocumentDraftId.SturyPreparation.SponshorshipLetter),
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
        var status = SponsorshipLetterStatus.Draft;
        switch (TypeConfirm)
        {
            case "P-Sign":
                status = SponsorshipLetterStatus.ProcessSign;
                break;

            case "P-Office":
                status = SponsorshipLetterStatus.ProcessToPOffice;
                break;

            case "Rejected":
                status = SponsorshipLetterStatus.Rejected;
                break;
        }

        var updateData = new UpdateSponsorshipLetterStatusRequest
        {
            Id = _dataItem.Id,
            Status = status
        };

        var updateDataResponse = await _studyPreparationSponsorshipLetterService.UpdateStudyPreparationSponsorshipLetterStatusAsync(updateData);
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
        await OnLoadData();
        DialogConfirmClose();
        StateHasChanged();
        await Task.CompletedTask;
    }
    #endregion
}
