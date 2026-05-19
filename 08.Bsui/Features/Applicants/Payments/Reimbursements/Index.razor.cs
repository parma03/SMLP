using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Psmp.Base.Common;
using Psmp.Base.Payments.Enums;
using Psmp.Base.Selections.Enums;
using Psmp.Bsui.Common.Constants;
using Psmp.Bsui.Common.Extensions;
using Psmp.Client.Services.BackEnd.Masters;
using Psmp.Shared.Common.Constants;
using Psmp.Shared.Common.Extensions;
using Psmp.Shared.Common.Responses;
using Psmp.Shared.Documents.Commands.CreateDocument;
using Psmp.Shared.Documents.Queries.GetDocumentFile;
using Psmp.Shared.Documents.Queries.GetDocuments;
using Psmp.Shared.Payments.Commands;
using Psmp.Shared.Payments.Queries;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Inputs;

namespace Psmp.Bsui.Features.Applicants.Payments.Reimbursements;
public partial class Index
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;
    [Inject]
    protected SetDataStatusService SetDataStatusService { get; set; } = default!;
    protected ClaimsPrincipal _user = default!;
    [Parameter]
    public string? Action { get; set; }
    [Parameter]
    public string? EmployeeNumber { get; set; }
    public ErrorResponse? Error { get; set; }
    protected bool _isLoading = false;
    protected bool _isLoadingDialog = false;
    protected string _displayName = default!;
    protected string? _university = string.Empty;
    protected string _accountNumber = default!;
    protected bool _bankAccountExist = false;
    protected DateOnly? DisabledFrom { get; set; }
    protected DateOnly? DisabledTo { get; set; }
    protected List<GetApplicantReimbursementResponse> _applicantReimbursement = new();
    private SfGrid<GetApplicantReimbursementResponse> Grid { get; set; } = new();
    protected List<GetDocumentsDocument> _getListDocuments = new();
    private readonly BankAccountRequest _requestBankAccount = new();
    private ReimbursementRequest _requestReimbursement = new();
    private List<DropdownItemResponse> _dropdownItemCategorys = new();
    private List<DropdownItemResponse> _dropdownItemCurrencyType = new();
    protected List<GetStepperResponse> _stepperData = new();

    private Guid _applicantId = default!;
    protected long _maximumFileSizeInBytes;
    private bool VisiblePopup { get; set; }
    private bool VisiblePopupReimbursement { get; set; }
    private string TitlePopupReimbursement { get; set; } = "Add Reimbursement";
    private bool IsEnabled { get; set; }

    protected string _employeeId = string.Empty;
    protected string _username = string.Empty;
    private List<GetPaymentCommentResponse>? _listPaymentComment = new();
    private bool VisiblePopupDelete { get; set; } = false;
    private string? DeleteType { get; set; }
    protected Guid? _documentId;
    public FileUploadResponse FileUploads { get; set; } = new();
    private bool IsAuploadedDocument { get; set; } = false;
    private int _activeTabIndexBacking;
    private int ActiveTabIndex
    {
        get => _activeTabIndexBacking;
        set
        {
            if (_activeTabIndexBacking != value)
            {
                _activeTabIndexBacking = value;
                OnTabChanged(value); // panggil handler kamu sendiri
            }
        }
    }
    private string _selectedTab = "Request";

    private decimal _countcurrency1;
    private decimal _countcurrency2;
    private decimal _countcurrency3;
    private decimal _countcurrency4;
    private decimal _countcurrency5;
    private decimal _countcurrency6;
    private decimal _countcurrency7;
    private decimal _countcurrency8;
    private decimal _countcurrency9;
    private decimal _countcurrency10;
    private decimal _countcurrency11;
    private decimal _countcurrency12;
    private decimal _countcurrency13;
    private decimal _countcurrency14;
    private int _count1;
    private int _count2;
    private int _count3;
    private int _count4;
    private int _count5;
    private int _count6;
    private int _count7;
    private int _count8;
    private int _count9;
    private int _count10;
    private int _count11;
    private int _count12;
    private int _count13;
    private int _count14;

    public void RowBound(RowDataBoundEventArgs<GetApplicantReimbursementResponse> args)
    {
        if (Action == null)
        {
            if (args.Data.PaymentStatus is not PaymentStatus.NeedRevision)
            {
                args.Row.AddClass(new string[] { "e-removeEditcommand" });
            }
        }
        else
        {
            //if (args.Data.CategoryId != 14)
            //{
            args.Row.AddClass(new string[] { "e-removeEditcommand" });
            //}
        }
    }
    protected override async Task OnInitializedAsync()
    {
        _user = (await AuthenticationStateTask).User;
        _displayName = _user.GetDisplayName() ?? string.Empty;
        _employeeId = _user.GetEmployeeId() ?? string.Empty;
        _maximumFileSizeInBytes = _documentOptions.Value.MaximumFileSizeInBytes;

        await GetDataStatusConfigAsync();

        //_dropdownItemCategorys = EnumExtentions.ToDropdownItem<ReimbursementCategory>();
        await GetCategoryReimbursemen();
        _dropdownItemCurrencyType = EnumExtentions.ToDropdownItem<CurrencyType>();
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

            if (string.IsNullOrEmpty(_employeeId))
            {
                _snackbar.AddError("Not Found Employee ID");
                _isLoading = false;
                return;
            }
            #endregion bypass user applicant
        }
        else
        {
            _employeeId = EmployeeNumber;
        }

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

        await SetDataOnLoadAsync();
    }

    private async Task GetDataStatusConfigAsync()
    {
        try
        {
            var response = await SetDataStatusService.GetSetDataStatusAsync();
            if (response.Error is null && response.Result is not null)
            {
                DisabledFrom = response.Result.DisabledFrom;
                DisabledTo = response.Result.DisabledTo;
            }
        }
        catch
        {
        }
    }
    private async Task SetDataOnLoadAsync()
    {
        var request = new GetApplicantReimbursementRequest()
        {
            ApplicantId = _applicantId
        };

        var response = await _paymentService.GetApplicantReimbursementAsync(request);
        if (response.Error is null)
        {
            _applicantReimbursement = response!.Result!.Items.ToList();
            _countcurrency1 = (decimal)_applicantReimbursement.Where(x => x.PaymentStatus == PaymentStatus.WaitingApproval).Where(x => x.CurrencyType == CurrencyType.Usd).Sum(x => x.Amount);
            _countcurrency2 = (decimal)_applicantReimbursement.Where(x => x.PaymentStatus == PaymentStatus.WaitingApproval).Where(x => x.CurrencyType == CurrencyType.Idr).Sum(x => x.Amount);
            _countcurrency3 = (decimal)_applicantReimbursement.Where(x => x.PaymentStatus == PaymentStatus.Pending).Where(x => x.CurrencyType == CurrencyType.Usd).Sum(x => x.Amount);
            _countcurrency4 = (decimal)_applicantReimbursement.Where(x => x.PaymentStatus == PaymentStatus.Pending).Where(x => x.CurrencyType == CurrencyType.Idr).Sum(x => x.Amount);
            _countcurrency5 = (decimal)_applicantReimbursement.Where(x => x.PaymentStatus == PaymentStatus.Rejected).Where(x => x.CurrencyType == CurrencyType.Usd).Sum(x => x.Amount);
            _countcurrency6 = (decimal)_applicantReimbursement.Where(x => x.PaymentStatus == PaymentStatus.Rejected).Where(x => x.CurrencyType == CurrencyType.Idr).Sum(x => x.Amount);
            _countcurrency7 = (decimal)_applicantReimbursement.Where(x => x.PaymentStatus == PaymentStatus.RequestPreparation).Where(x => x.CurrencyType == CurrencyType.Usd).Sum(x => x.Amount);
            _countcurrency8 = (decimal)_applicantReimbursement.Where(x => x.PaymentStatus == PaymentStatus.RequestPreparation).Where(x => x.CurrencyType == CurrencyType.Idr).Sum(x => x.Amount);
            //_countcurrency9 = (decimal)_applicantReimbursement.Where(x => x.PaymentStatus == PaymentStatus.RequestToBmc).Where(x => x.CurrencyType == CurrencyType.Usd).Sum(x => x.Amount);
            //_countcurrency10 = (decimal)_applicantReimbursement.Where(x => x.PaymentStatus == PaymentStatus.RequestToBmc).Where(x => x.CurrencyType == CurrencyType.Idr).Sum(x => x.Amount);
            _countcurrency11 = (decimal)_applicantReimbursement.Where(x => x.PaymentStatus == PaymentStatus.Completed).Where(x => x.CurrencyType == CurrencyType.Usd).Sum(x => x.Amount);
            _countcurrency12 = (decimal)_applicantReimbursement.Where(x => x.PaymentStatus == PaymentStatus.Completed).Where(x => x.CurrencyType == CurrencyType.Idr).Sum(x => x.Amount);
            _countcurrency13 = (decimal)_applicantReimbursement.Where(x => x.PaymentStatus == PaymentStatus.NeedRevision).Where(x => x.CurrencyType == CurrencyType.Idr).Sum(x => x.Amount);
            _countcurrency14 = (decimal)_applicantReimbursement.Where(x => x.PaymentStatus == PaymentStatus.NeedRevision).Where(x => x.CurrencyType == CurrencyType.Idr).Sum(x => x.Amount);

            _count1 = _applicantReimbursement.Where(x => x.PaymentStatus == PaymentStatus.WaitingApproval).Where(x => x.CurrencyType == CurrencyType.Usd).Count();
            _count2 = _applicantReimbursement.Where(x => x.PaymentStatus == PaymentStatus.WaitingApproval).Where(x => x.CurrencyType == CurrencyType.Idr).Count();
            _count3 = _applicantReimbursement.Where(x => x.PaymentStatus == PaymentStatus.Pending).Where(x => x.CurrencyType == CurrencyType.Usd).Count();
            _count4 = _applicantReimbursement.Where(x => x.PaymentStatus == PaymentStatus.Pending).Where(x => x.CurrencyType == CurrencyType.Idr).Count();
            _count5 = _applicantReimbursement.Where(x => x.PaymentStatus == PaymentStatus.Rejected).Where(x => x.CurrencyType == CurrencyType.Usd).Count();
            _count6 = _applicantReimbursement.Where(x => x.PaymentStatus == PaymentStatus.Rejected).Where(x => x.CurrencyType == CurrencyType.Idr).Count();
            _count7 = _applicantReimbursement.Where(x => x.PaymentStatus == PaymentStatus.RequestPreparation).Where(x => x.CurrencyType == CurrencyType.Usd).Count();
            _count8 = _applicantReimbursement.Where(x => x.PaymentStatus == PaymentStatus.RequestPreparation).Where(x => x.CurrencyType == CurrencyType.Idr).Count();
            //_count9 = _applicantReimbursement.Where(x => x.PaymentStatus == PaymentStatus.RequestToBmc).Where(x => x.CurrencyType == CurrencyType.Usd).Count();
            //_count10 = _applicantReimbursement.Where(x => x.PaymentStatus == PaymentStatus.RequestToBmc).Where(x => x.CurrencyType == CurrencyType.Idr).Count();
            _count11 = _applicantReimbursement.Where(x => x.PaymentStatus == PaymentStatus.Completed).Where(x => x.CurrencyType == CurrencyType.Usd).Count();
            _count12 = _applicantReimbursement.Where(x => x.PaymentStatus == PaymentStatus.Completed).Where(x => x.CurrencyType == CurrencyType.Idr).Count();
            _count13 = _applicantReimbursement.Where(x => x.PaymentStatus == PaymentStatus.NeedRevision).Where(x => x.CurrencyType == CurrencyType.Idr).Count();
            _count14 = _applicantReimbursement.Where(x => x.PaymentStatus == PaymentStatus.NeedRevision).Where(x => x.CurrencyType == CurrencyType.Idr).Count();
        }

        OnTabChanged(ActiveTabIndex);

    }
    //protected async Task OnClickAddBankAccountAsync()
    //{
    //    VisiblePopup = true;
    //    IsEnabled = true;
    //}
    //private void DialogOpen()
    //{
    //    VisiblePopup = true;
    //}

    //private void DialogClose()
    //{
    //    VisiblePopup = false;
    //}

    //private void ClickedNo()
    //{
    //    VisiblePopup = false;
    //}
    //private async Task ClickedYes()
    //{
    //    var bankAccount = _requestBankAccount;
    //    bankAccount.ApplicantId = _applicantId;

    //    var response = await _paymentService.CreateBankAccountAsync(bankAccount);
    //    if (response.Error is not null)
    //    {
    //        _snackbar.AddError(response.Error.Detail);
    //        VisiblePopup = false;
    //    }
    //    else
    //    {
    //        _snackbar.AddSuccess($"Success Create Bank Account");
    //        VisiblePopup = false;
    //        StateHasChanged();
    //    }
    //}

    private async Task GetCategoryReimbursemen()
    {
        var response = await _studyAssignmentService.GetCategoryReimbursemenAsync();
        if (response.Error is null)
        {
            _dropdownItemCategorys = response.Result!.Items.Select(x => new DropdownItemResponse { Id = x.Id, Description = x.Item }).ToList();
        }
    }
    private void OnClickAddAsync()
    {
        // reset
        _requestReimbursement = new();
        _getListDocuments = new();
        _listPaymentComment = new();
        _stepperData = new();
        FileUploads = new();
        _documentId = null;
        IsAuploadedDocument = false;
        // Set dialog
        VisiblePopupReimbursement = true;
        TitlePopupReimbursement = "Add Reimbursement";
        IsEnabled = true;
    }
    private void DialogReimbursementOpen()
    {
        VisiblePopupReimbursement = true;
    }

    private void DialogReimbursementClose()
    {
        VisiblePopupReimbursement = false;
    }

    private void ClickedReimbursementNo()
    {
        VisiblePopupReimbursement = false;
    }
    private async Task ClickedReimbursementYes()
    {
        if (_requestReimbursement.Category == 0)
        {
            _snackbar.AddWarning("Please input category!");
            StateHasChanged();
            return;
        }

        else if (_requestReimbursement.RequestDate == null)
        {
            _snackbar.AddWarning("Please input date!");
            StateHasChanged();
            return;
        }

        else if (_requestReimbursement.CurrencyType == 0)
        {
            _snackbar.AddWarning("Please input Currency!");
            StateHasChanged();
            return;
        }

        else if (_requestReimbursement.Amount == 0)
        {
            _snackbar.AddWarning("Please input amount!");
            StateHasChanged();
            return;
        }

        else if (_requestReimbursement.Notes == null)
        {
            _snackbar.AddWarning("Please input notes!");
            StateHasChanged();
            return;
        }

        else if (_requestReimbursement.Id == null && FileUploads.File == null)
        {
            _snackbar.AddWarning("Please browse document payment proof!");
            StateHasChanged();
            return;
        }

        _isLoadingDialog = true;
        StateHasChanged(); // update tampilan agar spinner muncul

        if (FileUploads.File != null)
        {
            await UploadReimbursement(_employeeId);
        }

        //await UploadReimbursement(_employeeId);

        var reimbursement = _requestReimbursement;
        reimbursement.ApplicantId = _applicantId;

        if (reimbursement.Id is null)
        {
            //create reimbursement
            if (_documentId is null)
            {
                _snackbar.AddError("Failed to upload document");
                _isLoadingDialog = false;
                StateHasChanged();
                return;
            }

            reimbursement.DocumentId = (Guid)_documentId;
            reimbursement.PaymentStatus = 1;
            var response = await _paymentService.CreateApplicantReimbursementAsync(reimbursement);

            if (response.Error is not null)
            {
                _snackbar.AddError(response.Error.Detail);
                VisiblePopupReimbursement = false;
            }
            else
            {
                _snackbar.AddSuccess($"Success Create Reimbursement");
                VisiblePopupReimbursement = false;
                StateHasChanged();
            }
        }
        else
        {
            // edit reimburs
            if (FileUploads.File is not null && _documentId is not null)
            {
                reimbursement.DocumentId = (Guid)_documentId;
            }

            if (reimbursement.PaymentStatus == (int)PaymentStatus.NeedRevision)
            {
                reimbursement.PaymentStatus = (int)PaymentStatus.WaitingApproval;
            }

            var response = await _paymentService.UpdateReimbursementAsync(reimbursement);

            if (response.Error is not null)
            {
                _snackbar.AddError(response.Error.Detail);
                VisiblePopupReimbursement = false;
            }
            else
            {
                _snackbar.AddSuccess($"Success Update Reimbursement");
                VisiblePopupReimbursement = false;
                StateHasChanged();
            }
        }

        //if (_documentId is not null)
        //{
        //    if (reimbursement.Id is null)
        //    {
        //        reimbursement.DocumentId = (Guid)_documentId;
        //        reimbursement.PaymentStatus = 1;
        //        var response = await _paymentService.CreateApplicantReimbursementAsync(reimbursement);

        //        if (response.Error is not null)
        //        {
        //            _snackbar.AddError(response.Error.Detail);
        //            VisiblePopupReimbursement = false;
        //        }
        //        else
        //        {
        //            _snackbar.AddSuccess($"Success Create Reimbursement");
        //            VisiblePopupReimbursement = false;
        //            StateHasChanged();
        //        }
        //    }
        //    else
        //    {
        //        if (FileUploads.File is not null)
        //        {
        //            reimbursement.DocumentId = (Guid)_documentId;
        //        }

        //        if (reimbursement.PaymentStatus == (int)PaymentStatus.NeedRevision)
        //        {
        //            reimbursement.PaymentStatus = (int)PaymentStatus.WaitingApproval;
        //        }

        //        var response = await _paymentService.UpdateReimbursementAsync(reimbursement);

        //        if (response.Error is not null)
        //        {
        //            _snackbar.AddError(response.Error.Detail);
        //            VisiblePopupReimbursement = false;
        //        }
        //        else
        //        {
        //            _snackbar.AddSuccess($"Success Update Reimbursement");
        //            VisiblePopupReimbursement = false;
        //            StateHasChanged();
        //        }

        //    }
        //}

        await SetDataOnLoadAsync();
        _isLoadingDialog = false;
        StateHasChanged(); // refresh tampilan setelah selesai
    }

    private async Task ClickedReimbursementApprove()
    {
        //if (_requestReimbursement.Category == 14)
        //{
        //    var request = new ReimburdementStatusRequest()
        //    {
        //        Id = (Guid)_requestReimbursement.Id,
        //        Status = 15,
        //    };
        //    var response = await _paymentService.UpdateReimbursementStatusAsync(request);
        //    if (response.Error is not null)
        //    {
        //        _snackbar.AddError(response.Error.Detail);
        //        VisiblePopupReimbursement = false;
        //        StateHasChanged();
        //    }
        //    else
        //    {
        //        _snackbar.AddSuccess($"Success Update Status Reimbursement");
        //        VisiblePopupReimbursement = false;
        //        await SetDataOnLoadAsync();
        //        StateHasChanged();
        //    }
        //}
        //else
        //{
        var request = new ReimburdementStatusRequest()
        {
            Id = (Guid)_requestReimbursement.Id,
            Status = _requestReimbursement.PaymentStatus == 1 ? 2 : _requestReimbursement.PaymentStatus == 2 ? 11 : 5,
        };
        var response = await _paymentService.UpdateReimbursementStatusAsync(request);
        if (response.Error is not null)
        {
            _snackbar.AddError(response.Error.Detail);
            VisiblePopupReimbursement = false;
            StateHasChanged();
        }
        else
        {
            _snackbar.AddSuccess($"Success Update Status Reimbursement");
            VisiblePopupReimbursement = false;
            await SetDataOnLoadAsync();
            StateHasChanged();
        }
        //}
    }
    private async Task ClickedReimbursementReject()
    {
        var request = new ReimburdementStatusRequest()
        {
            Id = (Guid)_requestReimbursement.Id,
            Status = 9,
            Notes = _requestReimbursement.Notes,
        };
        var response = await _paymentService.UpdateReimbursementStatusAsync(request);
        if (response.Error is not null)
        {
            _snackbar.AddError(response.Error.Detail);
            VisiblePopup = false;
            StateHasChanged();
        }
        else
        {
            _snackbar.AddSuccess($"Success Update Status Reimbursement");
            VisiblePopup = false;
            await SetDataOnLoadAsync();
            StateHasChanged();
        }
    }
    protected void ValueChangeUploaderReimbursement(UploadChangeEventArgs args)
    {
        FileUploads.FileName = args.Files.First().File.Name;
        FileUploads.FileSize = args.Files.First().File.Size;
        FileUploads.FileContentType = args.Files.First().File.ContentType;
        FileUploads.File = args.Files.First().File;
    }
    protected void OnRemoveUploaderReimbursement(RemovingEventArgs args)
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
                DocumentType = DocumentType.Reimbursement,
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
    public async void OnCommandClickedDocument(CommandClickEventArgs<GetDocumentsDocument> args)
    {
        if (args.CommandColumn.Type == CommandButtonType.None)
        {
            if (args.CommandColumn.Title == "Download")
            {
                await OnClickDownload(args.RowData.Id, args.RowData.DocumentType);
            }
            else if (args.CommandColumn.Title == "Delete")
            {
                _documentId = args.RowData.Id;
                VisiblePopupDelete = true;
                DeleteType = "DeleteDocument";
            }

        }
    }
    private async Task OnClickDownload(Guid documentId, DocumentType status)
    {
        var request = new GetDocumentFileRequest
        {
            DocumentId = documentId,
            DocumentType = status,
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
    private void DialogDeleteOpen()
    {
        VisiblePopupDelete = true;
    }

    private void DialogDeleteClose()
    {
        VisiblePopupDelete = false;
    }

    private void ClickedNoDelete()
    {
        VisiblePopupDelete = false;
    }

    private async void ClickedYesDelete()
    {
        if (DeleteType == "DeleteDocument")
        {
            var response = await _paymentService.DeleteDocumentReimbursementAsync((Guid)_documentId);
            if (response.Error is null)
            {
                _snackbar.AddSuccess("Success delete data");
                VisiblePopupDelete = false;
                VisiblePopupReimbursement = false;
                _getListDocuments = new();
                await SetDataOnLoadAsync();
                StateHasChanged();
            }
        }
        else if (DeleteType == "DeleteReimbursement")
        {
            //var response = await _selectionTOEFLService.DeleteSelectionToeflAsync(_requestTOEFL!.Id);
            //if (response.Error is null)
            //{
            //    _snackbar.AddSuccess("Success delete data");
            //    VisiblePopupDelete = false;
            //    await SetDataOnLoadAsync();
            //    StateHasChanged();
            //}
        }
    }
    public async Task OnCommandClicked(CommandClickEventArgs<GetApplicantReimbursementResponse> args)
    {
        _getListDocuments = new();
        if (args.CommandColumn.Type == CommandButtonType.Edit)
        {
            //if (args.RowData.CategoryId == 14)
            //{
            //    _navigationManager.NavigateTo(Psmp.Bsui.Features.Applicants.Payments.Constants.RouteFor.PaymentDetailAccepted(args.RowData.Id, args.RowData.ApplicantId), true);
            //}
            //else
            //{
            IsEnabled = true;
            VisiblePopupReimbursement = true;
            _requestReimbursement.CurrencyType = (int)args.RowData.CurrencyType;
            _requestReimbursement.Amount = args.RowData.Amount;
            _requestReimbursement.Category = (int)args.RowData.CategoryId;
            _requestReimbursement.Notes = args.RowData.Notes;
            _requestReimbursement.RequestDate = args.RowData.RequestDate;
            _requestReimbursement.Id = args.RowData.Id;
            _requestReimbursement.IBANCode = args.RowData.IBANCode;
            _requestReimbursement.DueDate = args.RowData.DueDate;
            _requestReimbursement.UniversityBank = args.RowData.UniversityBank;
            _requestReimbursement.UniversityBankAccount = args.RowData.UniversityBankAccount;
            _requestReimbursement.DocumentId = args.RowData.Document.Id;
            _requestReimbursement.PaymentStatus = (int)args.RowData.PaymentStatus;
            TitlePopupReimbursement = "Edit Reimbursement";
            DeleteType = "DeleteReimbursement";
            _stepperData = BuildStepperData(args.RowData.PaymentStatus, args.RowData.Name, args.RowData.ModifiedBy, PaymentCategory.Reimbursement, args.RowData.RequestDate, args.RowData.Modified);

            if (args.RowData.Document is not null)
            {
                _documentId = args.RowData.Document.Id;
                IsAuploadedDocument = true;
                _getListDocuments.Add(new GetDocumentsDocument
                {
                    Id = args.RowData.Document.Id,
                    FileName = args.RowData.Document.FileName,
                    DocumentType = DocumentType.Reimbursement,
                });
            }
            else
            {
                IsAuploadedDocument = false;
            }
            //}
        }
        else if (args.CommandColumn.Type == CommandButtonType.None)
        {
            IsEnabled = false;
            VisiblePopupReimbursement = true;
            _requestReimbursement.CurrencyType = (int)args.RowData.CurrencyType;
            _requestReimbursement.Amount = args.RowData.Amount;
            _requestReimbursement.Category = (int)args.RowData.CategoryId;
            _requestReimbursement.Notes = args.RowData.Notes;
            _requestReimbursement.RequestDate = args.RowData.RequestDate;
            _requestReimbursement.Id = args.RowData.Id;
            _requestReimbursement.IBANCode = args.RowData.IBANCode;
            _requestReimbursement.DueDate = args.RowData.DueDate;
            _requestReimbursement.UniversityBank = args.RowData.UniversityBank;
            _requestReimbursement.UniversityBankAccount = args.RowData.UniversityBankAccount;
            _requestReimbursement.PaymentStatus = (int)args.RowData.PaymentStatus;
            TitlePopupReimbursement = "View Reimbursement";
            _stepperData = BuildStepperData(args.RowData.PaymentStatus, args.RowData.Name, args.RowData.ModifiedBy, PaymentCategory.Reimbursement, args.RowData.RequestDate, args.RowData.Modified);

            if (args.RowData.Document is not null)
            {
                _documentId = args.RowData.Document.Id;
                IsAuploadedDocument = true;
                _getListDocuments.Add(new GetDocumentsDocument
                {
                    Id = args.RowData.Document.Id,
                    FileName = args.RowData.Document.FileName,
                    DocumentType = DocumentType.Reimbursement,
                });
                if (args.RowData.DocumentApprovedId is not null)
                {
                    _getListDocuments.Add(new GetDocumentsDocument
                    {
                        Id = (Guid)args.RowData.DocumentApprovedId,
                        FileName = args.RowData.DocumentApproved!.FileName,
                        DocumentType = DocumentType.FinalTuitionFee,
                    });
                }
            }
            else
            {
                IsAuploadedDocument = false;
            }

            var request = new GetPaymentCommentRequest()
            {
                PaymentId = args.RowData.Id
            };
            var response = await _paymentService.GetPaymentCommentAsync(request);
            if (response.Error is null)
            {

                _listPaymentComment = response.Result!.Items.ToList();
            }
        }
    }

    private List<GetStepperResponse> BuildStepperData(PaymentStatus currentStatus, string employeeName, string modifiedBy, PaymentCategory category, DateOnly? requestDate = null, DateTimeOffset? modifiedDate = null)
    {
        var stepperList = new List<GetStepperResponse>();

        if (category == PaymentCategory.Reimbursement)
        {
            // Step 1
            stepperList.Add(new GetStepperResponse
            {
                PaymentStatus = PaymentStatus.NewRequest,
                PaymentCategory = PaymentCategory.Reimbursement,
                Stage = Stage.Awardee,
                Text = "Requested",
                Name = employeeName,
                Icon = "e-icons e-check",
                Date = requestDate ?? default
            });

            var isStep2Active = currentStatus is >= PaymentStatus.WaitingApproval and < PaymentStatus.Posted;
            var isStep2Completed = currentStatus >= PaymentStatus.Approved;
            var step2Icon = "";
            var step2Text = "";

            switch (currentStatus)
            {
                case PaymentStatus.Rejected:
                    step2Icon = "e-icons e-close";
                    step2Text = "Rejected";
                    break;
                case PaymentStatus.Approved:
                case >= PaymentStatus.Posted:
                    step2Icon = "e-icons e-check";
                    step2Text = "Approved";
                    break;
                case PaymentStatus.WaitingApproval:
                    step2Icon = "";
                    step2Text = "Waiting Approval";
                    break;
                case PaymentStatus.NeedRevision:
                    step2Icon = "e-icons e-reset";
                    step2Text = "Need Revision";
                    break;
                case PaymentStatus.Pending:
                    step2Icon = "e-icons e-clock icon-blue";
                    step2Text = "Pending";
                    break;
                case PaymentStatus.RequestPreparation:
                    step2Icon = "e-icons e-check";
                    step2Text = "Approved";
                    break;
                case PaymentStatus.RequestToBmc:
                    step2Icon = "e-icons e-send";
                    step2Text = "Requested To BMC";
                    break;
                default:
                    step2Icon = "";
                    step2Text = "Waiting Approval";
                    break;
            }

            // Step 2
            stepperList.Add(new GetStepperResponse
            {
                PaymentStatus = PaymentStatus.WaitingApproval,
                PaymentCategory = PaymentCategory.Reimbursement,
                Stage = Stage.Admin,
                Text = step2Text,
                Name = (isStep2Active || isStep2Completed || currentStatus == PaymentStatus.Rejected) ? modifiedBy : null,
                Icon = step2Icon,
                Date = (isStep2Active || isStep2Completed || currentStatus == PaymentStatus.Rejected) && modifiedDate.HasValue
                    ? DateOnly.FromDateTime(modifiedDate.Value.DateTime)
                    : default
            });

            // Step 3
            var isPosted = currentStatus == PaymentStatus.Posted;
            var isCompleted = currentStatus == PaymentStatus.Completed;
            var isPreparation = currentStatus == PaymentStatus.RequestPreparation;

            stepperList.Add(new GetStepperResponse
            {
                PaymentStatus = PaymentStatus.Posted,
                PaymentCategory = PaymentCategory.Reimbursement,
                Stage = Stage.SAPPayroll,
                Text = isPreparation ? "Preparation" : "Completed",
                Name = null,
                Icon = (isPosted || isCompleted) ? "e-icons e-check" : "",
                Date = (isPosted || isCompleted) && modifiedDate.HasValue
                    ? DateOnly.FromDateTime(modifiedDate.Value.DateTime)
                    : default
            });
        }

        return stepperList;
    }

    // field untuk cache
    private List<GetApplicantReimbursementResponse> _filteredReimbursements = new();

    // panggil ini saat data awal di-load atau saat tab berubah
    private void UpdateFilteredReimbursements()
    {
        var result = _selectedTab switch
        {
            "Request" => _applicantReimbursement
                .Where(x => x.PaymentStatus is PaymentStatus.WaitingApproval or PaymentStatus.Rejected or PaymentStatus.Pending or PaymentStatus.NeedRevision),

            "Progress" => _applicantReimbursement
                .Where(x => x.PaymentStatus is PaymentStatus.RequestPreparation or PaymentStatus.Approved),

            "Completed" => _applicantReimbursement
                .Where(x => x.PaymentStatus is PaymentStatus.Completed),

            _ => _applicantReimbursement
        };

        _filteredReimbursements = result.ToList();
    }

    // property cuma mengembalikan cache (no LINQ evaluation here)
    private IEnumerable<GetApplicantReimbursementResponse> FilteredReimbursements => _filteredReimbursements;

    private void OnTabChanged(int index)
    {
        if (index < 0)
        {
            index = 0;
        }

        var newTab = index switch
        {
            0 => "Request",
            1 => "Progress",
            2 => "Completed",
            _ => "Request"
        };

        _selectedTab = newTab;

        UpdateFilteredReimbursements();
        StateHasChanged();
    }
}
