using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Psmp.Base.Selections.Enums;
using Psmp.Base.Studies.Enums;
using Psmp.Bsui.Common.Constants;
using Psmp.Bsui.Common.Extensions;
using Psmp.Shared.Common.Constants;
using Psmp.Shared.Common.Responses;
using Psmp.Shared.Documents.Commands.CreateDocument;
using Psmp.Shared.Documents.Queries.GetDocumentFile;
using Psmp.Shared.StudyPreparations.VisaGuarantee.Commands.Update;
using Psmp.Shared.StudyPreparations.VisaGuarantee.Queries;
using Syncfusion.Blazor.Inputs;

namespace Psmp.Bsui.Features.StudyPreparations.VisaGuarantee.Components;

public partial class FinalVisaGuarantee
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
    protected bool _isLoading = false;
    protected bool _isLoadingDialog = false;
    protected bool _isEnabled = false;
    protected string? _supportingDocument;
    protected string? _bringFamily = "";

    protected GetVisaGuaranteeByIdQueryResponse _dataItem = new();

    protected string? _finalDocument;
    public FileUploadResponse FileUploadFinal { get; set; } = new();
    protected Guid _documentApprovedId = Guid.Empty;
    protected long _maximumFileSizeInBytes = long.MaxValue;
    protected SfUploader _sfUploaderFinal = new();
    #endregion

    protected override async Task OnInitializedAsync()
    {
        _isLoading = false;
        _maximumFileSizeInBytes = _documentOptions.Value.MaximumFileSizeInBytes;
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
            _navigationManager.NavigateTo(Psmp.Bsui.Features.StudyPreparations.Constants.RouteFor.StudyPreparationTab(4), true);
            StateHasChanged();
            return;
        }

        _dataItem = dataResponse.Result;
        _bringFamily = _dataItem.IsBringFamily ? "Yes" : "No";
        if (_dataItem.DocumentId != Guid.Empty)
        {
            var resDoc = await _documentService.GetDocumentFileByDocumentTypeAsync(new GetDocumentFileRequest { DocumentId = _dataItem.DocumentId ?? Guid.Empty, DocumentType = DocumentType.VisaGuarantee });
            if (resDoc.Error is null)
            {
                var dataDoc = resDoc.Result;
                _supportingDocument = "data:application/pdf;base64," + Convert.ToBase64String(dataDoc!.Content);
            }
        }

        if (_dataItem.DocumentApprovedId != Guid.Empty)
        {
            _documentApprovedId = _dataItem.DocumentApprovedId ?? Guid.Empty;
            var resDoc = await _documentService.GetDocumentFileByDocumentTypeAsync(new GetDocumentFileRequest { DocumentId = _dataItem.DocumentApprovedId ?? Guid.Empty, DocumentType = DocumentType.FinalVisaGuarantee });
            if (resDoc.Error is null)
            {
                var dataDoc = resDoc.Result;
                _finalDocument = "data:application/pdf;base64," + Convert.ToBase64String(dataDoc!.Content);
            }
        }
    }

    public async Task OnClickBack()
    {
        _navigationManager.NavigateTo(Psmp.Bsui.Features.StudyPreparations.Constants.RouteFor.StudyPreparationTab(5), true);
        await Task.CompletedTask;
    }

    #region Document
    private async Task ValueChangeUploaderFinal(UploadChangeEventArgs args)
    {
        FileUploadFinal.FileName = args.Files.First().File.Name;
        FileUploadFinal.FileSize = args.Files.First().File.Size;
        FileUploadFinal.FileContentType = args.Files.First().File.ContentType;
        FileUploadFinal.File = args.Files.First().File;
        await Task.CompletedTask;
    }
    protected void OnRemoveUploaderFinal(RemovingEventArgs args)
    {
        FileUploadFinal = new();
        args.PostRawFile = false;
    }

    private async Task OnClickUploadFinalAsync(MouseEventArgs e)
    {
        _isLoadingDialog = true;
        if (FileUploadFinal.FileName is null)
        {
            _snackbar.AddWarning("Please browse document!");
            _isLoadingDialog = false;
            StateHasChanged();
            return;
        }

        await OnUploadDocumentFinal();
        if (_documentApprovedId == Guid.Empty)
        {
            _snackbar.AddWarning("Error upload file");
            _isLoadingDialog = false;
            StateHasChanged();
            return;
        }

        // update
        var updateData = new UpdateVisaGuaranteeDocumentApprovedRequest
        {
            Id = _dataItem.Id,
            DocumentApprovedId = _documentApprovedId,
            Status = VisaGuaranteeStatus.Completed
        };

        var updateDataResponse = await _studyPreparationVisaGuaranteeService.UpdateStudyPreparationVisaGuaranteeDocumentApprovedAsync(updateData);
        if (updateDataResponse.Error is not null)
        {
            _isLoadingDialog = false;
            StateHasChanged();
            var result = string.Join(", ", updateDataResponse.Error.Details.ToList());
            _snackbar.AddError("Error : " + result);
            return;
        }

        await OnLoadData();
        _isLoadingDialog = false;

        FileUploadFinal = new();
        await _sfUploaderFinal.RemoveAsync();

        StateHasChanged();
        await Task.CompletedTask;
    }

    private async Task OnUploadDocumentFinal()
    {
        var memoryStream = new MemoryStream();
        await FileUploadFinal.File.OpenReadStream(_maximumFileSizeInBytes).CopyToAsync(memoryStream);
        memoryStream.Position = 0; // Reset the stream position

        var file = IBrowserFileExtensions.ToFormFile(FileUploadFinal.File, memoryStream, FileUploadFinal.File.Name);

        var requestFile = new CreateDocumentRequest
        {
            DocumentType = DocumentType.FinalVisaGuarantee,
            File = file,
            EmployeeNumber = _dataItem.EmployeeNumber
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

        _documentApprovedId = resultUpload.Result!.DocumentId;
        StateHasChanged();
        await Task.CompletedTask;
    }
    private async Task OnClickDownloadFinalAsync(MouseEventArgs e)
    {
        var doc = await _documentService.GetDocumentFileByDocumentTypeAsync(new GetDocumentFileRequest { DocumentId = _documentApprovedId, DocumentType = DocumentType.FinalVisaGuarantee });
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
            $"{ContentTypes.ApplicationPdf}",
            dataDoc!.Content);
    }
    #endregion
}
