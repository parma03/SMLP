using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using MudBlazor;
using Smlp.Base.Selections.Enums;
using Smlp.Bsui.Common.Constants;
using Smlp.Bsui.Common.Extensions;
using Smlp.Shared.Common.Constants;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Documents.Queries.GetDocumentFile;
using Smlp.Shared.Payments.Queries;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Navigations;
using static Smlp.Bsui.Features.Payments.Index;

namespace Smlp.Bsui.Features.Payments.BankAccount;

public partial class Index
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;
    protected ClaimsPrincipal _user = default!;
    protected ErrorResponse? _error;
    protected bool _isLoading = false;
    protected bool _isLoadingData = false;
    protected bool _isLoadingDialog = false;

    private MudMessageBox _confirmMessageBox;
    private string _confirmTitle = string.Empty;
    private string _confirmMessage = string.Empty;

    public List<DropdownItemResponse> _dropdownCategories = new();

    [CascadingParameter]
    public required PaymentFilterState PaymentFilters { get; set; }
    //public List<DropdownItemResponse> _dropdownHps = new();
    //public List<DropdownItemStringResponse> _dropdownCompany = new();
    //public List<DropdownItemResponse> _dropdownDirectorate = new();
    //protected int _companyGroupId = 0;
    //protected string _companyCode = string.Empty;
    //protected int _directorateId = 0;
    protected int _category = 0;
    private bool VisiblePopup { get; set; }

    protected List<GetListApplicantBankAccountResponse> _applicantListApplicantBankAccount = new();
    private SfGrid<GetListApplicantBankAccountResponse> Grid { get; set; } = new();
    protected List<Guid> _selectedBankAccounts = new();
    protected bool _hasVerifiedSelected = false;
    private readonly GetListApplicantBankAccountResponse _viewResponse = new();
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
            _isLoading = true;
            await SetDataOnLoadAsync();
            _isLoading = false;
        }
    }
    private async Task SetDataOnLoadAsync()
    {
        _isLoadingData = true;
        await Task.Delay(2000);
        var request = new GetApplicantPaymentRequest()
        {
            CompanyGroup = PaymentFilters.CompanyGroupId,
            Company = PaymentFilters.CompanyCode,
            Category = _category,
            Directorate = PaymentFilters.DirectorateId
        };
        var response = await _paymentService.GetListApplicantBankAccountAsync(request);
        if (response.Error is null)
        {
            _applicantListApplicantBankAccount = response.Result?.Items.OrderByDescending(x => x.Name).ToList() ?? new();

        }
        else
        {
            _error = response.Error;
            _isLoadingData = false;
        }

        _isLoadingData = false;
    }

    //#region FIltering Dropdown
    //private async Task GetDropdownCompanyGroup()
    //{
    //    var data = await _companyGroupService.GetCompanyGroupsAsync();
    //    if (data.Error is null)
    //    {
    //        _dropdownHps = data.Result!.Items.Where(x => _companyCode == "1010" ? true : x.CompanyCode == _companyCode).Select(x => new DropdownItemResponse { Id = x.Id, Description = x.Name }).ToList();
    //    }
    //}
    //private async Task GetDropdownCompany()
    //{
    //    var data = await _companyService.GetCompanyByCompanyGroupIdAsync(_companyGroupId);
    //    if (data.Error is null)
    //    {
    //        _dropdownCompany = data.Result!.Items.Select(x => new DropdownItemStringResponse { Id = x.Code, Description = x.Name }).OrderBy(x => x.Id).ToList();
    //    }
    //}
    //private async Task GetDropdownDirectorate()
    //{
    //    var data = await _directorateService.GetDirectotaeByCompanyCodeAsync(_companyCode);
    //    if (data.Error is null)
    //    {
    //        _dropdownDirectorate = data.Result!.Items.Select(x => new DropdownItemResponse { Id = x.Id, Description = x.Name }).OrderBy(x => x.Id).ToList();
    //    }
    //}
    //private async Task GetDropdownCategories()
    //{
    //    _dropdownCategories = EnumExtentions.ToDropdownItem<ScholarshipLocation>();
    //}
    //public async Task OnChangeCompanyGroupAsync()
    //{
    //    await GetDropdownCompany();
    //    await SetDataOnLoadAsync();
    //    StateHasChanged();
    //}
    //public async Task OnChangeCompanyAsync()
    //{
    //    await GetDropdownDirectorate();
    //    await SetDataOnLoadAsync();
    //    StateHasChanged();
    //}
    //public async Task OnChangeDirectorateAsync()
    //{
    //    await SetDataOnLoadAsync();
    //    StateHasChanged();
    //}
    //public async Task OnChangeCategory()
    //{
    //    await SetDataOnLoadAsync();
    //    StateHasChanged();
    //}
    //#endregion

    #region Grid Events
    public async Task OnCommandClicked(CommandClickEventArgs<GetListApplicantBankAccountResponse> args)
    {
        if (args == null || args.CommandColumn == null)
        {
            return;
        }

        var title = args.CommandColumn.Title ?? string.Empty;

        if (string.Equals(title, "Download", StringComparison.OrdinalIgnoreCase))
        {
            if (args.RowData?.DocumentId != null)
            {
                await DownloadDocument((Guid)args.RowData.DocumentId);
            }
            else
            {
                _snackbar.AddError("Error : Document not found!!");
            }
        }
        else if (string.Equals(title, "View", StringComparison.OrdinalIgnoreCase))
        {
            VisiblePopup = true;
            if (args is not null)
            {
                _viewResponse.Name = args.RowData.Name;
                _viewResponse.EmployeeNumber = args.RowData.EmployeeNumber;
                _viewResponse.Email = args.RowData.Email;
                _viewResponse.BankName = args.RowData.BankName;
                _viewResponse.AccountHolder = args.RowData.AccountHolder;
                _viewResponse.AccountNumber = args.RowData.AccountNumber;
                _viewResponse.Currency = args.RowData.Currency;
                _viewResponse.BankCountry = args.RowData.BankCountry;
                _viewResponse.BankCity = args.RowData.BankCity;
                _viewResponse.BankBranch = args.RowData.BankBranch;
                _viewResponse.IsVerified = args.RowData.IsVerified;
            }
            else
            {
                _snackbar.AddError("Error : Data not found!!");
            }

            StateHasChanged();
        }
        else if (args.CommandColumn.Title == "Verivied")
        {
            await ToggleSingleAccount(args.RowData);
        }
    }
    private async void OnRowSelected(RowSelectEventArgs<GetListApplicantBankAccountResponse> args)
    {
        if (args.Data?.Id != null && !_selectedBankAccounts.Contains(args.Data.Id))
        {
            _selectedBankAccounts.Add(args.Data.Id);
        }

        await SyncSelectedRecords();
        UpdateButtonState();
        StateHasChanged();
    }
    private async void OnRowDeselected(RowDeselectEventArgs<GetListApplicantBankAccountResponse> args)
    {
        if (args.Data?.Id != null)
        {
            _selectedBankAccounts.Remove(args.Data.Id);
        }

        await SyncSelectedRecords();
        UpdateButtonState();
        StateHasChanged();
    }
    private async Task SyncSelectedRecords()
    {
        var selectedRecords = await Grid.GetSelectedRecordsAsync();
        _selectedBankAccounts = selectedRecords
            .Where(x => x.Id != null && x.Id != Guid.Empty)
            .Select(x => x.Id)
            .Distinct()
            .ToList();
    }

    private void UpdateButtonState()
    {
        _hasVerifiedSelected = _applicantListApplicantBankAccount
            .Where(x => _selectedBankAccounts.Contains(x.Id))
            .Any(x => x.IsVerified);
    }
    #endregion

    #region Verification Toggle
    private async Task ToggleSingleAccount(GetListApplicantBankAccountResponse account)
    {
        if (account.Id == null || account.Id == Guid.Empty)
        {
            _snackbar.AddError("Invalid bank account ID");
            return;
        }

        var newStatus = !account.IsVerified;
        var statusText = newStatus ? "verify" : "unverify";

        _confirmTitle = $"Confirm {statusText} Bank Account";
        _confirmMessage = $"Are you sure you want to {statusText} bank account for <b>{account.Name}</b>?";

        var options = new DialogOptions { CloseButton = false };
        var result = await _confirmMessageBox.Show(options);

        if (result != true)
        {
            StateHasChanged();
            return;
        }

        var response = await _paymentService.VerifyBankAccountAsync(new List<Guid> { account.Id }, newStatus);

        if (response.Error is null)
        {
            _snackbar.AddSuccess($"Bank account {(newStatus ? "verified" : "unverified")} successfully");
            await SetDataOnLoadAsync();
            StateHasChanged();
        }
        else
        {
            var result2 = string.Join(", ", response.Error.Details);
            _snackbar.AddError($"Operation failed: {result2}");
        }
    }

    private async Task ToggleSelectedAccounts()
    {
        if (!_selectedBankAccounts.Any())
        {
            _snackbar.AddWarning("Please select at least one bank account");
            return;
        }

        var newStatus = !_hasVerifiedSelected;
        var statusText = newStatus ? "verify" : "unverify";

        _confirmTitle = $"Confirm {statusText} Bank Accounts";
        _confirmMessage = $"Are you sure you want to {statusText} <b>{_selectedBankAccounts.Count}</b> selected bank account(s)?";

        var options = new DialogOptions { CloseButton = false };
        var result = await _confirmMessageBox.Show(options);

        if (result != true)
        {
            StateHasChanged();
            return;
        }

        var response = await _paymentService.VerifyBankAccountAsync(
            _selectedBankAccounts,
            newStatus);

        if (response.Error is null)
        {
            _snackbar.AddSuccess($"Successfully {(newStatus ? "verified" : "unverified")} {_selectedBankAccounts.Count} bank account(s)");
            _selectedBankAccounts.Clear();
            await SetDataOnLoadAsync();
            await Grid.ClearSelectionAsync();
            _hasVerifiedSelected = false;
        }
        else
        {
            var result2 = string.Join(", ", response.Error.Details);
            _snackbar.AddError($"Operation failed: {result2}");
        }
    }
    #endregion

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
        if (args.Item.Id.Contains("_excelexport"))
        {
            var exportProperties = new ExcelExportProperties
            {
                IncludeTemplateColumn = true
            };
            await Grid.ExportToExcelAsync(exportProperties);
        }
    }
    public void ExcelQueryCellInfoHandler(ExcelQueryCellInfoEventArgs<GetListApplicantBankAccountResponse> args)
    {
        var a = args.Column.Field;
    }

    private void DialogOpen()
    {
        VisiblePopup = true;
    }

    private void DialogClose()
    {
        VisiblePopup = false;
    }
    private void ClickedNo()
    {
        DialogClose();
    }

    private async Task ClickedYes()
    {
        _isLoadingDialog = true;
        StateHasChanged();

        await SetDataOnLoadAsync();

        _isLoadingDialog = false;
        StateHasChanged();

        DialogClose();
    }
}
