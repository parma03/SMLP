using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Psmp.Base.Selections.Enums;
using Psmp.Bsui.Common.Constants;
using Psmp.Bsui.Common.Extensions;
using Psmp.Shared.Applicants.Queries.GetApplicant;
using Psmp.Shared.Common.Constants;
using Psmp.Shared.Common.Responses;
using Psmp.Shared.Documents.Commands.CreateDocument;
using Psmp.Shared.Documents.Queries.GetDocumentFile;
using Psmp.Shared.Payments.Commands;
using Psmp.Shared.Payments.Queries;
using Syncfusion.Blazor.Inputs;

namespace Psmp.Bsui.Features.Applicants.Payments.Reimbursements;
public partial class FinalPayment
{
    #region Parameter
    [Parameter]
    public Guid Id { get; set; }
    [Parameter]
    public Guid ApplicantId { get; set; }

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

    protected GetApplicantReimbursementResponse _data = new();
    protected GetApplicantByEmployeeIdResponse _dataApplicant = new();
    protected ReimbursementRequest _requestReimbursement = new();

    protected string? _finalDocument;
    public FileUploadResponse FileUploadFinal { get; set; } = new();
    protected Guid _documentApprovedId = Guid.Empty;
    protected long _maximumFileSizeInBytes = long.MaxValue;
    protected SfUploader _sfUploaderFinal = new();
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
            _requestReimbursement.UniversityBank = _data.UniversityBank;
            _requestReimbursement.UniversityBankAccount = _data.UniversityBankAccount;
            _requestReimbursement.DocumentId = _data.Document.Id;
            _requestReimbursement.PONumber = _data.PONumber;
            _requestReimbursement.BADate = _data.BADate;
            _requestReimbursement.DueDate = _data.DueDate;
            _requestReimbursement.DocumentDraft = _data.DocumentDraftId;
            _requestReimbursement.BASTNumber = _data.BASTNumber;
            _requestReimbursement.PaymentStatus = (int)_data.PaymentStatus;
        }

        var responseDataApplicant = await _applicantService.GetApplicantByIdAsync(_data.ApplicantId);
        if (responseDataApplicant.Error is not null)
        {
            _snackbar.AddError("Applicant not found!");
            return;
        }

        _dataApplicant = responseDataApplicant.Result!.Items.FirstOrDefault();

        if (_data.DocumentApprovedId != Guid.Empty)
        {
            _documentApprovedId = _data.DocumentApprovedId ?? Guid.Empty;
            var resDoc = await _documentService.GetDocumentFileByDocumentTypeAsync(new GetDocumentFileRequest { DocumentId = _data.DocumentApprovedId ?? Guid.Empty, DocumentType = DocumentType.FinalTuitionFee });
            if (resDoc.Error is null)
            {
                var dataDoc = resDoc.Result;
                _finalDocument = "data:application/pdf;base64," + Convert.ToBase64String(dataDoc!.Content);
            }
        }
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
        _requestReimbursement.DocumentFinal = _documentApprovedId;
        _requestReimbursement.PaymentStatus = 11;
        var updateDataResponse = await _paymentService.UpdateReimbursementAsync(_requestReimbursement);
        if (updateDataResponse.Error is not null)
        {
            _isLoadingDialog = false;
            StateHasChanged();
            var result = string.Join(", ", updateDataResponse.Error.Details.ToList());
            _snackbar.AddError("Error : " + result);
            return;
        }

        if (_data.DocumentApprovedId == Guid.Empty || _data.DocumentApprovedId is null)
        {
            //await _emailService.SendMailSponsorshipLetterAsync(new SendEmailSponsorshipLetterRequest { Id = _dataItem.Id });
        }

        await LoadData();
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
            DocumentType = DocumentType.FinalTuitionFee,
            File = file,
            EmployeeNumber = _dataApplicant.EmployeeNumber
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
        var doc = await _documentService.GetDocumentFileByDocumentTypeAsync(new GetDocumentFileRequest { DocumentId = _documentApprovedId, DocumentType = DocumentType.FinalTuitionFee });
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
