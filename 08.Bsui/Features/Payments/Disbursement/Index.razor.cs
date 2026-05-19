using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Smlp.Base.Common;
using Smlp.Base.Payments.Enums;

//using Smlp.Bsui.Features.Payments.Constants;
using Smlp.Base.Selections.Enums;
using Smlp.Bsui.Common.Constants;
using Smlp.Bsui.Common.Extensions;
using Smlp.Shared.Common.Constants;
using Smlp.Shared.Common.Extensions;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Documents.Commands.CreateDocument;
using Smlp.Shared.Documents.Queries.GetDocumentFile;
using Smlp.Shared.Documents.Queries.GetDocuments;
using Smlp.Shared.Masters.Queries;
using Smlp.Shared.Payments.Commands;
using Smlp.Shared.Payments.Queries;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Inputs;
using Syncfusion.Blazor.Navigations;
using static Smlp.Bsui.Features.Payments.Index;

namespace Smlp.Bsui.Features.Payments.Disbursement;

public partial class Index
{
    [Parameter]
    public Guid? ApplicantId { get; set; }
    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;
    protected ClaimsPrincipal _user = default!;
    protected List<GetApplicantDisbursementResponse> _applicantDisbursement = new();
    private List<GetApplicantDisbursementResponse> _filteredDisbursements = new();
    private SfGrid<GetApplicantDisbursementResponse> Grid { get; set; } = new();

    protected ErrorResponse? _error;
    protected bool _isLoadingDialog = false;
    protected bool _isLoading = false;
    protected bool _isLoadingData = false;

    protected string _employeeId = string.Empty;
    protected string _username = string.Empty;
    protected string _usernameApplicant = string.Empty;
    private bool VisiblePopup { get; set; }
    private bool IsViewMode { get; set; }
    private bool VisibleFormPopup { get; set; }
    private bool VisibleDeletePopup { get; set; }

    [Parameter]
    public string? Action { get; set; }

    [CascadingParameter]
    public PaymentFilterState PaymentFilters { get; set; } = default!;

    public List<DropdownItemResponse> _dropdownCategories = new();
    public List<DropdownItemResponse> _dropdownScholarshipCode = new();
    public List<DropdownItemResponse> _dropdownBatch = new();

    private decimal _countcurrency1;
    private decimal _countcurrency2;
    //private decimal _countcurrency3;
    //private decimal _countcurrency4;
    private decimal _countcurrency5;
    private decimal _countcurrency6;
    private int _count1;
    private int _count2;
    //private int _count3;
    //private int _count4;
    private int _count5;
    private int _count6;
    protected long _maximumFileSizeInBytes = long.MaxValue;
    protected SfUploader? _uploadObj;
    protected int _companyGroupId = 0;
    protected int _directorateId = 0;
    protected int _category = 0;
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
    private string _selectedTab = "Progress";
    private bool IsEnabled { get; set; }
    private string TitlePopup { get; set; } = "Add Disbursement";
    private List<DropdownItemResponse> _dropdownItemCurrencyType = new();
    protected int _itemId = default!;
    private List<DropdownItemResponse> _itemsDropDown = new();
    private List<GetItemStudyAssignmentResponse> _items = new();
    public DisbursementRequest _requestData = new();
    public string? _frequensi;
    public FileUploadResponse FileUploads { get; set; } = new();
    protected List<GetDocumentsDocument> _getListDocuments = new();
    protected Guid? _documentId;
    private Guid _deleteId;
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
            _dropdownItemCurrencyType = EnumExtentions.ToDropdownItem<CurrencyType>();
            await SetDataOnLoadAsync();
            await GetDropdownnItemStudyAssigments();
            _isLoading = false;
        }
    }

    private async Task SetDataOnLoadAsync()
    {
        _isLoadingData = true;
        await Task.Delay(2000);
        var request = new GetApplicantDisbursementRequest()
        {
            ApplicantId = ApplicantId,
            CompanyGroup = PaymentFilters.CompanyGroupId,
            CompanyCode = PaymentFilters.CompanyCode,
            Directorate = PaymentFilters.DirectorateId
        };

        var response = await _disbursementService.GetApplicantDisbursementAsync(request);
        if (response.Error is null)
        {
            _applicantDisbursement = response.Result!.Items.ToList();
            _countcurrency1 = (decimal)_applicantDisbursement.Where(x => x.PaymentStatus == PaymentStatus.RequestPreparation).Where(x => x.CurrencyType == CurrencyType.Usd).Sum(x => x.Amount);
            _countcurrency2 = (decimal)_applicantDisbursement.Where(x => x.PaymentStatus == PaymentStatus.RequestPreparation).Where(x => x.CurrencyType == CurrencyType.Idr).Sum(x => x.Amount);
            //_countcurrency3 = (decimal)_applicantDisbursement.Sum(x => x.TotalAmountRequestBmc);
            //_countcurrency4 = (decimal)_applicantDisbursement.Sum(x => x.TotalIdrAmountRequestBmc);
            _countcurrency5 = (decimal)_applicantDisbursement.Where(x => x.PaymentStatus == PaymentStatus.Completed).Where(x => x.CurrencyType == CurrencyType.Usd).Sum(x => x.Amount);
            _countcurrency6 = (decimal)_applicantDisbursement.Where(x => x.PaymentStatus == PaymentStatus.Completed).Where(x => x.CurrencyType == CurrencyType.Idr).Sum(x => x.Amount);

            _count1 = _applicantDisbursement.Where(x => x.PaymentStatus == PaymentStatus.RequestPreparation).Where(x => x.CurrencyType == CurrencyType.Usd).Count();
            _count2 = _applicantDisbursement.Where(x => x.PaymentStatus == PaymentStatus.RequestPreparation).Where(x => x.CurrencyType == CurrencyType.Idr).Count();
            //_count3 = _applicantDisbursement.Sum(x => x.TotalRequestBmc);
            //_count4 = _applicantDisbursement.Sum(x => x.TotalIdrRequestBmc);
            _count5 = _applicantDisbursement.Where(x => x.PaymentStatus == PaymentStatus.Completed).Where(x => x.CurrencyType == CurrencyType.Usd).Count();
            _count6 = _applicantDisbursement.Where(x => x.PaymentStatus == PaymentStatus.Completed).Where(x => x.CurrencyType == CurrencyType.Idr).Count();
        }
        else
        {
            _error = response.Error;
            //_isLoadingData = false;
        }

        OnTabChanged(ActiveTabIndex);
        //CalculatePreparationTotals();
        _isLoadingData = false;
    }

    private async Task GetDropdownnItemStudyAssigments()
    {
        var response = await _studyAssignmentService.GetStudyAssignmentsAsync();
        if (response.Error is null)
        {
            _itemsDropDown = response.Result!.Items.Select(x => new DropdownItemResponse { Id = x.Id, Description = x.Item }).ToList();
            _items = response.Result.Items.ToList();
        }
    }
    private void CreateDisbursementClick()
    {
        _employeeId = string.Empty;
        _username = string.Empty;
        _frequensi = string.Empty;
        IsEnabled = true;
        IsViewMode = false;
        _requestData = new();
        FileUploads = new();
        _getListDocuments = new();
        // mode editable karena user akan mengisi
        //TitlePopupReimbursement = "Add Disbursement";
        TitlePopup = "Add Disbursement";
        VisiblePopup = true;
        StateHasChanged();
    }
    private async Task OnDataClearedHandler(bool cleared)
    {
        if (cleared)
        {
            _employeeId = string.Empty;
            _username = string.Empty;
            _frequensi = string.Empty;
            _requestData = new();
            FileUploads = new();
            _getListDocuments = new();

            await InvokeAsync(StateHasChanged);
        }
    }

    public async void OnCommandClickedDocument(CommandClickEventArgs<GetDocumentsDocument> args)
    {
        if (args.CommandColumn.Type == CommandButtonType.None)
        {
            if (args.CommandColumn.Title == "Download")
            {
                await OnClickDownload(args.RowData.Id);
            }
            //else if (args.CommandColumn.Title == "Delete")
            //{
            //    _documentId = args.RowData.Id;
            //    VisiblePopupDelete = true;
            //    DeleteType = "DeleteDocument";
            //}

        }
    }
    protected void ValueChangeUploaderDisbursement(UploadChangeEventArgs args)
    {
        FileUploads.FileName = args.Files.First().File.Name;
        FileUploads.FileSize = args.Files.First().File.Size;
        FileUploads.FileContentType = args.Files.First().File.ContentType;
        FileUploads.File = args.Files.First().File;
    }
    protected void OnRemoveUploaderDibursement(RemovingEventArgs args)
    {
        FileUploads = new();
        args.PostRawFile = false;
    }
    private async Task OnClickDownload(Guid documentId)
    {
        var request = new GetDocumentFileRequest
        {
            DocumentId = documentId,
            DocumentType = DocumentType.Disbursement,
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

    private async Task OnCommandClicked(CommandClickEventArgs<GetApplicantDisbursementResponse> args)
    {
        if (args == null || args.CommandColumn == null || args.RowData == null)
        {
            return;
        }

        var row = args.RowData;
        var title = args.CommandColumn.Title ?? string.Empty;

        if (string.Equals(title, "Delete", StringComparison.OrdinalIgnoreCase) ||
        args.CommandColumn.Type == CommandButtonType.Delete)
        {
            VisiblePopup = false;
            IsViewMode = false;

            _deleteId = row.Id;

            _requestData = new DisbursementRequest
            {
                Id = row.Id,
                RequestId = row.RequestId,
                ApplicantId = row.ApplicantId,
                ItemId = row.ItemId
            };

            TitlePopup = "Delete Confirmation";
            VisibleDeletePopup = true;

            StateHasChanged();
            return;
        }

        if (!string.Equals(title, "View", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        VisibleDeletePopup = false;
        IsEnabled = false;
        IsViewMode = true;
        TitlePopup = "View Disbursement";

        //var row = args.RowData;

        _username = !string.IsNullOrWhiteSpace(row.DisplayName) ? row.DisplayName : row.EmployeeNumber;
        _employeeId = row.EmployeeNumber;
        _requestData = new DisbursementRequest
        {
            Id = row.Id,
            RequestId = row.RequestId,
            ApplicantId = row.ApplicantId,
            ItemId = row.ItemId,
            CurrencyType = (int)row.CurrencyType,
            Amount = row.Amount,
            Notes = row.Notes ?? string.Empty,
            StudyAssignmentFeeId = row.StudyAssignmentFeeId,
            DocumentId = row.DocumentId
        };

        if (!string.IsNullOrWhiteSpace(row.PaymentFrequency))
        {
            _frequensi = row.PaymentFrequency;
        }
        else
        {
            var item = _items?.SingleOrDefault(x => x.Id == row.ItemId);
            if (item != null)
            {
                var fee = item.StudyAssignmentFee?.FirstOrDefault();
                if (fee != null)
                {
                    if (fee.PeriodType == "Month")
                    {
                        _frequensi = $"Period - {fee.Perod} Month";
                    }
                    else
                    {
                        _frequensi = fee.PeriodType == "Times" ? $"Period - {fee.Perod} Times" : $"Period - {fee.Perod} {fee.PeriodType}";
                    }
                }
                else
                {
                    _frequensi = string.Empty;
                }
            }
            else
            {
                _frequensi = string.Empty;
            }
        }

        _getListDocuments = new List<GetDocumentsDocument>();
        if (row.Document != null)
        {
            var docIdProp = row.Document.GetType().GetProperty("Id");
            var docNameProp = row.Document.GetType().GetProperty("FileName") ?? row.Document.GetType().GetProperty("Name");
            var docIdVal = docIdProp?.GetValue(row.Document);
            var docNameVal = docNameProp?.GetValue(row.Document)?.ToString() ?? "attachment";

            _getListDocuments.Add(new GetDocumentsDocument
            {
                Id = docIdVal is Guid g ? g : Guid.NewGuid(),
                FileName = docNameVal
            });
        }

        VisiblePopup = true;
        StateHasChanged();
    }
    private void OnActionClick(GetApplicantDisbursementResponse row, string title)
    {
        if (row == null)
        {
            return;
        }

        // Logika untuk Delete
        if (string.Equals(title, "Delete", StringComparison.OrdinalIgnoreCase))
        {
            VisiblePopup = false;
            IsViewMode = false;

            _deleteId = row.Id;

            _requestData = new DisbursementRequest
            {
                Id = row.Id,
                RequestId = row.RequestId,
                ApplicantId = row.ApplicantId,
                ItemId = row.ItemId
            };

            TitlePopup = "Delete Confirmation";
            VisibleDeletePopup = true;

            StateHasChanged();
            return;
        }

        //View
        if (string.Equals(title, "View", StringComparison.OrdinalIgnoreCase))
        {
            VisibleDeletePopup = false;
            IsEnabled = false;
            IsViewMode = true;
            TitlePopup = "View Disbursement";

            _username = !string.IsNullOrWhiteSpace(row.DisplayName) ? row.DisplayName : row.EmployeeNumber;
            _employeeId = row.EmployeeNumber;
            _requestData = new DisbursementRequest
            {
                Id = row.Id,
                RequestId = row.RequestId,
                ApplicantId = row.ApplicantId,
                ItemId = row.ItemId,
                CurrencyType = (int)row.CurrencyType,
                Amount = row.Amount,
                Notes = row.Notes ?? string.Empty,
                StudyAssignmentFeeId = row.StudyAssignmentFeeId,
                DocumentId = row.DocumentId,
                Country = row.Country,
                City = row.City
            };

            if (!string.IsNullOrWhiteSpace(row.PaymentFrequency))
            {
                _frequensi = row.PaymentFrequency;
            }
            else
            {
                var item = _items?.SingleOrDefault(x => x.Id == row.ItemId);
                if (item != null)
                {
                    var fee = item.StudyAssignmentFee?.FirstOrDefault();
                    if (fee != null)
                    {
                        if (fee.PeriodType == "Month")
                        {
                            _frequensi = $"Period - {fee.Perod} Month";
                        }
                        else
                        {
                            _frequensi = fee.PeriodType == "Times" ? $"Period - {fee.Perod} Times" : $"Period - {fee.Perod} {fee.PeriodType}";
                        }
                    }
                    else
                    {
                        _frequensi = string.Empty;
                    }
                }
                else
                {
                    _frequensi = string.Empty;
                }
            }

            _getListDocuments = new List<GetDocumentsDocument>();
            if (row.Document != null)
            {
                var docIdProp = row.Document.GetType().GetProperty("Id");
                var docNameProp = row.Document.GetType().GetProperty("FileName") ?? row.Document.GetType().GetProperty("Name");
                var docIdVal = docIdProp?.GetValue(row.Document);
                var docNameVal = docNameProp?.GetValue(row.Document)?.ToString() ?? "attachment";

                _getListDocuments.Add(new GetDocumentsDocument
                {
                    Id = docIdVal is Guid g ? g : Guid.NewGuid(),
                    FileName = docNameVal
                });
            }

            VisiblePopup = true;
            StateHasChanged();
            return;
        }
    }

    private void DialogOpenForm()
    {
        VisiblePopup = true;
    }

    private void DialogCloseForm()
    {
        VisiblePopup = false;
        VisibleDeletePopup = false;
    }
    private void DialogOpenDelete()
    {

    }

    private void DialogCloseDelete()
    {
        VisibleDeletePopup = false;
    }
    private void ClickedNo()
    {
        DialogCloseForm();
    }

    private async Task ClickedYes()
    {
        if (_requestData.ItemId == 0)
        {
            _snackbar.AddWarning("Please input category!");
            //StateHasChanged();
            return;
        }

        else if (_requestData.CurrencyType == 0)
        {
            _snackbar.AddWarning("Please input Currency!");
            //StateHasChanged();
            return;
        }

        else if (_requestData.Amount == 0)
        {
            _snackbar.AddWarning("Please input amount!");
            //StateHasChanged();
            return;
        }

        else if (_requestData.Notes == null)
        {
            _snackbar.AddWarning("Please input notes!");
            //StateHasChanged();
            return;
        }

        else if (FileUploads.File == null)
        {
            _snackbar.AddWarning("Please browse document payment proof!");
            //StateHasChanged();
            return;
        }

        _isLoadingDialog = true;
        StateHasChanged(); // update tampilan agar spinner muncul

        try
        {
            await UploadDisbursement(_employeeId);
            var setData = _requestData;
            //setData.ApplicantId = _applicantId;
            if (_documentId is not null)
            {
                if (setData.Id is null)
                {
                    setData.DocumentId = (Guid)_documentId;
                    setData.PaymentStatus = 11;
                    var response = await _disbursementService.CreateDisbursementAsync(setData);

                    if (response.Error is not null)
                    {
                        _snackbar.AddError(response.Error.Detail);
                        VisiblePopup = false;
                    }
                    else
                    {
                        _snackbar.AddSuccess($"Success Create Disbursement");
                        VisiblePopup = false;
                        await SetDataOnLoadAsync();
                        //StateHasChanged();
                    }
                }
                else
                {
                    if (FileUploads.File is not null)
                    {
                        setData.DocumentId = (Guid)_documentId;
                    }

                    var response = await _disbursementService.UpdateDisbursementAsync(setData);

                    if (response.Error is not null)
                    {
                        _snackbar.AddError(response.Error.Detail);
                        VisiblePopup = false;
                    }
                    else
                    {
                        _snackbar.AddSuccess($"Success Update Disbursement");
                        VisiblePopup = false;
                        await SetDataOnLoadAsync();
                        //StateHasChanged();
                    }
                }
            }
        }
        finally
        {
            _isLoadingDialog = false;
            VisiblePopup = false;
            StateHasChanged();
        }
        // _isLoadingDialog = false;
        // StateHasChanged(); // refresh tampilan setelah selesai
        // await SetDataOnLoadAsync();
        // StateHasChanged();
        // DialogClose();
    }
    private async Task ClickedYesDelete()
    {
        try
        {
            VisibleDeletePopup = false;
            _isLoadingDialog = true;
            StateHasChanged();

            _requestData.Id = _deleteId;

            var response = await _disbursementService.DeleteDisbursementAsync(
                _requestData
            );

            if (response?.Error != null)
            {
                _snackbar.AddError(response.Error.Detail);
                return;
            }

            _snackbar.AddSuccess("Success delete disbursement");

            await SetDataOnLoadAsync(); // refresh grid
        }
        finally
        {
            _isLoadingDialog = false;
            VisibleDeletePopup = false;
            _deleteId = Guid.Empty;
            _requestData = new();
            StateHasChanged();
        }
    }
    private async Task<MemoryStream> UploadDisbursement(string employeeNumber)
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
                DocumentType = DocumentType.Disbursement,
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
    public void ExcelQueryCellInfoHandler(ExcelQueryCellInfoEventArgs<GetApplicantPaymentResponse> args)
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

    private void UpdateFilteredReimbursements()
    {
        var result = _selectedTab switch
        {
            "Progress" => _applicantDisbursement
                .Where(x => x.PaymentStatus is PaymentStatus.RequestPreparation),

            "Completed" => _applicantDisbursement
                .Where(x => x.PaymentStatus == PaymentStatus.Completed),

            _ => _applicantDisbursement
        };

        _filteredDisbursements = result.ToList();
    }

    private void OnTabChanged(int index)
    {
        if (index < 0)
        {
            index = 0;
        }

        _selectedTab = index switch
        {
            0 => "Progress",
            1 => "Completed",
            _ => "Progress"
        };

        UpdateFilteredReimbursements();
        StateHasChanged();
    }
    public async Task OnClickBack()
    {
        _navigationManager.NavigateTo(Smlp.Bsui.Features.Payments.Constants.RouteFor.Payment);
        //await Task.CompletedTask;
    }
}
