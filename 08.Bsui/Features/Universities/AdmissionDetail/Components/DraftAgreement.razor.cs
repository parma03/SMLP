using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Psmp.Base.Payments.Enums;
using Psmp.Base.Selections.Enums;
using Psmp.Base.Universities.Enums;
using Psmp.Bsui.Common.Constants;
using Psmp.Bsui.Common.Extensions;
using Psmp.Shared.Common.Constants;
using Psmp.Shared.Common.Extensions;
using Psmp.Shared.Common.Responses;
using Psmp.Shared.Documents.Commands.CreateDocument;
using Psmp.Shared.Documents.Queries.GetDocumentFile;
using Psmp.Shared.Masters.Queries;
using Psmp.Shared.Universities.AdmissionAgreement.Commands.Create;
using Psmp.Shared.Universities.AdmissionAgreement.Commands.Update;
using Psmp.Shared.Universities.AdmissionAgreement.Queries;
using Psmp.Shared.Universities.AdmissionSuperiorLetter.Queries;
using Syncfusion.Blazor.DocumentEditor;
using Syncfusion.DocIORenderer;

namespace Psmp.Bsui.Features.Universities.AdmissionDetail.Components;

public partial class DraftAgreement
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
    protected bool _isEnabled = false;
    protected bool _disableButton = false;
    protected bool _disableButtonDocument = false;
    protected bool _disableButtonReject = false;
    protected bool _disableButtonDownload = false;

    protected bool _isVisibleConfirmation = false;
    protected string? _typeConfirmation = "";
    protected string? _titleConfirmation = "";
    protected string? _contentConfirmation = "";

    protected string? _contentTooltipSuperior = "";
    protected string? _contentTooltipStudyStartDate = "";
    protected string? _contentTooltipStudyEndDate = "";
    protected string? _contentTooltipNummberOfLetter = "";

    protected SfDocumentEditorContainer _containerDocument = default!;
    protected DocumentEditorSettingsModel _settingDocument = new() { ShowRuler = true };
    protected GetAdmissionAgreementByAdmissionIdQueryResponse _data = new();
    protected string[] _toolbarItems = DocumentEditorExtensions.ToolBarItems();

    protected Guid _documentDraftId = Guid.Empty;
    protected string _contentButton = "Update";

    protected GetAdmissionSuperiorLetterByAdmissionIdQueryResponse _dataItem = new();
    protected List<GetItemStudyAssignmentResponse> _itemStudyAssignments = new();
    #endregion

    #region Dcoument
    internal bool ReadOnly { get; set; } = true;
    internal bool Toolbar { get; set; } = true;
    internal bool ShowProperties { get; set; } = false;
    #endregion

    protected override async void OnInitialized()
    {
        _isLoading = true;
        _user = (await AuthenticationStateTask).User;
        await OnLoadTooltip();
        await OnLoadData();
        await OnLoadDataSuperiorLetter();
        _isLoading = false;
        StateHasChanged();
    }

    #region OnLoad Data
    private async Task OnLoadData()
    {
        var data = await _universityAdmissionAgreementService.GetUniversityAdmissionAgreementByAdmissionIdAsync(Id);
        _error = data.Error;
        if (data.Error is null && data.Result != null && data.Result.AdmissionId != Guid.Empty)
        {
            _data = data.Result;
            _documentDraftId = _data.DocumentDraftId;
            _contentButton = "Update";

            // get status from admission agreement
            _disableButton = _data.DraftStatus switch
            {
                DocumentDraftStatus.Completed or DocumentDraftStatus.ProcessToPOffice or DocumentDraftStatus.ProcessToPSign => true,
                _ => false,
            };

            _disableButtonReject = _data.DraftStatus switch
            {
                DocumentDraftStatus.ProcessToPOffice or DocumentDraftStatus.ProcessToPSign => false,
                _ => true,
            };
        }
        else
        {
            var dataAdmission = await _universityAdmissionService.GetUniversityAdmissionByIdAsync(Id);
            _error = dataAdmission.Error;
            if (dataAdmission.Error is null && dataAdmission.Result != null && dataAdmission.Result.Id != Guid.Empty)
            {
                _data = new GetAdmissionAgreementByAdmissionIdQueryResponse
                {
                    StudyStart = DateOnly.FromDateTime(DateTime.Now),
                    ApplicantId = dataAdmission.Result!.ApplicantId,
                    AdmissionId = dataAdmission.Result.Id,
                    EmployeeNumber = dataAdmission.Result.EmployeeNumber,
                    StudyEnd = DateOnly.FromDateTime(DateTime.Now)
                };

                _disableButtonDownload = true;
                _disableButtonReject = true;
            }
        }

        StateHasChanged();
        await Task.CompletedTask;
    }

    public async Task OnLoadDataSuperiorLetter()
    {
        var data = await _universityAdmissionSuperiorLetterService.GetUniversityAdmissionSuperiorLetterByAdmissionIdAsync(Id);
        if (data.Result != null)
        {
            _dataItem = data.Result;
        }

        await Task.CompletedTask;
    }

    private async Task OnLoadItemStudyAssignment()
    {
        var response = await _studyAssignmentService.GetStudyAssignmentsAsync();
        if (response.Error is null)
        {
            _itemStudyAssignments = response.Result!.Items.ToList();
        }
    }

    private async Task OnLoadTooltip()
    {
        _contentTooltipSuperior = @"Use tag {{SuperiorLetter}} and add to document. If use multiple lease use ; and use tag {{SuperiorLetter0}},...,{{SuperiorLetterN}} same split data";
        _contentTooltipStudyStartDate = @"Use tag {{StudyStartDate}} and add to document.";
        _contentTooltipStudyEndDate = @"Use tag {{StudyEndDate}} and add to document.";
        _contentTooltipNummberOfLetter = @"Use tag {{NumberOfLetter}} and add to document. If use multiple lease use ; and use tag {{NumberOfLetter0}},...,{{NumberOfLetterN}} same split data";

        await Task.CompletedTask;
    }
    #endregion
    public async Task OnClickBack()
    {
        _navigationManager.NavigateTo(Psmp.Bsui.Features.Universities.Constants.RouteFor.UniversityAdmissionTab(1), true);
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

        var createData = new CreateAdmissionAgreementRequest
        {
            AdmissionId = _data.AdmissionId,
            ApplicantId = _data.ApplicantId,
            StudyStart = _data.StudyStart,
            StudyEnd = _data.StudyEnd,
            NumberOfLetter = _data.NumberOfLetter ?? "-",
            SuperiorLetter = _data.SuperiorLetter ?? "-",
            DocumentDraftId = _documentDraftId,
            DraftStatus = DocumentDraftStatus.Saved
        };
        var createDataResponse = await _universityAdmissionAgreementService.CreateUniversityAdmissionAgreementAsync(createData);
        if (createDataResponse.Error is not null)
        {
            _isLoadingDialog = false;
            StateHasChanged();
            var result = string.Join(", ", createDataResponse.Error.Details.ToList());
            _snackbar.AddError("Applicant - Error : " + result);
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

        if (_data.Id == Guid.Empty)
        {
            var createData = new CreateAdmissionAgreementRequest
            {
                AdmissionId = _data.AdmissionId,
                ApplicantId = _data.ApplicantId,
                StudyStart = _data.StudyStart,
                StudyEnd = _data.StudyEnd,
                NumberOfLetter = _data.NumberOfLetter ?? "-",
                SuperiorLetter = _data.SuperiorLetter ?? "-",
                DocumentDraftId = _documentDraftId,
                DraftStatus = DocumentDraftStatus.Saved
            };
            var createDataResponse = await _universityAdmissionAgreementService.CreateUniversityAdmissionAgreementAsync(createData);
            if (createDataResponse.Error is not null)
            {
                _isLoadingDialog = false;
                StateHasChanged();
                var result = string.Join(", ", createDataResponse.Error.Details.ToList());
                _snackbar.AddError("Applicant - Error : " + result);
                return;
            }
        }
        else
        {
            var updateData = new UpdateAdmissionAgreementRequest
            {
                Id = _data.Id,
                AdmissionId = _data.AdmissionId,
                ApplicantId = _data.ApplicantId,
                StudyStart = _data.StudyStart,
                StudyEnd = _data.StudyEnd,
                NumberOfLetter = _data.NumberOfLetter ?? "-",
                SuperiorLetter = _data.SuperiorLetter ?? "-",
                DocumentDraftId = _documentDraftId,
                DraftStatus = DocumentDraftStatus.Saved
            };
            var updateDataResponse = await _universityAdmissionAgreementService.UpdateUniversityAdmissionAgreementAsync(updateData);
            if (updateDataResponse.Error is not null)
            {
                _isLoadingDialog = false;
                StateHasChanged();
                var result = string.Join(", ", updateDataResponse.Error.Details.ToList());
                _snackbar.AddError("Applicant - Error : " + result);
                return;
            }
        }

        _snackbar.AddSuccess($"Success update data!");
        await OnLoadData();
        await OnLoadExist();
        _disableButtonDocument = _documentDraftId == Guid.Empty && _disableButton;
        _disableButtonDownload = false;
        _isLoadingDialog = false;
        await Task.CompletedTask;
    }

    private async Task OnUploadDocument()
    {
        var timeStamp = DateTime.Now.ToString("ddMMyyyyHHmmss");
        var filename = $"Surat Perintah_{_data.EmployeeNumber}_{timeStamp}.docx";
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
                DocumentType = DocumentType.AdmissionAgreementLetter,
                File = formFile,
                EmployeeNumber = _data.EmployeeNumber
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

        var doc = await _documentService.GetDocumentFileByDocumentTypeAsync(new GetDocumentFileRequest { DocumentId = _documentDraftId, DocumentType = DocumentType.AdmissionAgreementLetter });
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
            $"DraftAgreement_{_data.EmployeeNumber}_{DateTime.Now:yyyyMMdd}.pdf",
            $"{ContentTypes.ApplicationPdf}",
            content);
        downloadStream.Close();
        stream.Close();
    }
    #endregion

    #region Confirmation
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
        var status = DocumentDraftStatus.Saved;
        if (_typeConfirmation is not null)
        {
            switch (_typeConfirmation)
            {
                case "Psign":
                    status = DocumentDraftStatus.ProcessToPSign;
                    break;

                case "Poffice":
                    status = DocumentDraftStatus.ProcessToPOffice;
                    break;

                case "Rejected":
                    status = DocumentDraftStatus.Rejected;
                    break;
            }

            var updateData = new UpdateAdmissionAgreementStatusRequest
            {
                Id = _data.Id,
                DraftStatus = status
            };
            var updateDataResponse = await _universityAdmissionAgreementService.UpdateUniversityAdmissionAgreementStatusAsync(updateData);
            if (updateDataResponse.Error is not null)
            {
                _isLoadingDialog = false;
                StateHasChanged();
                var result = string.Join(", ", updateDataResponse.Error.Details.ToList());
                _snackbar.AddError("Applicant - Error : " + result);
                return;
            }

            if (status is DocumentDraftStatus.ProcessToPSign or DocumentDraftStatus.ProcessToPOffice)
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
    private async Task OnLoadDataGenerate()
    {
        var request = new GetDocumentFileRequest
        {
            DocumentId = Guid.Parse(_documentOptions.Value.DocumentDraftId.UniversityRegistration.Aggrement),
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

            var year = DateTime.Now.Year.ToString();
            document.Replace("{{Year}}", year, true, true);

            if (!string.IsNullOrEmpty(_dataItem.Name))
            {
                document.Replace("{{EmployeeNameCapital}}", _dataItem.Name.ToUpper(), true, true);
                document.Replace("{{EmployeeName}}", _dataItem.Name.ToCapitalizeEachWord(), true, true);
            }

            if (!string.IsNullOrEmpty(_dataItem.EmployeeNumber))
            {
                document.Replace("{{EmployeeNumber}}", Convert.ToInt32(_dataItem.EmployeeNumber).ToString(), true, true);
            }

            if (!string.IsNullOrEmpty(_dataItem.Position))
            {
                document.Replace("{{EmployeePosition}}", _dataItem.Position.ToCapitalizeEachWord(), true, true);
            }

            if (!string.IsNullOrEmpty(_dataItem.Email))
            {
                document.Replace("{{Email}}", _dataItem.Email, true, true);
            }

            if (!string.IsNullOrEmpty(_dataItem.University))
            {
                document.Replace("{{UniversityNameCapital}}", _dataItem.University.ToUpper(), true, true);
                document.Replace("{{UniversityName}}", _dataItem.University.ToCapitalizeEachWord(), true, true);
            }

            if (!string.IsNullOrEmpty(_dataItem.Study))
            {
                document.Replace("{{StudyCapital}}", _dataItem.Study.ToUpper(), true, true);
                document.Replace("{{Study}}", _dataItem.Study.ToCapitalizeEachWord(), true, true);
            }

            if (!string.IsNullOrEmpty(_dataItem.Country))
            {
                document.Replace("{{Country}}", _dataItem.Country.ToCapitalizeEachWord(), true, true);
            }

            var startDate = _dataItem.StudyStart.ToDateTime(new TimeOnly()).AddDays(-14).ToLocalDisplayName();
            if (!string.IsNullOrEmpty(startDate))
            {
                document.Replace("{{StudyStartDate}}", startDate, true, true);
            }

            var endDate = _dataItem.StudyEnd.ToDateTime(new TimeOnly()).AddDays(7).ToLocalDisplayName();
            if (!string.IsNullOrEmpty(endDate))
            {
                document.Replace("{{StudyEndDate}}", endDate, true, true);
            }

            var winterAllowence = _itemStudyAssignments.Where(x => x.Id == (int)ItemStudyAssignment.WinterAllowance).FirstOrDefault();
            var equipmentAllowance = _itemStudyAssignments.Where(x => x.Id == (int)ItemStudyAssignment.PerlengkapanTugasBelajar).FirstOrDefault();

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
        if (_containerDocument != null)
        {
            _disableButtonDocument = _documentDraftId == Guid.Empty && !_disableButton;
            if (_data.DraftStatus == DocumentDraftStatus.Completed)
            {
                _disableButtonDocument = true;
            }
            else if (_data.DraftStatus is DocumentDraftStatus.ProcessToPSign or DocumentDraftStatus.ProcessToPOffice)
            {
                _disableButtonDocument = true;
            }

            if (_documentDraftId != Guid.Empty)
            {
                await OnLoadExist();
            }
            else if (_data.DraftStatus != DocumentDraftStatus.Completed)
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
            DocumentType = DocumentType.AdmissionAgreementLetter
        };

        var getDraftDocument = await _documentService.GetDocumentFileByDocumentTypeAsync(request);
        _errorDraft = getDraftDocument.Error;
        if (getDraftDocument.Error is null && _containerDocument != null)
        {
            var document = new Syncfusion.DocIO.DLS.WordDocument();
            Stream stream = new MemoryStream(getDraftDocument.Result!.Content);
            document.Open(stream, Syncfusion.DocIO.FormatType.Docx);
            stream.Dispose();
            if (!string.IsNullOrEmpty(_data.SuperiorLetter))
            {
                var superiorArr = _data.SuperiorLetter.Split(";");
                if (superiorArr.Count() > 1)
                {
                    var idx = 0;
                    foreach (var item in superiorArr)
                    {
                        document.Replace("{{SuperiorLetter" + idx + "}}", item, true, true);
                        idx++;
                    }
                }
                else
                {
                    document.Replace("{{SuperiorLetter}}", _data.SuperiorLetter, true, true);
                }
            }

            document.Replace("{{StudyStartDate}}", _data.StudyStart.ToString("dd MMMM yyyy"), true, true);
            document.Replace("{{StudyEndDate}}", _data.StudyEnd.ToString("dd MMMM yyyy"), true, true);

            if (!string.IsNullOrEmpty(_data.NumberOfLetter))
            {
                var numberOfLetterArr = _data.NumberOfLetter.Split(";");
                if (numberOfLetterArr.Count() > 1)
                {
                    var idx = 0;
                    foreach (var item in numberOfLetterArr)
                    {
                        document.Replace("{{NumberOfLetter" + idx + "}}", item, true, true);
                        idx++;
                    }
                }
                else
                {
                    document.Replace("{{NumberOfLetter}}", _data.NumberOfLetter, true, true);
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
            DocumentType = DocumentType.AdmissionAgreementLetter
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
            DocumentId = Guid.Parse(_documentOptions.Value.DocumentDraftId.UniversityRegistration.Aggrement),
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
