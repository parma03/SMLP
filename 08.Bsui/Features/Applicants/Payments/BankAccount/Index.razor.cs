using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Smlp.Base.Selections.Enums;
using Smlp.Bsui.Common.Constants;
using Smlp.Bsui.Common.Extensions;
using Smlp.Shared.Common.Constants;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Documents.Commands.CreateDocument;
using Smlp.Shared.Documents.Queries.GetDocumentFile;
using Smlp.Shared.Payments.Commands;
using Smlp.Shared.Payments.Queries;
using Syncfusion.Blazor.Inputs;
namespace Smlp.Bsui.Features.Applicants.Payments.BankAccount;

public partial class Index
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

    protected ClaimsPrincipal _user = default!;
    [Parameter]
    public string? Action { get; set; }
    [Parameter]
    public string? EmployeeNumber { get; set; }
    public ErrorResponse? Error { get; set; }
    protected bool _isLoading = false;
    protected bool _isLoadingDialog = false;
    protected string _displayName = default!;
    protected string _type = default!;
    private bool _visible;
    private bool _isEnabled;
    private string _title = default!;
    protected long _maximumFileSizeInBytes;
    protected string _employeeId = string.Empty;
    protected string _username = string.Empty;
    protected string? _university = string.Empty;
    private BankAccountRequest _request = new();
    protected Guid? _documentId;
    private Guid _applicantId = default!;
    private List<GetApplicantBankAccountResponse>? _response = new();
    public FileUploadResponse FileUploads { get; set; } = new();

    protected GetApplicantBankAccountResponse? _responseUsd = new();
    protected GetApplicantBankAccountResponse? _responseIdr = new();
    protected GetApplicantBankAccountResponse? _responseOther = new();

    protected override async Task OnInitializedAsync()
    {
        _isLoading = true;
        _user = (await AuthenticationStateTask).User;
        _displayName = _user.GetDisplayName() ?? string.Empty;
        _maximumFileSizeInBytes = _documentOptions.Value.MaximumFileSizeInBytes;
        _title = "";
        if (EmployeeNumber is null)
        {
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
            else
            {
                _employeeId = _user.GetEmployeeId() ?? string.Empty;
            }

            if (string.IsNullOrEmpty(_employeeId))
            {
                _snackbar.AddError("Not Found Employee ID");
                _isLoading = false;
                return;
            }
        }
        else
        {
            _employeeId = EmployeeNumber;
        }
        #endregion bypass user applicant

        var applicat = await _applicantService.GetApplicantByEmployeeIdAsync(_employeeId);
        if (applicat.Error is null)
        {
            var data = applicat.Result!.Items.Where(x => x.Reimbursement).SingleOrDefault();
            if (data is not null)
            {
                _applicantId = data!.Id;
                _username = data!.Name;
                _university = data!.UniversityApplicant;

            }
            else
            {
                var firstdata = applicat.Result!.Items.FirstOrDefault();
                _applicantId = firstdata!.Id;
                _username = firstdata.Name;
            }
        }

        await LoadData();
    }
    private async Task LoadData()
    {

        var response = await _paymentService.GetApplicantBankAccountAsync(_applicantId);
        if (response.Error is null)
        {
            _response = response.Result!.Items.ToList();

            _responseUsd = _response.Where(x => x.Currency == "USD").FirstOrDefault()!;
            _responseIdr = _response.Where(x => x.Currency == "IDR").FirstOrDefault()!;
            _responseOther = _response.Where(x => x.Currency is not "USD" and not "IDR").FirstOrDefault()!;
        }

        _isLoading = false;
    }
    protected async Task OnClickAddBankAccountAsync(string type, GetApplicantBankAccountResponse? applicantBank)
    {
        if (applicantBank is null)
        {
            _request = new();
            _type = type;
            if (type == "OtherCurrency")
            {
                _title = $"Bank Account Other Currency";
            }
            else
            {
                _title = $"Bank Account {_type}";
            }

            _request.Currency = _type is not "USD" and not "IDR" ? "" : _type;
            _visible = true;
            _isEnabled = true;
        }
        else
        {
            if (applicantBank.Currency == "OtherCurrency")
            {
                _title = $"Bank Account Other Currency";
            }
            else
            {
                _title = $"Bank Account {applicantBank.Currency}";
            }

            _visible = true;
            _isEnabled = true;
            _request.BankBranch = applicantBank.BankBranch;
            _request.Currency = applicantBank.Currency;
            _request.BankCity = applicantBank.BankCity;
            _request.ApplicantId = applicantBank.ApplicantId;
            _request.BankName = applicantBank.BankName;
            _request.AccountNumber = applicantBank.AccountNumber;
            _request.AccountHolder = applicantBank.AccountHolder;
            _request.BankCountry = applicantBank.BankCountry;
            _request.DocumentId = applicantBank.DocumentId;
            _request.PhoneNumber = applicantBank.PhoneNumber;
            _request.Id = applicantBank.Id;
        }
    }
    private void DialogOpen()
    {
        _visible = true;
    }

    private void DialogClose()
    {
        _visible = false;
    }

    private void ClickedNo()
    {
        _visible = false;
    }
    private async Task ClickedYes()
    {
        _isLoadingDialog = true;
        StateHasChanged(); // update tampilan agar spinner muncul

        await UploadReimbursement(_employeeId);
        if ((_request.DocumentId == null || _request.DocumentId == Guid.Empty) && (_documentId == null || _documentId == Guid.Empty))
        {
            _snackbar.AddWarning("Please browse document of Account Number!");
            _isLoadingDialog = false;
            StateHasChanged();
            return;
        }

        if (_documentId != null && _documentId != Guid.Empty)
        {
            _request.DocumentId = _documentId;
        }

        _request.ApplicantId = _applicantId;
        if (_request.Id is null)
        {
            _request.DocumentId = _documentId;
            var response = await _paymentService.CreateBankAccountAsync(_request);
            if (response.Error is not null)
            {
                _snackbar.AddError(response.Error.Detail);
                _visible = false;
            }
            else
            {
                _snackbar.AddSuccess($"Success Create Bank Account");
                _visible = false;
                StateHasChanged();
            }
        }
        else
        {
            if (FileUploads.File is not null)
            {
                _request.DocumentId = (Guid)_documentId;
            }

            var response = await _paymentService.UpdateCreateBankAccountAsync(_request);
            if (response.Error is not null)
            {
                _snackbar.AddError(response.Error.Detail);
                _visible = false;
            }
            else
            {
                _snackbar.AddSuccess($"Success Update Bank Account");
                _visible = false;
                StateHasChanged();
            }
        }

        await LoadData();

        _isLoadingDialog = false;
        StateHasChanged(); // refresh tampilan setelah selesai
    }
    private async Task ViewBankAccount(GetApplicantBankAccountResponse? applicantBank)
    {
        _title = $"Bank Account {applicantBank.Currency}";
        _visible = true;
        _isEnabled = false;
        if (applicantBank is not null)
        {
            _request.BankBranch = applicantBank.BankBranch;
            _request.Currency = applicantBank.Currency;
            _request.BankCity = applicantBank.BankCity;
            _request.ApplicantId = applicantBank.ApplicantId;
            _request.BankName = applicantBank.BankName;
            _request.AccountNumber = applicantBank.AccountNumber;
            _request.AccountHolder = applicantBank.AccountHolder;
            _request.BankCountry = applicantBank.BankCountry;
            _request.PhoneNumber = applicantBank.PhoneNumber;
            _request.Id = applicantBank.Id;
        }
    }

    protected void ValueChange(UploadChangeEventArgs args)
    {
        FileUploads.FileName = args.Files.First().File.Name;
        FileUploads.FileSize = args.Files.First().File.Size;
        FileUploads.FileContentType = args.Files.First().File.ContentType;
        FileUploads.File = args.Files.First().File;
    }
    protected void OnRemove(RemovingEventArgs args)
    {
        FileUploads = new();
        args.PostRawFile = false;
    }
    private async Task<MemoryStream> UploadReimbursement(string employeeNumber)
    {
        var memoryStream = new MemoryStream();
        if (FileUploads.File is not null)
        {
            await FileUploads.File.OpenReadStream(_maximumFileSizeInBytes).CopyToAsync(memoryStream);
            memoryStream.Position = 0; // Reset the stream position

            var formFile = new FormFile(memoryStream, 0, FileUploads.File.Size, FileUploads.File.Name, FileUploads.File.Name)
            {
                Headers = new HeaderDictionary(),
                ContentType = FileUploads.File.ContentType
            };

            var file = IBrowserFileExtensions.ToFormFile(FileUploads.File, memoryStream, FileUploads.File.Name);

            var requestFile = new CreateDocumentRequest
            {
                DocumentType = DocumentType.BankAccount,
                File = file,
                EmployeeNumber = employeeNumber,
            };

            var resultUpload = await _documentService.CreateDocumentAsync(requestFile);
            if (resultUpload.Error is null)
            {
                _documentId = resultUpload.Result!.DocumentId;
                //_updateSelectionTOEFLRequest.DocumentId = resultUpload.Result!.DocumentId;
            }
            else
            {
                _snackbar.AddError(resultUpload.Error.Detail);
            }
        }

        return memoryStream;

    }
    protected async Task OnClickDownloadUSD()
    {
        if (_responseUsd == null)
        {
            _snackbar.AddError("No Document Found");
        }
        else
        {
            await DownloadDocument(_responseUsd!.DocumentId!.Value);
        }
    }
    protected async Task OnClickDownloadIDR()
    {
        if (_responseIdr == null)
        {
            _snackbar.AddError("No Document Found");
        }
        else
        {
            await DownloadDocument(_responseIdr!.DocumentId!.Value);
        }
    }
    protected async Task OnClickDownloadOther()
    {
        if (_responseOther == null)
        {
            _snackbar.AddError("No Document Found");
        }
        else
        {
            await DownloadDocument(_responseOther!.DocumentId!.Value);
        }
    }
    private async Task DownloadDocument(Guid documentId)
    {
        var request = new GetDocumentFileRequest
        {
            DocumentId = documentId,
            DocumentType = DocumentType.BankAccount,
        };

        var response = await _documentService.GetDocumentFileByDocumentTypeAsync(request);

        if (response.Error is not null)
        {
            var result = string.Empty;
            for (var i = 0; i < response.Error.Details.Count; i++)
            {
                result = result + response.Error.Details[i] + ",";
            }

            _snackbar.AddError(result);

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
}
