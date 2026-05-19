using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Smlp.Base.Payments.Enums;
using Smlp.Base.Selections.Enums;
using Smlp.Base.Studies.Enums;
using Smlp.Bsui.Common.Constants;
using Smlp.Bsui.Common.Extensions;
using Smlp.Shared.Common.Constants;
using Smlp.Shared.Common.Extensions;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Documents.Commands.CreateDocument;
using Smlp.Shared.Documents.Queries.GetDocumentFile;
using Smlp.Shared.Masters.Queries;
using Smlp.Shared.StudyPreparations.VisaGuarantee.Commands.Update;
using Smlp.Shared.StudyPreparations.VisaGuarantee.Queries;
using Syncfusion.Blazor.DocumentEditor;
using Syncfusion.DocIORenderer;

namespace Smlp.Bsui.Features.StudyPreparations.VisaGuarantee.Components;

public partial class DraftVisaGuarantee
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

    protected GetVisaGuaranteeByIdQueryResponse _dataItem = new();
    protected UpdateVisaGuaranteeRequest _update = new();
    protected List<GetItemStudyAssignmentResponse> _itemStudyAssignments = new();
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
        var dataResponse = await _studyPreparationVisaGuaranteeService.GetStudyPreparationVisaGuaranteeByIdAsync(Id);
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
            VisaGuaranteeStatus.Accepted or VisaGuaranteeStatus.Draft or VisaGuaranteeStatus.Rejected => false,
            _ => true,
        };

        _disableButtonReject = _dataItem.Status switch
        {
            VisaGuaranteeStatus.ProcessToPOffice or VisaGuaranteeStatus.ProcessSign => false,
            _ => true,
        };

        if (_dataItem.DocumentId != Guid.Empty)
        {
            var resDoc = await _documentService.GetDocumentFileByDocumentTypeAsync(new GetDocumentFileRequest { DocumentId = _dataItem.DocumentId ?? Guid.Empty, DocumentType = DocumentType.VisaGuarantee });
            if (resDoc.Error is null)
            {
                var dataDoc = resDoc.Result;
                _supportingDocument = "data:application/pdf;base64," + Convert.ToBase64String(dataDoc!.Content);
            }
        }
    }

    private async Task OnLoadItemStudyAssignment()
    {
        var response = await _studyAssignmentService.GetStudyAssignmentsAsync();
        if (response.Error is null)
        {
            _itemStudyAssignments = response.Result!.Items.ToList();
        }
    }

    public async Task OnClickBack()
    {
        _navigationManager.NavigateTo(Smlp.Bsui.Features.StudyPreparations.Constants.RouteFor.StudyPreparationTab(5), true);
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

        var updateData = new UpdateVisaGuaranteeRequest
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
            Status = VisaGuaranteeStatus.Draft
        };
        var updateDataResponse = await _studyPreparationVisaGuaranteeService.UpdateStudyPreparationVisaGuaranteeAsync(updateData);
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

        var updateData = new UpdateVisaGuaranteeRequest
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
            Status = VisaGuaranteeStatus.Draft
        };
        var updateDataResponse = await _studyPreparationVisaGuaranteeService.UpdateStudyPreparationVisaGuaranteeAsync(updateData);
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
        var filename = $"Visa Guarantee_{_dataItem.EmployeeNumber}_{timeStamp}.docx";
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
                DocumentType = DocumentType.DraftVisaGuarantee,
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

        var doc = await _documentService.GetDocumentFileByDocumentTypeAsync(new GetDocumentFileRequest { DocumentId = _documentDraftId, DocumentType = DocumentType.DraftVisaGuarantee });
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
            $"DraftVisaGuarantee_{_dataItem.EmployeeNumber}_{DateTime.Now:yyyyMMdd}.pdf",
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
            DocumentId = Guid.Parse(_documentOptions.Value.DocumentDraftId.SturyPreparation.VisaGuarantee),
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

        await OnLoadItemStudyAssignment();
        if (_itemStudyAssignments.Count() == 0)
        {
            _snackbar.AddError("Item Study Assignment not found!");
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

            var winterAllowence = _itemStudyAssignments.Where(x => x.Id == (int)ItemStudyAssignment.WinterAllowance).FirstOrDefault();
            var apartmentAllowance = _itemStudyAssignments.Where(x => x.Id == (int)ItemStudyAssignment.ApartmentAllowance).FirstOrDefault();
            var equipmentAllowance = _itemStudyAssignments.Where(x => x.Id == (int)ItemStudyAssignment.PerlengkapanTugasBelajar).FirstOrDefault();
            var livingAllowance = _itemStudyAssignments.Where(x => x.Id == (int)ItemStudyAssignment.LivingAllowance).FirstOrDefault();

            var valWinterAllowence = "";
            if (winterAllowence is not null)
            {
                var _ = winterAllowence.StudyAssignmentFee.Where(x => !string.IsNullOrEmpty(x.Country) && x.Country!.ToLower().Equals(_dataItem.Country.ToLower())).OrderByDescending(x => x.Fee).FirstOrDefault();
                if (_ is null)
                {
                    _ = winterAllowence.StudyAssignmentFee.OrderByDescending(x => x.Fee).FirstOrDefault();
                }

                if (_ is not null)
                {
                    valWinterAllowence = $"{_.CurrencyType.GetDescription().ToUpper()} {_.Fee.ToCurrency()}";
                }
            }

            document.Replace("{{WinterAllowance}}", valWinterAllowence, true, true);

            var valApartmentAllowence = "";
            if (apartmentAllowance is not null)
            {
                var _ = apartmentAllowance.StudyAssignmentFee.Where(x => !string.IsNullOrEmpty(x.Country) && x.Country!.ToLower().Equals(_dataItem.Country.ToLower())).OrderByDescending(x => x.Fee).FirstOrDefault();
                if (_ is null)
                {
                    _ = apartmentAllowance.StudyAssignmentFee.OrderByDescending(x => x.Fee).FirstOrDefault();
                }

                if (_ is not null)
                {
                    valApartmentAllowence = $"{_.CurrencyType.GetDescription().ToUpper()} {_.Fee.ToCurrency()}";
                }
            }

            document.Replace("{{ApartmentAllowance}}", valApartmentAllowence, true, true);

            var valEquipmentAllowence = "";
            if (equipmentAllowance is not null)
            {
                var _ = equipmentAllowance.StudyAssignmentFee.Where(x => !string.IsNullOrEmpty(x.Country) && x.Country!.ToLower().Equals(_dataItem.Country.ToLower())).OrderByDescending(x => x.Fee).FirstOrDefault();
                if (_ is null)
                {
                    _ = equipmentAllowance.StudyAssignmentFee.OrderByDescending(x => x.Fee).FirstOrDefault();
                }

                if (_ is not null)
                {
                    valEquipmentAllowence = $"{_.CurrencyType.GetDescription().ToUpper()} {_.Fee.ToCurrency()}";
                }
            }

            document.Replace("{{EquipmentAllowance}}", valEquipmentAllowence, true, true);

            var valLivingAllowence = "";
            if (livingAllowance is not null)
            {
                var _ = livingAllowance.StudyAssignmentFee.Where(x => !string.IsNullOrEmpty(x.Country) && x.Country!.ToLower().Equals(_dataItem.Country.ToLower())).OrderByDescending(x => x.Fee).FirstOrDefault();
                if (_ is null)
                {
                    _ = livingAllowance.StudyAssignmentFee.OrderByDescending(x => x.Fee).FirstOrDefault();
                }

                if (_ is not null)
                {
                    valLivingAllowence = $"{_.CurrencyType.GetDescription().ToUpper()} {_.Fee.ToCurrency()}";
                }
            }

            document.Replace("{{LivingAllowance}}", valLivingAllowence, true, true);

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
        if (_dataItem.Status == VisaGuaranteeStatus.Completed)
        {
            _disableButtonDocument = true;
        }
        else if (_dataItem.Status is VisaGuaranteeStatus.ProcessSign or VisaGuaranteeStatus.ProcessToPOffice)
        {
            _disableButtonDocument = true;
        }

        if (_documentDraftId != Guid.Empty)
        {
            await OnLoadExist();
        }
        else if (_dataItem.Status != VisaGuaranteeStatus.Completed)
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
            DocumentType = DocumentType.DraftVisaGuarantee
        };

        var getDraftDocument = await _documentService.GetDocumentFileByDocumentTypeAsync(request);
        _errorDraft = getDraftDocument.Error;
        if (getDraftDocument.Error is null && _containerDocument != null)
        {
            var document = new Syncfusion.DocIO.DLS.WordDocument();
            Stream stream = new MemoryStream(getDraftDocument.Result!.Content);
            document.Open(stream, Syncfusion.DocIO.FormatType.Docx);
            stream.Dispose();

            if (!string.IsNullOrEmpty(_dataItem.PlaceDateOfLetter))
            {
                var superiorArr = _dataItem.PlaceDateOfLetter.Split(";");
                if (superiorArr.Count() > 1)
                {
                    var idx = 0;
                    foreach (var item in superiorArr)
                    {
                        document.Replace("{{PlaceDate" + idx + "}}", item, true, true);
                        idx++;
                    }
                }
                else
                {
                    document.Replace("{{PlaceDate}}", _dataItem.PlaceDateOfLetter, true, true);
                }
            }

            if (!string.IsNullOrEmpty(_dataItem.NumberOfLetter))
            {
                var superiorArr = _dataItem.NumberOfLetter.Split(";");
                if (superiorArr.Count() > 1)
                {
                    var idx = 0;
                    foreach (var item in superiorArr)
                    {
                        document.Replace("{{NumberOfLetter" + idx + "}}", item, true, true);
                        idx++;
                    }
                }
                else
                {
                    document.Replace("{{NumberOfLetter}}", _dataItem.NumberOfLetter, true, true);
                }
            }

            if (!string.IsNullOrEmpty(_dataItem.InvoiceAddressPosition))
            {
                var superiorArr = _dataItem.InvoiceAddressPosition.Split(";");
                if (superiorArr.Count() > 1)
                {
                    var idx = 0;
                    foreach (var item in superiorArr)
                    {
                        document.Replace("{{InvoiceAddressPosition" + idx + "}}", item, true, true);
                        idx++;
                    }
                }
                else
                {
                    document.Replace("{{InvoiceAddressPosition}}", _dataItem.InvoiceAddressPosition, true, true);
                }
            }

            if (!string.IsNullOrEmpty(_dataItem.InvoiceAddressName))
            {
                var superiorArr = _dataItem.InvoiceAddressName.Split(";");
                if (superiorArr.Count() > 1)
                {
                    var idx = 0;
                    foreach (var item in superiorArr)
                    {
                        document.Replace("{{InvoiceAddressName" + idx + "}}", item, true, true);
                        idx++;
                    }
                }
                else
                {
                    document.Replace("{{InvoiceAddressName}}", _dataItem.InvoiceAddressName, true, true);
                }
            }

            if (!string.IsNullOrEmpty(_dataItem.InvoiceAddressCompany))
            {
                var superiorArr = _dataItem.InvoiceAddressCompany.Split(";");
                if (superiorArr.Count() > 1)
                {
                    var idx = 0;
                    foreach (var item in superiorArr)
                    {
                        document.Replace("{{InvoiceAddressCompany" + idx + "}}", item, true, true);
                        idx++;
                    }
                }
                else
                {
                    document.Replace("{{InvoiceAddressCompany}}", _dataItem.InvoiceAddressCompany, true, true);
                }
            }

            if (!string.IsNullOrEmpty(_dataItem.MainPicPosition))
            {
                var superiorArr = _dataItem.MainPicPosition.Split(";");
                if (superiorArr.Count() > 1)
                {
                    var idx = 0;
                    foreach (var item in superiorArr)
                    {
                        document.Replace("{{MainPicPosition" + idx + "}}", item, true, true);
                        idx++;
                    }
                }
                else
                {
                    document.Replace("{{MainPicPosition}}", _dataItem.MainPicPosition, true, true);
                }
            }

            if (!string.IsNullOrEmpty(_dataItem.MainPicName))
            {
                var superiorArr = _dataItem.MainPicName.Split(";");
                if (superiorArr.Count() > 1)
                {
                    var idx = 0;
                    foreach (var item in superiorArr)
                    {
                        document.Replace("{{MainPicName" + idx + "}}", item, true, true);
                        idx++;
                    }
                }
                else
                {
                    document.Replace("{{MainPicName}}", _dataItem.MainPicName, true, true);
                }
            }

            if (!string.IsNullOrEmpty(_dataItem.MainPicEmail))
            {
                var superiorArr = _dataItem.MainPicEmail.Split(";");
                if (superiorArr.Count() > 1)
                {
                    var idx = 0;
                    foreach (var item in superiorArr)
                    {
                        document.Replace("{{MainPicEmail" + idx + "}}", item, true, true);
                        idx++;
                    }
                }
                else
                {
                    document.Replace("{{MainPicEmail}}", _dataItem.MainPicEmail, true, true);
                }
            }

            if (!string.IsNullOrEmpty(_dataItem.CarbonCopy))
            {
                var superiorArr = _dataItem.CarbonCopy.Split(";");
                if (superiorArr.Count() > 1)
                {
                    var idx = 0;
                    foreach (var item in superiorArr)
                    {
                        document.Replace("{{CarbonCopy" + idx + "}}", item, true, true);
                        idx++;
                    }
                }
                else
                {
                    document.Replace("{{CarbonCopy}}", _dataItem.CarbonCopy, true, true);
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

    private async Task OnLoadDraft()
    {
        var request = new GetDocumentFileRequest
        {
            DocumentId = _documentDraftId,
            DocumentType = DocumentType.DraftVisaGuarantee
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
            DocumentId = Guid.Parse(_documentOptions.Value.DocumentDraftId.SturyPreparation.VisaGuarantee),
            DocumentType = DocumentType.Template
        };

        var getDraftDocument = await _documentService.GetDocumentFileByDocumentTypeAsync(request);
        _errorDraft = getDraftDocument.Error;
        if (getDraftDocument.Error is null && _containerDocument != null)
        {
            Stream stream = new MemoryStream(getDraftDocument.Result!.Content);
            var editor = _containerDocument.DocumentEditor;
            await editor.OpenAsync(stream, ImportFormatType.Docx);
            StateHasChanged();
        }

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
        var status = VisaGuaranteeStatus.Draft;
        switch (TypeConfirm)
        {
            case "P-Sign":
                status = VisaGuaranteeStatus.ProcessSign;
                break;

            case "P-Office":
                status = VisaGuaranteeStatus.ProcessToPOffice;
                break;

            case "Rejected":
                status = VisaGuaranteeStatus.Rejected;
                break;
        }

        var updateData = new UpdateVisaGuaranteeStatusRequest
        {
            Id = _dataItem.Id,
            Status = status
        };

        var updateDataResponse = await _studyPreparationVisaGuaranteeService.UpdateStudyPreparationVisaGuaranteeStatusAsync(updateData);
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
