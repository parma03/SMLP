/*
* Pertamina Scholarship Management Portal
* Copyright (c) 2024-2025 PT. Pertamina Persero
*/

using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Psmp.Base.Selections.Enums;
using Psmp.Base.Studies.Enums;
using Psmp.Bsui.Common.Constants;
using Psmp.Bsui.Common.Extensions;
using Psmp.Shared.Applicants.Commands.Create;
using Psmp.Shared.Applicants.Queries.GetApplicant;
using Psmp.Shared.Applicants.Queries.GetApplicantStudyPreparation;
using Psmp.Shared.Common.Constants;
using Psmp.Shared.Common.Extensions;
using Psmp.Shared.Common.Responses;
using Psmp.Shared.Documents.Commands.CreateDocument;
using Psmp.Shared.Documents.Queries.GetDocumentFile;
using Psmp.Shared.Documents.Queries.GetDocuments;
using Psmp.Shared.StudyPreparations.Essay.Commands.Create;
using Psmp.Shared.StudyPreparations.Gmat.Commands.Create;
using Psmp.Shared.StudyPreparations.Gre.Commands.Create;
using Psmp.Shared.StudyPreparations.Ielts.Commands.Create;
using Psmp.Shared.StudyPreparations.SponsorshipLetter.Commands.Create;
using Psmp.Shared.StudyPreparations.VisaGuarantee.Commands.Create;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Inputs;

namespace Psmp.Bsui.Features.Applicants.Studies.Preparation;

public partial class PreparationDoc
{
    #region Parameter
    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

    protected ClaimsPrincipal _user = default!;
    #endregion

    #region Protected Properties
    public ErrorResponse? Error { get; set; }
    protected bool _isLoading = false;
    protected bool _isCanView = false;
    protected bool _isLoadingDialog = false;
    protected string _displayName = string.Empty;
    protected string _employeeId = string.Empty;
    protected string _username = string.Empty;
    protected Guid _lastApplicantId = Guid.Empty;
    protected long _maximumFileSizeInBytes = long.MaxValue;
    #endregion

    #region Data
    protected List<GetApplicantStudyPreparationByApplicantIdResponse> _dataGrid = new();
    protected GetApplicantByEmployeeIdResponse _lastApplicant = new();
    #endregion

    #region Modal Properties
    private string? ActionType { get; set; }
    private bool IsEnabled { get; set; }
    private bool IsDisabled { get; set; }
    private bool VisiblePopup { get; set; }
    private string TitlePopup { get; set; } = "Add";
    public Guid DocumentIdSupportDoc { get; set; } = new();
    public FileUploadResponse FileUploadSupportDoc { get; set; } = new();

    public List<DropdownItemResponse> _listBringFamily = new();
    public List<DropdownEnumResponse<StudyCategory>> _dropdownEnumCategories = new();
    protected CreateApplicantStudyPreparationRequest _createData = new();
    protected string? _documentPathSupportingDoc;
    protected List<GetDocumentsDocument> _getListDocuments = new();
    #endregion

    protected override async Task OnInitializedAsync()
    {
        _user = (await AuthenticationStateTask).User;
        _maximumFileSizeInBytes = _documentOptions.Value.MaximumFileSizeInBytes;
        _employeeId = _user.GetEmployeeId() ?? string.Empty;
        _displayName = _user.GetDisplayName() ?? string.Empty;

        _isLoading = true;
        _isCanView = false;

        #region bypass user applicant
        var isProductionEnvironment = _webHostEnvironment.IsInEnvironment(EnvironmentNames.Production);
        if (!isProductionEnvironment && _hrisOptions.Value.ByPass)
        {
            var asResponse = await _applicantByPassService.GetApplicantByPassEmployeeIdAsync(_user.GetEmployeeId() ?? string.Empty);
            if (asResponse.Result != null && asResponse.Result.EmployeeIdAlias != null)
            {
                _employeeId = asResponse.Result.EmployeeIdAlias;
                _username = asResponse.Result.EmailAlias;
            }
        }

        if (string.IsNullOrEmpty(_employeeId))
        {
            _snackbar.AddError("Not Found Employee ID");
            _isLoading = false;
            return;
        }
        #endregion bypass user applicant

        // check if exist data scholarship
        var applicant = await _applicantService.GetApplicantByEmployeeIdAsync(_employeeId);
        if (applicant.Error != null)
        {
            Error = applicant.Error;
        }
        else
        {
            var lastApplicant = applicant.Result!.Items.OrderByDescending(x => x.Created).FirstOrDefault();
            if (lastApplicant is not null && lastApplicant!.Id != Guid.Empty)
            {
                _isCanView = true;
                _lastApplicantId = lastApplicant!.Id;
                _lastApplicant = lastApplicant;
                await SetDataOnLoadAsync();
                await GetDropdownCategories();
                await GetDropdownBringFamily();
            }
        }

        _isLoading = false;
    }

    #region Data Grid
    private async Task SetDataOnLoadAsync()
    {
        var data = await _applicantStudyPreparationService.GetApplicantStudyPreparationByApplicantIdAsync(_lastApplicantId);
        if (data.Error is null)
        {
            _dataGrid = data.Result!.Items.ToList();
        }
        else
        {
            Error = data.Error;
        }
    }

    public void RowBound(RowDataBoundEventArgs<GetApplicantStudyPreparationByApplicantIdResponse> args)
    {
        if (args.Data.DocumentApprovedId is null || args.Data.DocumentApprovedId == Guid.Empty)
        {
            args.Row.AddClass(new string[] { "e-removeDeletecommand" });
        }
    }
    #endregion

    #region Dropdown
    private async Task GetDropdownCategories()
    {
        _dropdownEnumCategories = EnumExtentions.ToDropdownEnum<StudyCategory>();
        await Task.CompletedTask;
    }

    private async Task GetDropdownBringFamily()
    {
        var data = new List<DropdownItemResponse>()
        {
            new() {Id = 0, Description = "No" },
            new() {Id = 1, Description = "Yes"}
        };
        _listBringFamily = data;

        await Task.CompletedTask;
    }
    #endregion

    #region Command
    public async Task OnCommandClicked(CommandClickEventArgs<GetApplicantStudyPreparationByApplicantIdResponse> args)
    {
        _getListDocuments = new();
        if (args.CommandColumn.Type == CommandButtonType.None)
        {
            IsEnabled = false;
            IsDisabled = !IsEnabled;
            TitlePopup = "View";
            ActionType = "view";
            _createData = new CreateApplicantStudyPreparationRequest
            {
                Category = args.RowData.Category,
                TestCenter = args.RowData.TestCenter,
                LocationTest = args.RowData.TestLocation,
                TestDate = args.RowData.TestDate != null ? (DateOnly)args.RowData.TestDate : DateOnly.MinValue,
                Score = args.RowData.Score ?? 0,
                UniversityName = args.RowData.UniversityName,
                AddressTo = args.RowData.AddressTo,
                Study = args.RowData.Study,
                AcademicYear = args.RowData.AcademicYear,
                IsBringFamily = Convert.ToInt32(args.RowData.IsBringFamily),
                Country = args.RowData.Country,
                UniversityRank = args.RowData.UniversityRank,
                UniversityDueDate = args.RowData.UniversityDueDate,
                DocumentId = args.RowData.DocumentId,
                Comment = args.RowData.Comment ?? ""
            };

            if (args.RowData.DocumentId != Guid.Empty)
            {
                var docType = DocumentType.IELTSCertificate;
                var docApproved = DocumentType.FinalSponsorshipLetter;
                switch (args.RowData.Category)
                {
                    case StudyCategory.Essay:
                        docType = DocumentType.Essay;
                        break;

                    case StudyCategory.Ielts:
                        docType = DocumentType.IELTSCertificate;
                        break;

                    case StudyCategory.Gre:
                        docType = DocumentType.GRECertificate;
                        break;

                    case StudyCategory.Gmat:
                        docType = DocumentType.GMATCertificate;
                        break;

                    case StudyCategory.SponsorshipLetter:
                        docType = DocumentType.SponsorshipLetter;
                        docApproved = DocumentType.FinalSponsorshipLetter;
                        break;

                    case StudyCategory.VisaGuarantee:
                        docType = DocumentType.VisaGuarantee;
                        docApproved = DocumentType.FinalVisaGuarantee;
                        break;
                }

                // load document
                var docLoA = await _documentService.GetDocumentFileByDocumentTypeAsync(new GetDocumentFileRequest { DocumentId = args.RowData.DocumentId ?? Guid.Empty, DocumentType = docType });
                if (docLoA.Error is null)
                {
                    var dataDoc = docLoA.Result;
                    _getListDocuments.Add(new GetDocumentsDocument
                    {
                        Id = args.RowData.DocumentId!.Value,
                        FileName = dataDoc!.FileName ?? "",
                        DocumentType = docType
                    });
                }

                var resDocApp = await _documentService.GetDocumentFileByDocumentTypeAsync(new GetDocumentFileRequest { DocumentId = args.RowData.DocumentApprovedId ?? Guid.Empty, DocumentType = docApproved });
                if (resDocApp.Error is null)
                {
                    var dataDoc = resDocApp.Result;
                    _getListDocuments.Add(new GetDocumentsDocument
                    {
                        Id = args.RowData.DocumentApprovedId!.Value,
                        FileName = dataDoc!.FileName ?? "",
                        DocumentType = docApproved
                    });
                }
            }

            VisiblePopup = true;
        }
        else if (args.CommandColumn.Type == CommandButtonType.Edit)
        {
            if (args.RowData.DocumentId != Guid.Empty)
            {
                // for download document
                var docType = DocumentType.IELTSCertificate;
                switch (args.RowData.Category)
                {
                    case StudyCategory.Essay:
                        docType = DocumentType.Essay;
                        break;

                    case StudyCategory.Ielts:
                        docType = DocumentType.IELTSCertificate;
                        break;

                    case StudyCategory.Gre:
                        docType = DocumentType.GRECertificate;
                        break;

                    case StudyCategory.Gmat:
                        docType = DocumentType.GMATCertificate;
                        break;

                    case StudyCategory.SponsorshipLetter:
                        docType = DocumentType.SponsorshipLetter;
                        break;

                    case StudyCategory.VisaGuarantee:
                        docType = DocumentType.VisaGuarantee;
                        break;
                }

                await OnClickDownload(args.RowData.DocumentId ?? Guid.Empty, docType);
            }
        }
        else if (args.CommandColumn.Type == CommandButtonType.Delete)
        {
            if (args.RowData.DocumentApprovedId != Guid.Empty)
            {
                // for download document
                var docType = DocumentType.FinalSponsorshipLetter;
                var fileName = string.Empty;
                switch (args.RowData.Category)
                {
                    case StudyCategory.SponsorshipLetter:
                        docType = DocumentType.FinalSponsorshipLetter;
                        break;

                    case StudyCategory.VisaGuarantee:
                        docType = DocumentType.FinalVisaGuarantee;
                        break;
                }

                await OnClickDownload(args.RowData.DocumentApprovedId ?? Guid.Empty, docType);
            }
        }

        StateHasChanged();
        await Task.CompletedTask;
    }
    #endregion

    #region Document
    private async Task<string> UploadDocumentSupportDoc(DocumentType documentType)
    {
        var memoryStream = new MemoryStream();
        await FileUploadSupportDoc.File.OpenReadStream(_maximumFileSizeInBytes).CopyToAsync(memoryStream);
        memoryStream.Position = 0; // Reset the stream position

        var file = IBrowserFileExtensions.ToFormFile(FileUploadSupportDoc.File, memoryStream, FileUploadSupportDoc.File.Name);

        var requestFile = new CreateDocumentRequest
        {
            DocumentType = documentType,
            File = file,
            EmployeeNumber = _lastApplicant.EmployeeNumber
        };

        var resultUpload = await _documentService.CreateDocumentAsync(requestFile);
        if (resultUpload.Error is not null)
        {
            return string.Join(", ", resultUpload.Error.Details.ToList());
        }

        DocumentIdSupportDoc = resultUpload.Result!.DocumentId;
        return string.Empty;
    }

    private async Task OnDownload(Guid id, DocumentType docType)
    {
        var doc = await _documentService.GetDocumentFileByDocumentTypeAsync(new GetDocumentFileRequest { DocumentId = id, DocumentType = docType });
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
            dataDoc!.DocumentType,
            dataDoc!.Content);
    }
    #endregion

    #region Modal
    private void OnClickAddAsync()
    {
        TitlePopup = "Add";
        ActionType = "add";
        IsEnabled = true;
        IsDisabled = !IsEnabled;
        VisiblePopup = true;
        _documentPathSupportingDoc = string.Empty;
        _getListDocuments = new();
        _createData = new CreateApplicantStudyPreparationRequest { TestDate = DateOnly.FromDateTime(DateTime.Now), UniversityDueDate = DateOnly.FromDateTime(DateTime.Now), Category = StudyCategory.Ielts };
    }

    private void DialogOpen()
    {
        VisiblePopup = true;
    }

    private void DialogClose()
    {
        ActionType = "";
        VisiblePopup = false;
        _createData = new();
    }
    private void ClickedNo()
    {
        DialogClose();
    }

    private async Task ClickedYes()
    {
        _isLoadingDialog = true;
        if (FileUploadSupportDoc.FileName is null)
        {
            _snackbar.AddWarning("Please browse supporting document !");
            _isLoadingDialog = false;
            StateHasChanged();
            return;
        }

        if (ActionType == "add")
        {
            var resDoc = string.Empty;
            var countData = 0;
            switch (_createData.Category)
            {
                case StudyCategory.Essay:
                    // validation max Essay 2
                    countData = _dataGrid.Where(x => x.Category == StudyCategory.Essay).Count();
                    if (countData >= 2)
                    {
                        _snackbar.AddError("Study Preparation - Error : Maximum Essay is 2");
                        _isLoadingDialog = false;
                        StateHasChanged();
                        return;
                    }

                    resDoc = await UploadDocumentSupportDoc(DocumentType.Essay);
                    if (!string.IsNullOrEmpty(resDoc))
                    {
                        _snackbar.AddError("Study Preparation - Error : " + resDoc);
                        _isLoadingDialog = false;
                        StateHasChanged();
                        return;
                    }

                    var createDataEssay = new CreateEssayRequest
                    {
                        ApplicantId = _lastApplicantId,
                        DocumentId = DocumentIdSupportDoc,
                        IsSubmited = true
                    };
                    var createEssayRes = await _applicantStudyPreparationService.CreateEssayAsync(createDataEssay);
                    if (createEssayRes.Error is not null)
                    {
                        var result = string.Join(", ", createEssayRes.Error.Details.ToList());
                        _snackbar.AddError("Study Preparation - Error : " + result);
                        _isLoadingDialog = false;
                        StateHasChanged();
                        return;
                    }

                    _snackbar.AddSuccess("Success add data");
                    break;

                case StudyCategory.Ielts:
                    // validation max 2 in IELTS, GRE and GMAT
                    countData = _dataGrid.Where(x => x.Category == StudyCategory.Ielts).Count();
                    if (countData >= 2)
                    {
                        _snackbar.AddError("Study Preparation - Error : Maximum tests IELTS are allowed to be reimbursed is 2");
                        _isLoadingDialog = false;
                        StateHasChanged();
                        return;
                    }

                    resDoc = await UploadDocumentSupportDoc(DocumentType.IELTSCertificate);
                    if (!string.IsNullOrEmpty(resDoc))
                    {
                        _snackbar.AddError("Study Preparation - Error : " + resDoc);
                        _isLoadingDialog = false;
                        StateHasChanged();
                        return;
                    }

                    var createDataIelts = new CreateIeltsRequest
                    {
                        ApplicantId = _lastApplicantId,
                        TestCenter = _createData.TestCenter,
                        LocationTest = _createData.LocationTest,
                        TestDate = _createData.TestDate,
                        Score = _createData.Score,
                        DocumentId = DocumentIdSupportDoc
                    };
                    var createIeltsRes = await _applicantStudyPreparationService.CreateIeltsAsync(createDataIelts);
                    if (createIeltsRes.Error is not null)
                    {
                        var result = string.Join(", ", createIeltsRes.Error.Details.ToList());
                        _snackbar.AddError("Study Preparation - Error : " + result);
                        _isLoadingDialog = false;
                        StateHasChanged();
                        return;
                    }

                    _snackbar.AddSuccess("Success add data");
                    break;

                case StudyCategory.Gre:
                    // validation max 2 in IELTS, GRE and GMAT
                    countData = _dataGrid.Where(x => x.Category == StudyCategory.Gre).Count();
                    if (countData >= 2)
                    {
                        _snackbar.AddError("Study Preparation - Error : Maximum tests GRE are allowed to be reimbursed is 2");
                        _isLoadingDialog = false;
                        StateHasChanged();
                        return;
                    }

                    resDoc = await UploadDocumentSupportDoc(DocumentType.GRECertificate);
                    if (!string.IsNullOrEmpty(resDoc))
                    {
                        _snackbar.AddError("Study Preparation - Error : " + resDoc);
                        _isLoadingDialog = false;
                        StateHasChanged();
                        return;
                    }

                    var createDataGre = new CreateGreRequest
                    {
                        ApplicantId = _lastApplicantId,
                        TestCenter = _createData.TestCenter,
                        LocationTest = _createData.LocationTest,
                        TestDate = _createData.TestDate,
                        Score = _createData.Score,
                        DocumentId = DocumentIdSupportDoc
                    };
                    var createGreRes = await _applicantStudyPreparationService.CreateGreAsync(createDataGre);
                    if (createGreRes.Error is not null)
                    {
                        var result = string.Join(", ", createGreRes.Error.Details.ToList());
                        _snackbar.AddError("Study Preparation - Error : " + result);
                        _isLoadingDialog = false;
                        StateHasChanged();
                        return;
                    }

                    _snackbar.AddSuccess("Success add data");
                    break;

                case StudyCategory.Gmat:
                    // validation max 2 in IELTS, GRE and GMAT
                    countData = _dataGrid.Where(x => x.Category == StudyCategory.Gmat).Count();
                    if (countData >= 2)
                    {
                        _snackbar.AddError("Study Preparation - Error : Maximum tests GMAT are allowed to be reimbursed is 2");
                        _isLoadingDialog = false;
                        StateHasChanged();
                        return;
                    }

                    resDoc = await UploadDocumentSupportDoc(DocumentType.GMATCertificate);
                    if (!string.IsNullOrEmpty(resDoc))
                    {
                        _snackbar.AddError("Study Preparation - Error : " + resDoc);
                        _isLoadingDialog = false;
                        StateHasChanged();
                        return;
                    }

                    var createDataGmat = new CreateGmatRequest
                    {
                        ApplicantId = _lastApplicantId,
                        TestCenter = _createData.TestCenter,
                        LocationTest = _createData.LocationTest,
                        TestDate = _createData.TestDate,
                        Score = _createData.Score,
                        DocumentId = DocumentIdSupportDoc
                    };
                    var createGmatRes = await _applicantStudyPreparationService.CreateGmatAsync(createDataGmat);
                    if (createGmatRes.Error is not null)
                    {
                        var result = string.Join(", ", createGmatRes.Error.Details.ToList());
                        _snackbar.AddError("Applicant - Error : " + result);
                        _isLoadingDialog = false;
                        StateHasChanged();
                        return;
                    }

                    _snackbar.AddSuccess("Success add data");
                    break;

                case StudyCategory.SponsorshipLetter:
                    resDoc = await UploadDocumentSupportDoc(DocumentType.SponsorshipLetter);
                    if (!string.IsNullOrEmpty(resDoc))
                    {
                        _snackbar.AddError("Study Preparation - Error : " + resDoc);
                        _isLoadingDialog = false;
                        StateHasChanged();
                        return;
                    }

                    var createDataSponsorshipLetter = new CreateSponsorshipLetterRequest
                    {
                        ApplicantId = _lastApplicantId,
                        UniversityName = _createData.UniversityName,
                        AddressTo = _createData.AddressTo,
                        Study = _createData.Study,
                        IsBringFamily = _createData.IsBringFamily,
                        Country = _createData.Country,
                        AcademicYear = _createData.AcademicYear,
                        UniversityRank = _createData.UniversityRank,
                        UniversityDueDate = _createData.UniversityDueDate,
                        DocumentId = DocumentIdSupportDoc,
                        Comment = _createData.Comment
                    };
                    var createSponsorRes = await _applicantStudyPreparationService.CreateSponsorshipLetterAsync(createDataSponsorshipLetter);
                    if (createSponsorRes.Error is not null)
                    {
                        var result = string.Join(", ", createSponsorRes.Error.Details.ToList());
                        _snackbar.AddError("Study Preparation - Error : " + result);
                        _isLoadingDialog = false;
                        StateHasChanged();
                        return;
                    }

                    _snackbar.AddSuccess("Success add data");
                    break;

                case StudyCategory.VisaGuarantee:
                    countData = _dataGrid.Where(x => x.Category == StudyCategory.VisaGuarantee).Count();
                    if (countData > 0)
                    {
                        _snackbar.AddError("Study Preparation - Error : Maximum Visa Guarantee is 1");
                        _isLoadingDialog = false;
                        StateHasChanged();
                        return;
                    }

                    resDoc = await UploadDocumentSupportDoc(DocumentType.VisaGuarantee);
                    if (!string.IsNullOrEmpty(resDoc))
                    {
                        _snackbar.AddError("Study Preparation - Error : " + resDoc);
                        _isLoadingDialog = false;
                        StateHasChanged();
                        return;
                    }

                    var createDataVisa = new CreateVisaGuaranteeRequest
                    {
                        ApplicantId = _lastApplicantId,
                        UniversityName = _createData.UniversityName,
                        AddressTo = _createData.AddressTo,
                        Study = _createData.Study,
                        IsBringFamily = _createData.IsBringFamily,
                        Country = _createData.Country,
                        AcademicYear = _createData.AcademicYear,
                        UniversityRank = _createData.UniversityRank,
                        UniversityDueDate = _createData.UniversityDueDate,
                        DocumentId = DocumentIdSupportDoc,
                        Comment = _createData.Comment
                    };
                    var createVisaRes = await _applicantStudyPreparationService.CreateVisaGuaranteeAsync(createDataVisa);
                    if (createVisaRes.Error is not null)
                    {
                        var result = string.Join(", ", createVisaRes.Error.Details.ToList());
                        _snackbar.AddError("Study Preparation - Error : " + result);
                        _isLoadingDialog = false;
                        StateHasChanged();
                        return;
                    }

                    _snackbar.AddSuccess("Success add data");
                    break;
            }
        }

        await SetDataOnLoadAsync();
        StateHasChanged();
        DialogClose();
        _isLoadingDialog = false;
    }
    protected void OnRemoveUploaderSupportDoc(RemovingEventArgs args)
    {
        FileUploadSupportDoc = new();
        args.PostRawFile = false;
    }
    private async Task ValueChangeUploaderSupportDoc(UploadChangeEventArgs args)
    {
        FileUploadSupportDoc.FileName = args.Files.First().File.Name;
        FileUploadSupportDoc.FileSize = args.Files.First().File.Size;
        FileUploadSupportDoc.FileContentType = args.Files.First().File.ContentType;
        FileUploadSupportDoc.File = args.Files.First().File;

        await Task.CompletedTask;
    }

    public async void OnCommandClickedDocument(CommandClickEventArgs<GetDocumentsDocument> args)
    {
        if (args.CommandColumn.Type == CommandButtonType.None)
        {
            if (args.CommandColumn.Title == "Download")
            {
                await OnClickDownload(args.RowData.Id, args.RowData.DocumentType);
            }

        }
    }

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
