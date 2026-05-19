using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Psmp.Base.Common;
using Psmp.Base.Payments.Enums;
using Psmp.Base.Scholarships.Enums;
using Psmp.Base.Selections.Enums;
using Psmp.Bsui.Common.Constants;
using Psmp.Bsui.Common.Extensions;
using Psmp.Bsui.Common.Helper;
using Psmp.Shared.Common.Constants;
using Psmp.Shared.Common.Extensions;
using Psmp.Shared.Common.Responses;
using Psmp.Shared.Documents.Queries.GetDocumentFile;
using Psmp.Shared.Documents.Queries.GetDocuments;
using Psmp.Shared.Payments.Commands;
using Psmp.Shared.Payments.Queries;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Inputs;
using Syncfusion.Blazor.Navigations;
using static Psmp.Bsui.Features.Payments.Index;

namespace Psmp.Bsui.Features.Payments.Reimbursements;

public partial class Index
{
    [Parameter]
    public Guid? ApplicantId { get; set; }
    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;
    protected ClaimsPrincipal _user = default!;
    protected List<GetApplicantPaymentReimbursementResponse> _applicantReimbursement = new();
    private SfGrid<GetApplicantPaymentReimbursementResponse> Grid { get; set; } = new();
    private GetApplicantPaymentReimbursementResponse _dataReimbursement = new();
    protected List<GetDocumentsDocument> _getListDocuments = new();
    protected List<GetStepperResponse> _stepperData = new();
    private List<GetPaymentCommentResponse>? _listPaymentComment = new();

    protected ErrorResponse? _error;
    protected bool _isLoading = false;
    protected bool _isLoadingDialog = false;
    protected bool _isLoadingData = false;
    protected Guid? _documentId;
    private bool VisiblePopup { get; set; }
    private bool VisiblePopupValidation { get; set; }

    [CascadingParameter]
    public required PaymentFilterState PaymentFilters { get; set; }
    public List<DropdownItemResponse> _dropdownCategories = new();
    public List<DropdownItemResponse> _dropdownScholarshipCode = new();
    public List<DropdownItemResponse> _dropdownBatch = new();
    private List<DropdownItemResponse> _dropdownItemCategorys = new();
    private List<DropdownItemResponse> _dropdownItemCurrencyType = new();

    private decimal _countcurrency1;
    private decimal _countcurrency2;
    private decimal _countcurrency3;
    private decimal _countcurrency4;
    private decimal _countcurrency5;
    private decimal _countcurrency6;
    private decimal _countcurrency7;
    private decimal _countcurrency8;
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
    private int _count11;
    private int _count12;
    private int _count13;
    private int _count14;
    protected long _maximumFileSizeInBytes = long.MaxValue;
    protected SfUploader? _uploadObj;
    protected int _category = 0;
    protected string _employeeNumber = string.Empty;
    protected string _employeeName = string.Empty;
    protected string _displayName = default!;
    protected string _usernameApplicant = string.Empty;
    protected bool IsApprovalDisabled { get; set; } = false;

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
    protected override async Task OnInitializedAsync()
    {
        _user = (await AuthenticationStateTask).User;

        PaymentFilters.OnFilterChanged += async (_, _) =>
        {
            await SetDataOnLoadAsync();
            StateHasChanged();
        };

        if (_user.Identity.IsAuthenticated)
        {
            _displayName = _user.GetDisplayName() ?? string.Empty;
            //_companyCode = _user.GetCompanyCode();
            if (ApplicantId != null)
            {
                var applicantRes = await _applicantService.GetApplicantByIdAsync(ApplicantId.Value);
                if (applicantRes.Error is null)
                {
                    var data = applicantRes.Result!.Items.SingleOrDefault();

                    _usernameApplicant = data!.Name;
                }
            }

            _isLoading = true;
            await GetDataStatusConfiguration();
            await SetDataOnLoadAsync();
            await GetDropdownCategories();
            //await GetDropdownCompanyGroup();
            //await GetDropdownCompany();
            //await GetDropdownDirectorate();
            await GetCategoryReimbursemen();
            _dropdownItemCurrencyType = EnumExtentions.ToDropdownItem<CurrencyType>();
            _isLoading = false;
        }
    }

    private async Task SetDataOnLoadAsync()
    {
        _isLoadingData = true;
        await Task.Delay(500);
        var request = new GetApplicantPaymentRequest()
        {
            CompanyGroup = PaymentFilters.CompanyGroupId,
            Company = PaymentFilters.CompanyCode,
            Category = _category,
            ApplicantId = ApplicantId,
            Directorate = PaymentFilters.DirectorateId
        };

        var response = await _paymentService.GetReimbursementAsync(request);
        if (response.Error is null)
        {
            _applicantReimbursement = response.Result?.Items.OrderByDescending(x => x.RequestDate).ToList();
            _countcurrency1 = (decimal)_applicantReimbursement.Where(x => x.PaymentStatus == PaymentStatus.WaitingApproval && x.CurrencyType == 2).Sum(x => x.Amount); //Waiting for Approval
            _countcurrency2 = (decimal)_applicantReimbursement.Where(x => x.PaymentStatus == PaymentStatus.WaitingApproval && x.CurrencyType == 1).Sum(x => x.Amount); //Waiting for Approval
            _countcurrency3 = (decimal)_applicantReimbursement.Where(x => x.PaymentStatus == PaymentStatus.Pending && x.CurrencyType == 2).Sum(x => x.Amount); //Pending
            _countcurrency4 = (decimal)_applicantReimbursement.Where(x => x.PaymentStatus == PaymentStatus.Pending && x.CurrencyType == 1).Sum(x => x.Amount); //Pending
            _countcurrency5 = (decimal)_applicantReimbursement.Where(x => x.PaymentStatus == PaymentStatus.Rejected && x.CurrencyType == 2).Sum(x => x.Amount); //Rejected
            _countcurrency6 = (decimal)_applicantReimbursement.Where(x => x.PaymentStatus == PaymentStatus.Rejected && x.CurrencyType == 1).Sum(x => x.Amount); //Rejected
            _countcurrency7 = (decimal)_applicantReimbursement.Where(x => x.PaymentStatus == PaymentStatus.RequestPreparation && x.CurrencyType == 2).Sum(x => x.Amount); //Preparation
            _countcurrency8 = (decimal)_applicantReimbursement.Where(x => x.PaymentStatus == PaymentStatus.RequestPreparation && x.CurrencyType == 1).Sum(x => x.Amount); //Preparation
            //_countcurrency9 = (decimal)_applicantReimbursement.Where(x => x.PaymentStatus == PaymentStatus.Completed && x.CurrencyType == 2).Sum(x => x.Amount); 
            //_countcurrency10 = (decimal)_applicantReimbursement.Where(x => x.PaymentStatus == PaymentStatus.Completed && x.CurrencyType == 1).Sum(x => x.Amount);
            _countcurrency11 = (decimal)_applicantReimbursement.Where(x => x.PaymentStatus == PaymentStatus.Completed && x.CurrencyType == 2).Sum(x => x.Amount); //Posted
            _countcurrency12 = (decimal)_applicantReimbursement.Where(x => x.PaymentStatus == PaymentStatus.Completed && x.CurrencyType == 1).Sum(x => x.Amount); //Posted
            _countcurrency13 = (decimal)_applicantReimbursement.Where(x => x.PaymentStatus == PaymentStatus.NeedRevision && x.CurrencyType == 2).Sum(x => x.Amount); //Need Revision
            _countcurrency14 = (decimal)_applicantReimbursement.Where(x => x.PaymentStatus == PaymentStatus.NeedRevision && x.CurrencyType == 1).Sum(x => x.Amount); //Need Revision

            _count1 = _applicantReimbursement.Where(x => x.PaymentStatus == PaymentStatus.WaitingApproval && x.CurrencyType == 2).Count();
            _count2 = _applicantReimbursement.Where(x => x.PaymentStatus == PaymentStatus.WaitingApproval && x.CurrencyType == 1).Count();
            _count3 = _applicantReimbursement.Where(x => x.PaymentStatus == PaymentStatus.Pending && x.CurrencyType == 2).Count();
            _count4 = _applicantReimbursement.Where(x => x.PaymentStatus == PaymentStatus.Pending && x.CurrencyType == 1).Count();
            _count5 = _applicantReimbursement.Where(x => x.PaymentStatus == PaymentStatus.Rejected && x.CurrencyType == 2).Count();
            _count6 = _applicantReimbursement.Where(x => x.PaymentStatus == PaymentStatus.Rejected && x.CurrencyType == 1).Count();
            _count7 = _applicantReimbursement.Where(x => x.PaymentStatus == PaymentStatus.RequestPreparation && x.CurrencyType == 2).Count();
            _count8 = _applicantReimbursement.Where(x => x.PaymentStatus == PaymentStatus.RequestPreparation && x.CurrencyType == 1).Count();
            //_count9 = _applicantReimbursement.Sum(x => x.TotalComplited);
            //_count10 = _applicantReimbursement.Sum(x => x.TotalIdrComplited);
            _count11 = _applicantReimbursement.Where(x => x.PaymentStatus == PaymentStatus.Completed && x.CurrencyType == 2).Count();
            _count12 = _applicantReimbursement.Where(x => x.PaymentStatus == PaymentStatus.Completed && x.CurrencyType == 1).Count();
            _count13 = _applicantReimbursement.Where(x => x.PaymentStatus == PaymentStatus.NeedRevision && x.CurrencyType == 2).Count();
            _count14 = _applicantReimbursement.Where(x => x.PaymentStatus == PaymentStatus.NeedRevision && x.CurrencyType == 1).Count();
        }
        else
        {
            _error = response.Error;
            _isLoadingData = false;
        }

        OnTabChanged(ActiveTabIndex);

        _isLoadingData = false;
    }

    private async Task GetDataStatusConfiguration()
    {
        var response = await _setDataStatusService.GetSetDataStatusAsync();
        if (response.Error is null && response.Result is not null)
        {
            var config = response.Result;
            IsApprovalDisabled = SetDataStatusHelper.IsApprovalDisabled(config.DisabledFrom, config.DisabledTo);
        }
    }

    private async Task GetCategoryReimbursemen()
    {
        var response = await _studyAssignmentService.GetCategoryReimbursemenAsync();
        if (response.Error is null)
        {
            _dropdownItemCategorys = response.Result!.Items.Select(x => new DropdownItemResponse { Id = x.Id, Description = x.Item }).ToList();
        }
    }

    public async Task OnCommandClicked(CommandClickEventArgs<GetApplicantPaymentReimbursementResponse> args)
    {
        // safety checks
        if (args == null || args.CommandColumn == null || args.RowData == null)
        {
            return;
        }

        var title = args.CommandColumn.Title ?? string.Empty;

        if (string.Equals(title, "View", StringComparison.OrdinalIgnoreCase))
        {
            _getListDocuments.Clear();

            _employeeName = args.RowData.Name;
            _employeeNumber = args.RowData.EmployeeNumber;
            _dataReimbursement = new GetApplicantPaymentReimbursementResponse
            {
                Id = args.RowData.Id,
                IdReimbursement = args.RowData.IdReimbursement,
                CurrencyType = args.RowData.CurrencyType,
                Amount = args.RowData.Amount,
                CategoryId = args.RowData.CategoryId,
                RequestDate = args.RowData.RequestDate,
                RequestId = args.RowData.RequestId,
                Notes = args.RowData.Notes,
                IBANCode = args.RowData.IBANCode,
                UniversityBank = args.RowData.UniversityBank,
                UniversityBankAccount = args.RowData.UniversityBankAccount,
                DueDate = args.RowData.DueDate,
                PaymentStatus = args.RowData.PaymentStatus,
                Document = args.RowData.Document
            };

            //_dataReimbursement.CurrencyType = (int)args.RowData.CurrencyType;
            //_dataReimbursement.Amount = args.RowData.Amount;
            //_dataReimbursement.CategoryId = (int)args.RowData.CategoryId;
            //_dataReimbursement.RequestDate = args.RowData.RequestDate!;
            //_dataReimbursement.RequestId = args.RowData.RequestId!;
            //_dataReimbursement.Notes = args.RowData.Notes!;
            //_dataReimbursement.Id = args.RowData.Id;
            //_dataReimbursement.IdReimbursement = args.RowData.IdReimbursement;
            _stepperData = BuildStepperData(args.RowData.PaymentStatus, args.RowData.Name, args.RowData.ModifiedBy, PaymentCategory.Reimbursement, args.RowData.RequestDate, args.RowData.Modified);

            if (args.RowData.Document is not null)
            {
                _documentId = args.RowData.Document.Id;
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

            var request = new GetPaymentCommentRequest()
            {
                PaymentId = args.RowData.IdReimbursement
            };
            var response = await _paymentService.GetPaymentCommentAsync(request);
            if (response.Error is null)
            {

                _listPaymentComment = response.Result!.Items.ToList();
            }

            // Buka dialog
            VisiblePopup = true;
            StateHasChanged();
        }
        else
        {
            _dataReimbursement = new();
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
            //var isPosted = currentStatus == PaymentStatus.Posted;
            //var isRejected = currentStatus == PaymentStatus.Rejected;

            //stepperList.Add(new GetStepperResponse
            //{
            //    PaymentStatus = PaymentStatus.Posted,
            //    PaymentCategory = PaymentCategory.Reimbursement,
            //    Stage = Stage.SAPPayroll,
            //    Text = isRejected ? "Cancelled" : "Posted",
            //    Name = (isPosted || isRejected) ? modifiedBy : null,
            //    Icon = isRejected ? "e-icons e-close" : (isPosted ? "e-icons e-check" : ""),
            //    Date = (isPosted || isRejected) && modifiedDate.HasValue
            //        ? DateOnly.FromDateTime(modifiedDate.Value.DateTime)
            //        : default
            //});
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

    // View action
    private void DialogOpen()
    {
        VisiblePopup = true;
    }

    private void DialogClose()
    {
        VisiblePopup = false;
    }

    private async Task ClickedYes((string Notes, PaymentStatus Status) statusInfo)
    {
        var testReq = new ReimburdementStatusRequest
        {
            Id = _dataReimbursement.IdReimbursement,
            Status = statusInfo.Status.GetIntValue(),
            Notes = statusInfo.Notes
        };

        _isLoadingDialog = true;
        StateHasChanged();

        var isError = false;

        if (isError)
        {

        }
        else
        {
            //await SetDataOnLoadAsync();
        }

        VisiblePopupValidation = false;
        DialogClose();
        await _paymentService.UpdateReimbursementStatusAsync(testReq);
        await Task.Delay(150);

        if (isError)
        {
            _snackbar.AddError("Failed to update reimbursement");
        }
        else
        {
            _snackbar.AddSuccess("Success Update Reimbursement");
        }

        _isLoadingDialog = false;
        await SetDataOnLoadAsync();
        StateHasChanged();
    }

    #region FIltering Dropdown
    //private async Task GetDropdownCompanyGroup()
    //{
    //    var data = await _companyGroupService.GetCompanyGroupsAsync();
    //    if (data.Error is null)
    //    {
    //        //_dropdownHps = data.Result!.Items.Where(x => _companyCode == "1010" ? true : x.CompanyCode == _companyCode).Select(x => new DropdownItemResponse { Id = x.Id, Description = x.Name }).ToList();
    //        _dropdownHps = data.Result!.Items.Select(x => new DropdownItemResponse { Id = x.Id, Description = x.Name }).ToList();

    //    }
    //}

    //private async Task GetDropdownCompany()
    //{
    //    if (_companyGroupId == null)
    //    {
    //        _dropdownCompany = new List<DropdownItemStringResponse>();
    //        _companyCode = string.Empty;
    //        return;
    //    }

    //    var data = await _companyService.GetCompanyByCompanyGroupIdAsync(_companyGroupId.Value);
    //    if (data.Error is null)
    //    {
    //        _dropdownCompany = data.Result!.Items.Select(x => new DropdownItemStringResponse { Id = x.Code, Description = x.Name }).OrderBy(x => x.Id).ToList();
    //    }
    //}
    //private async Task GetDropdownDirectorate()
    //{
    //    if (_companyCode == string.Empty || _companyCode == null)
    //    {
    //        _dropdownDirectorate = new List<DropdownItemResponse>();
    //        _directorateId = null;
    //        return;
    //    }

    //    var data = await _directorateService.GetDirectotaeByCompanyCodeAsync(_companyCode);
    //    if (data.Error is null)
    //    {
    //        _dropdownDirectorate = data.Result!.Items.Select(x => new DropdownItemResponse { Id = x.Id, Description = x.Name }).OrderBy(x => x.Id).ToList();
    //    }
    //}
    private async Task GetDropdownCategories()
    {
        _dropdownCategories = EnumExtentions.ToDropdownItem<ScholarshipLocation>();
    }

    //public async Task OnChangeCompanyGroupAsync(int? value)
    //{
    //    _companyGroupId = value;
    //    await GetDropdownCompany();
    //    await SetDataOnLoadAsync();

    //    //clear company & directory filter
    //    _companyCode = string.Empty;
    //    _directorateId = null;
    //    StateHasChanged();
    //}
    //public async Task OnChangeCompanyAsync(string value)
    //{
    //    _companyCode = value;
    //    await GetDropdownDirectorate();
    //    await SetDataOnLoadAsync();
    //    //clear directory filter
    //    _directorateId = null;
    //    StateHasChanged();
    //}

    //public async Task OnChangeDirectorateAsync(int? value)
    //{
    //    _directorateId = value;
    //    await SetDataOnLoadAsync();
    //    StateHasChanged();
    //}
    public async Task OnChangeCategory()
    {
        await SetDataOnLoadAsync();
        StateHasChanged();
    }
    #endregion
    private async Task ExportExcel()
    {
        var exportProperties = new ExcelExportProperties
        {
            IncludeTemplateColumn = true
        };
        await Grid.ExportToExcelAsync(exportProperties);
    }
    public async Task ToolbarClickHandler(ClickEventArgs args)
    {
        if (args.Item.Id.Contains("_excelexport")) //Id is combination of Grid's ID and itemname
        {
            var exportProperties = new ExcelExportProperties
            {
                IncludeTemplateColumn = true
            };
            await Grid.ExportToExcelAsync(exportProperties);
        }
    }
    public void ExcelQueryCellInfoHandler(ExcelQueryCellInfoEventArgs<GetApplicantPaymentReimbursementResponse> args)
    {
        var a = args.Column.Field;
        //if (args.Column.Field == "CurrencyType")
        //{
        //    args.Cell.Value = args.Data.CurrencyType.GetDescription();
        //}
        //else if (args.Column.Field == "Ammount")
        //{
        //    args.Cell.Value = args.Data.Ammount?.ToString("N0", new System.Globalization.CultureInfo("id-ID"));
        //}
        //else if (args.Column.Field == "PaymentStatus")
        //{
        //    args.Cell.Value = args.Data.PaymentStatus.GetDescription().ToString();
        //}
    }

    private List<GetApplicantPaymentReimbursementResponse> _filteredReimbursements = new();

    // panggil ini saat data awal di-load atau saat tab berubah
    private void UpdateFilteredReimbursements()
    {
        var result = _selectedTab switch
        {
            "Request" => _applicantReimbursement
                .Where(x => x.PaymentStatus is PaymentStatus.WaitingApproval or PaymentStatus.Pending or PaymentStatus.NeedRevision or PaymentStatus.Rejected),

            "Progress" => _applicantReimbursement
                .Where(x => x.PaymentStatus is PaymentStatus.RequestPreparation),

            "Completed" => _applicantReimbursement
                .Where(x => x.PaymentStatus == PaymentStatus.Completed),

            _ => _applicantReimbursement
        };

        _filteredReimbursements = result.ToList();

    }

    // property cuma mengembalikan cache (no LINQ evaluation here)
    private IEnumerable<GetApplicantPaymentReimbursementResponse> FilteredReimbursements => _filteredReimbursements;

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
    public async Task OnClickBack()
    {
        _navigationManager.NavigateTo(Psmp.Bsui.Features.Payments.Constants.RouteFor.Payment);
        //await Task.CompletedTask;
    }

}
