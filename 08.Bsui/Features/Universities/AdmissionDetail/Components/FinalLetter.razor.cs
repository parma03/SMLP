using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Smlp.Base.Selections.Enums;
using Smlp.Base.Universities.Enums;
using Smlp.Bsui.Common.Constants;
using Smlp.Bsui.Common.Extensions;
using Smlp.Shared.Common.Constants;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Documents.Commands.CreateDocument;
using Smlp.Shared.Documents.Queries.GetDocumentFile;
using Smlp.Shared.Universities.Admission.Commands.Update;
using Smlp.Shared.Universities.AdmissionAgreement.Commands.Create;
using Smlp.Shared.Universities.AdmissionAgreement.Commands.Update;
using Smlp.Shared.Universities.AdmissionAgreement.Queries;
using Smlp.Shared.Universities.AdmissionWarrant.Commands.Create;
using Smlp.Shared.Universities.AdmissionWarrant.Commands.Update;
using Smlp.Shared.Universities.AdmissionWarrant.Queries;
using Syncfusion.Blazor.Inputs;

namespace Smlp.Bsui.Features.Universities.AdmissionDetail.Components;

public partial class FinalLetter
{
    #region Parameter
    [Parameter]
    public Guid Id { get; set; }

    [Parameter]
    public bool IsRead { get; set; } = false;

    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;
    protected ClaimsPrincipal _user = default!;
    #endregion

    #region Protected Properties
    protected long _maximumFileSizeInBytes = long.MaxValue;
    protected ErrorResponse? _error;
    protected bool _isLoading = false;
    protected bool _isLoadingDialog = false;

    protected string? _documentPathWarrant;
    protected string? _documentPathAgreement;

    protected GetAdmissionWarrantByAdmissionIdQueryResponse _dataWarrant = new();
    protected GetAdmissionAgreementByAdmissionIdQueryResponse _dataAgreement = new();
    protected Guid _documentApprovedWarrantId = new();
    protected Guid _documentApprovedAgreementId = new();
    protected SfUploader _sfUploaderWarrant = new();
    protected SfUploader _sfUploaderAgreement = new();
    #endregion

    #region Public Properties
    public FileUploadResponse FileUploadWarrant { get; set; } = new();
    public FileUploadResponse FileUploadAgreement { get; set; } = new();
    #endregion

    protected override async Task OnInitializedAsync()
    {
        _isLoading = true;
        _user = (await AuthenticationStateTask).User;
        _maximumFileSizeInBytes = _documentOptions.Value.MaximumFileSizeInBytes;
        await OnLoadData();
        await OnLoadDocument();
        _isLoading = false;
        await Task.CompletedTask;
    }

    #region OnLoad Data
    private async Task OnLoadData()
    {
        var dataWarrant = await _universityAdmissionWarrantService.GetUniversityAdmissionWarrantByAdmissionIdAsync(Id);
        if (dataWarrant.Result != null && dataWarrant.Result.AdmissionId != Guid.Empty)
        {
            _dataWarrant = dataWarrant.Result;
        }
        else
        {
            var dataAdmission = await _universityAdmissionService.GetUniversityAdmissionByIdAsync(Id);
            if (dataAdmission.Result != null && dataAdmission.Result.Id != Guid.Empty)
            {
                _dataWarrant = new GetAdmissionWarrantByAdmissionIdQueryResponse
                {
                    StudyStart = DateOnly.FromDateTime(DateTime.Now),
                    ApplicantId = dataAdmission.Result.ApplicantId,
                    AdmissionId = dataAdmission.Result.Id,
                    StudyEnd = DateOnly.FromDateTime(DateTime.Now)
                };
            }
        }

        var dataAgreement = await _universityAdmissionAgreementService.GetUniversityAdmissionAgreementByAdmissionIdAsync(Id);
        if (dataAgreement.Result != null && dataAgreement.Result.AdmissionId != Guid.Empty)
        {
            _dataAgreement = dataAgreement.Result;
        }
        else
        {
            var dataAdmission = await _universityAdmissionService.GetUniversityAdmissionByIdAsync(Id);
            if (dataAdmission.Result != null && dataAdmission.Result.Id != Guid.Empty)
            {
                _dataAgreement = new GetAdmissionAgreementByAdmissionIdQueryResponse
                {
                    StudyStart = DateOnly.FromDateTime(DateTime.Now),
                    ApplicantId = dataAdmission.Result.ApplicantId,
                    AdmissionId = dataAdmission.Result.Id,
                    StudyEnd = DateOnly.FromDateTime(DateTime.Now)
                };
            }
        }

        StateHasChanged();
        await Task.CompletedTask;
    }

    public async Task OnLoadDocument()
    {
        if (_dataWarrant is not null && _dataWarrant.DocumentApprovedId != Guid.Empty)
        {
            // load document
            var docWarrant = await _documentService.GetDocumentFileByDocumentTypeAsync(new GetDocumentFileRequest { DocumentId = (Guid)_dataWarrant.DocumentApprovedId, DocumentType = DocumentType.AdmissionWarrantApprovedLetter });
            if (docWarrant.Error is null)
            {
                var dataDoc = docWarrant.Result;
                _documentPathWarrant = "data:application/pdf;base64," + Convert.ToBase64String(dataDoc!.Content);
            }
            else
            {
                _isLoadingDialog = false;
                StateHasChanged();
                var result = string.Join(", ", docWarrant.Error.Details.ToList());
                _snackbar.AddError("Error : " + result);
                return;
            }
        }

        if (_dataAgreement is not null && _dataAgreement.DocumentApprovedId != Guid.Empty)
        {
            // load document
            var docAgreement = await _documentService.GetDocumentFileByDocumentTypeAsync(new GetDocumentFileRequest { DocumentId = (Guid)_dataAgreement.DocumentApprovedId, DocumentType = DocumentType.AdmissionAgreementApprovedLetter });
            if (docAgreement.Error is null)
            {
                var dataDoc = docAgreement.Result;
                _documentPathAgreement = "data:application/pdf;base64," + Convert.ToBase64String(dataDoc!.Content);
            }
            else
            {
                _isLoadingDialog = false;
                StateHasChanged();
                var result = string.Join(", ", docAgreement.Error.Details.ToList());
                _snackbar.AddError("Error : " + result);
                return;
            }
        }
    }
    #endregion
    public async Task OnClickBack()
    {
        _navigationManager.NavigateTo(Smlp.Bsui.Features.Universities.Constants.RouteFor.UniversityAdmissionTab(1), true);
        await Task.CompletedTask;
    }
    #region Document Warrant
    private async Task ValueChangeUploaderFinalWarrant(UploadChangeEventArgs args)
    {
        FileUploadWarrant.FileName = args.Files.First().File.Name;
        FileUploadWarrant.FileSize = args.Files.First().File.Size;
        FileUploadWarrant.FileContentType = args.Files.First().File.ContentType;
        FileUploadWarrant.File = args.Files.First().File;
        await Task.CompletedTask;
    }
    protected void OnRemoveUploaderFinalWarrant(RemovingEventArgs args)
    {
        FileUploadWarrant = new();
        args.PostRawFile = false;
    }

    private async Task OnClickUploadWarrantAsync(MouseEventArgs e)
    {
        _isLoadingDialog = true;

        if (FileUploadWarrant.FileName is null)
        {
            _snackbar.AddWarning("Please browse warrant document!");
            _isLoadingDialog = false;
            StateHasChanged();
            return;
        }

        var addmissionApplicant = await _universityAdmissionService.GetUniversityAdmissionByApplicantIdAsync(_dataWarrant.ApplicantId);
        if (addmissionApplicant.Result is not null)
        {
            var dataAdmission = addmissionApplicant.Result.Items.Where(x => x.Status == AdmissionStatus.Completed && x.Id != _dataWarrant.AdmissionId).FirstOrDefault();
            if (dataAdmission != null)
            {
                _snackbar.AddWarning($"Can not upload because applicant have 1 admission completed : {dataAdmission.Id}.");
                _isLoadingDialog = false;
                StateHasChanged();
                return;
            }
        }

        await OnUploadDocumentWarrant();
        if (_documentApprovedWarrantId == Guid.Empty)
        {
            _snackbar.AddWarning("Error upload file");
            _isLoadingDialog = false;
            StateHasChanged();
            return;
        }

        if (_dataWarrant.Id == Guid.Empty)
        {
            var createData = new CreateAdmissionWarrantRequest
            {
                AdmissionId = _dataWarrant.AdmissionId,
                ApplicantId = _dataWarrant.ApplicantId,
                StudyStart = _dataWarrant.StudyStart,
                StudyEnd = _dataWarrant.StudyEnd,
                NumberOfLetter = string.Empty,
                SuperiorLetter = string.Empty,
                DocumentDraftId = _dataWarrant.DocumentDraftId,
                DocumentApprovedId = _documentApprovedWarrantId,
                DraftStatus = DocumentDraftStatus.Completed
            };
            var createDataResponse = await _universityAdmissionWarrantService.CreateUniversityAdmissionWarrantAsync(createData);
            if (createDataResponse.Error is not null)
            {
                _isLoadingDialog = false;
                StateHasChanged();
                var result = string.Join(", ", createDataResponse.Error.Details.ToList());
                _snackbar.AddError("Error : " + result);
                return;
            }
        }
        else
        {
            // update
            var updateData = new UpdateAdmissionWarrantDocumentApprovedRequest
            {
                Id = _dataWarrant.Id,
                DocumentApprovedId = _documentApprovedWarrantId,
                DraftStatus = DocumentDraftStatus.Completed
            };
            var updateDataResponse = await _universityAdmissionWarrantService.UpdateUniversityAdmissionWarrantDocumentApprovedAsync(updateData);
            if (updateDataResponse.Error is not null)
            {
                _isLoadingDialog = false;
                StateHasChanged();
                var result = string.Join(", ", updateDataResponse.Error.Details.ToList());
                _snackbar.AddError("Error : " + result);
                return;
            }
        }

        await OnLoadData();
        await OnLoadDocument();
        await OnUpdateAdmission();
        _isLoadingDialog = false;

        FileUploadWarrant = new();
        await _sfUploaderWarrant.RemoveAsync();

        StateHasChanged();
        await Task.CompletedTask;
    }

    private async Task OnUploadDocumentWarrant()
    {
        var memoryStream = new MemoryStream();
        await FileUploadWarrant.File.OpenReadStream(_maximumFileSizeInBytes).CopyToAsync(memoryStream);
        memoryStream.Position = 0; // Reset the stream position

        var file = IBrowserFileExtensions.ToFormFile(FileUploadWarrant.File, memoryStream, FileUploadWarrant.File.Name);

        var requestFile = new CreateDocumentRequest
        {
            DocumentType = DocumentType.AdmissionWarrantApprovedLetter,
            File = file,
            EmployeeNumber = _user.GetEmployeeId()
        };

        var resultUpload = await _documentService.CreateDocumentAsync(requestFile);
        if (resultUpload.Error is not null)
        {
            _isLoadingDialog = false;
            StateHasChanged();
            var result = string.Join(", ", resultUpload.Error.Details.ToList());
            _snackbar.AddError("Error : " + result);
            return;
        }

        _documentApprovedWarrantId = resultUpload.Result!.DocumentId;
        StateHasChanged();
        await Task.CompletedTask;
    }
    private async Task OnClickDownloadWarrantAsync(MouseEventArgs e)
    {
        await OnDownload((Guid)_dataWarrant.DocumentApprovedId, DocumentType.AdmissionWarrantApprovedLetter, "SuratPerintahFinal");
    }
    #endregion

    #region Agreement
    private async Task ValueChangeUploaderFinalAgreement(UploadChangeEventArgs args)
    {
        FileUploadAgreement.FileName = args.Files.First().File.Name;
        FileUploadAgreement.FileSize = args.Files.First().File.Size;
        FileUploadAgreement.FileContentType = args.Files.First().File.ContentType;
        FileUploadAgreement.File = args.Files.First().File;
        await Task.CompletedTask;
    }
    protected void OnRemoveUploaderFinalAgreement(RemovingEventArgs args)
    {
        FileUploadAgreement = new();
        args.PostRawFile = false;
    }
    private async Task OnClickUploadAgreementAsync(MouseEventArgs e)
    {
        _isLoadingDialog = true;

        if (FileUploadAgreement.FileName is null)
        {
            _snackbar.AddWarning("Please browse agreement document!");
            _isLoadingDialog = false;
            StateHasChanged();
            return;
        }

        var addmissionApplicant = await _universityAdmissionService.GetUniversityAdmissionByApplicantIdAsync(_dataWarrant.ApplicantId);
        if (addmissionApplicant.Result is not null)
        {
            var dataAdmission = addmissionApplicant.Result.Items.Where(x => x.Status == AdmissionStatus.Completed && x.Id != _dataWarrant.AdmissionId).FirstOrDefault();
            if (dataAdmission != null)
            {
                _snackbar.AddWarning($"Can not upload because applicant have 1 admission completed : {dataAdmission.Id}.");
                _isLoadingDialog = false;
                StateHasChanged();
                return;
            }
        }

        await OnUploadDocumentAgreement();
        if (_documentApprovedAgreementId == Guid.Empty)
        {
            _snackbar.AddWarning("Error upload file");
            _isLoadingDialog = false;
            StateHasChanged();
            return;
        }

        if (_dataAgreement.Id == Guid.Empty)
        {
            var createData = new CreateAdmissionAgreementRequest
            {
                AdmissionId = _dataAgreement.AdmissionId,
                ApplicantId = _dataAgreement.ApplicantId,
                StudyStart = _dataAgreement.StudyStart,
                StudyEnd = _dataAgreement.StudyEnd,
                NumberOfLetter = string.Empty,
                SuperiorLetter = string.Empty,
                DocumentDraftId = _dataAgreement.DocumentDraftId,
                DocumentApprovedId = _documentApprovedAgreementId,
                DraftStatus = DocumentDraftStatus.Completed
            };
            var createDataResponse = await _universityAdmissionAgreementService.CreateUniversityAdmissionAgreementAsync(createData);
            if (createDataResponse.Error is not null)
            {
                _isLoadingDialog = false;
                StateHasChanged();
                var result = string.Join(", ", createDataResponse.Error.Details.ToList());
                _snackbar.AddError("Error : " + result);
                return;
            }
        }
        else
        {
            // update
            var updateData = new UpdateAdmissionAgreementDocumentApprovedRequest
            {
                Id = _dataAgreement.Id,
                DocumentApprovedId = _documentApprovedAgreementId,
                DraftStatus = DocumentDraftStatus.Completed
            };
            var updateDataResponse = await _universityAdmissionAgreementService.UpdateUniversityAdmissionAgreementDocumentApprovedAsync(updateData);
            if (updateDataResponse.Error is not null)
            {
                _isLoadingDialog = false;
                StateHasChanged();
                var result = string.Join(", ", updateDataResponse.Error.Details.ToList());
                _snackbar.AddError("Error : " + result);
                return;
            }
        }

        await OnLoadData();
        await OnLoadDocument();
        await OnUpdateAdmission();
        _isLoadingDialog = false;

        FileUploadAgreement = new();
        await _sfUploaderAgreement.RemoveAsync();

        StateHasChanged();
        await Task.CompletedTask;
    }

    private async Task OnUploadDocumentAgreement()
    {
        var memoryStream = new MemoryStream();
        await FileUploadAgreement.File.OpenReadStream(_maximumFileSizeInBytes).CopyToAsync(memoryStream);
        memoryStream.Position = 0; // Reset the stream position

        var file = IBrowserFileExtensions.ToFormFile(FileUploadAgreement.File, memoryStream, FileUploadAgreement.File.Name);

        var requestFile = new CreateDocumentRequest
        {
            DocumentType = DocumentType.AdmissionAgreementApprovedLetter,
            File = file,
            EmployeeNumber = _user.GetEmployeeId()
        };

        var resultUpload = await _documentService.CreateDocumentAsync(requestFile);
        if (resultUpload.Error is not null)
        {
            var result = string.Join(", ", resultUpload.Error.Details.ToList());
            _snackbar.AddError("Error : " + result);
            return;
        }

        _documentApprovedAgreementId = resultUpload.Result!.DocumentId;
        StateHasChanged();
        await Task.CompletedTask;
    }
    private async Task OnClickDownloadAgreementAsync(MouseEventArgs e)
    {
        await OnDownload((Guid)_dataAgreement.DocumentApprovedId, DocumentType.AdmissionAgreementApprovedLetter, "SuratPerjanjianFinal");
    }
    #endregion

    #region Admission
    private async Task OnUpdateAdmission()
    {
        if (_dataAgreement.DocumentApprovedId != Guid.Empty && _dataWarrant.DocumentApprovedId != Guid.Empty)
        {
            // update admission
            var updateData = new UpdateAdmissionStatusRequest
            {
                Id = _dataWarrant.AdmissionId,
                Status = AdmissionStatus.Completed
            };

            var updateDataResponse = await _universityAdmissionService.UpdateUniversityAdmissionStatusAsync(updateData);
            if (updateDataResponse.Error is not null)
            {
                _isLoadingDialog = false;
                StateHasChanged();
                var result = string.Join(", ", updateDataResponse.Error.Details.ToList());
                _snackbar.AddError("Error : " + result);
                return;
            }
        }
    }
    #endregion

    #region Download
    private async Task OnDownload(Guid id, DocumentType docType, string fileName)
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
            $"{fileName}_{_user.GetEmployeeId()}_{DateTime.Now:yyyyMMdd}.pdf",
            $"{ContentTypes.ApplicationPdf}",
            dataDoc!.Content);
    }
    #endregion

}
