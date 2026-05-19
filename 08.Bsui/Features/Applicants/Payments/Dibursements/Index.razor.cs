using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Smlp.Base.Common;
using Smlp.Base.Payments.Enums;
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
using Syncfusion.Blazor.DropDowns;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Inputs;

namespace Smlp.Bsui.Features.Applicants.Payments.Dibursements;

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
    protected string? _university = string.Empty;
    private bool _isUniversityAdded = false;
    private List<DropdownItemResponse> _dropdownItemCurrencyType = new();
    private List<DropdownItemResponse> _itemsDropDown = new();
    private List<GetItemStudyAssignmentResponse> _items = new();
    private DisbursementRequest _requestData = new();
    private string? _frequensi;
    protected long _maximumFileSizeInBytes;

    protected List<GetApplicantDisbursementResponse> _applicantDisbursement = new();
    private SfGrid<GetApplicantDisbursementResponse> Grid { get; set; } = new();
    protected List<GetDocumentsDocument> _getListDocuments = new();
    protected string _employeeId = string.Empty;
    protected string _username = string.Empty;
    private Guid _applicantId = default!;

    private string TitlePopup { get; set; } = "Add Disbursement";
    private bool VisiblePopup { get; set; }
    private bool IsEnabled { get; set; }

    private bool VisiblePopupDelete { get; set; } = false;
    private Guid _deleteId;

    protected int _itemId = default!;
    protected string? _country = default!;
    protected string? _city = default!;
    private string? DeleteType { get; set; }
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
    private string _selectedTab = "Progress";
    protected Guid? _documentId;
    public FileUploadResponse FileUploads { get; set; } = new();

    public List<DropdownItemStringResponse>? _cityDropdownList = new();
    public List<DropdownItemStringResponse>? _countryDropdownList = new();

    private decimal _countcurrency1;
    private decimal _countcurrency2;
    private decimal _countcurrency3;
    private decimal _countcurrency4;
    private decimal _countcurrency5;
    private decimal _countcurrency6;
    private int _count1;
    private int _count2;
    private int _count3;
    private int _count4;
    private int _count5;
    private int _count6;

    protected override async Task OnInitializedAsync()
    {
        _isLoading = true; // start global loading
        try
        {
            _user = (await AuthenticationStateTask).User;
            _displayName = _user.GetDisplayName() ?? string.Empty;
            _employeeId = _user.GetEmployeeId() ?? string.Empty;
            _maximumFileSizeInBytes = _documentOptions.Value.MaximumFileSizeInBytes;
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
                var data = applicat.Result!.Items.Where(x => x.Disbursement).SingleOrDefault();
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
                    _username = firstdata!.Name;
                }
            }

            try
            {
                var uniResp = await _universityAdmissionSuperiorLetterService.GetUniversityAdmissionSuperiorLetterByApplicantIdAsync(_applicantId);
                if (uniResp.Error is null && uniResp.Result is not null)
                {
                    _isUniversityAdded = uniResp.Result.Items.Any();
                }
                else
                {
                    // jika error, anggap belum ada data (atau sesuaikan jika ingin behavior lain)
                    _isUniversityAdded = false;
                }
            }
            catch
            {
                _isUniversityAdded = false;
            }

            await GetDropdownnItemStudyAssigments();
            await SetDataOnLoadAsync();
        }
        catch (Exception ex)
        {

        }
        finally
        {
            _isLoading = false; // stop global loading
            StateHasChanged();
        }
    }
    private async Task SetDataOnLoadAsync()
    {
        var request = new GetApplicantDisbursementRequest()
        {
            ApplicantId = _applicantId
        };
        var response = await _disbursementService.GetApplicantDisbursementAsync(request);
        if (response.Error is null)
        {
            _applicantDisbursement = response!.Result!.Items.ToList();
            _countcurrency1 = (decimal)_applicantDisbursement.Where(x => x.PaymentStatus == PaymentStatus.RequestPreparation).Where(x => x.CurrencyType == CurrencyType.Usd).Sum(x => x.Amount);
            _countcurrency2 = (decimal)_applicantDisbursement.Where(x => x.PaymentStatus == PaymentStatus.RequestPreparation).Where(x => x.CurrencyType == CurrencyType.Idr).Sum(x => x.Amount);
            _countcurrency3 = (decimal)_applicantDisbursement.Where(x => x.PaymentStatus == PaymentStatus.RequestToBmc).Where(x => x.CurrencyType == CurrencyType.Usd).Sum(x => x.Amount);
            _countcurrency4 = (decimal)_applicantDisbursement.Where(x => x.PaymentStatus == PaymentStatus.RequestToBmc).Where(x => x.CurrencyType == CurrencyType.Idr).Sum(x => x.Amount);
            _countcurrency5 = (decimal)_applicantDisbursement.Where(x => x.PaymentStatus == PaymentStatus.Completed).Where(x => x.CurrencyType == CurrencyType.Usd).Sum(x => x.Amount);
            _countcurrency6 = (decimal)_applicantDisbursement.Where(x => x.PaymentStatus == PaymentStatus.Completed).Where(x => x.CurrencyType == CurrencyType.Idr).Sum(x => x.Amount);

            _count1 = _applicantDisbursement.Where(x => x.PaymentStatus == PaymentStatus.RequestPreparation).Where(x => x.CurrencyType == CurrencyType.Usd).Count();
            _count2 = _applicantDisbursement.Where(x => x.PaymentStatus == PaymentStatus.RequestPreparation).Where(x => x.CurrencyType == CurrencyType.Idr).Count();
            _count3 = _applicantDisbursement.Where(x => x.PaymentStatus == PaymentStatus.RequestToBmc).Where(x => x.CurrencyType == CurrencyType.Usd).Count();
            _count4 = _applicantDisbursement.Where(x => x.PaymentStatus == PaymentStatus.RequestToBmc).Where(x => x.CurrencyType == CurrencyType.Idr).Count();
            _count5 = _applicantDisbursement.Where(x => x.PaymentStatus == PaymentStatus.Completed).Where(x => x.CurrencyType == CurrencyType.Usd).Count();
            _count6 = _applicantDisbursement.Where(x => x.PaymentStatus == PaymentStatus.Completed).Where(x => x.CurrencyType == CurrencyType.Idr).Count();
        }

        // OnTabChanged(ActiveTabIndex);
        UpdateFilteredReimbursements();
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

    protected async Task OnClickAddAsync()
    {
        // Clear data
        _requestData = new();
        _getListDocuments = new();
        _countryDropdownList = new();
        _cityDropdownList = new();
        _frequensi = string.Empty;
        _documentId = null;
        FileUploads = new();
        IsAuploadedDocument = false;

        // Set properties
        TitlePopup = "Add Disbursement";
        IsEnabled = true;
        VisiblePopup = true;
    }
    private void DialogOpen()
    {
        VisiblePopup = true;
    }

    private void DialogClose()
    {
        VisiblePopup = false;
    }

    private async Task ClickedNo()
    {
        VisiblePopup = false;
    }
    private async Task ClickedYes()
    {
        _isLoadingDialog = true;
        StateHasChanged(); // update tampilan agar spinner muncul

        await UploadDisbursement(_employeeId);
        var setData = _requestData;
        setData.ApplicantId = _applicantId;
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
                    StateHasChanged();
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
                    StateHasChanged();
                }
            }
        }

        _isLoadingDialog = false;
        StateHasChanged(); // refresh tampilan setelah selesai
    }
    public async void OnCommandClickedDocument(CommandClickEventArgs<GetDocumentsDocument> args)
    {
        if (args.CommandColumn.Type == CommandButtonType.None)
        {
            if (args.CommandColumn.Title == "Download")
            {
                await OnClickDownload(args.RowData.Id);
            }
            else if (args.CommandColumn.Title == "Delete")
            {
                _documentId = args.RowData.Id;
                VisiblePopupDelete = true;
                DeleteType = "DeleteDocument";
            }

        }
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
    public void OnCommandClicked(CommandClickEventArgs<GetApplicantDisbursementResponse> args)
    {
        _username = args.RowData.DisplayName;
        _employeeId = args.RowData.EmployeeNumber;
        _getListDocuments = new();

        //handle delete
        if (args.CommandColumn.Type == CommandButtonType.Delete)
        {
            _deleteId = args.RowData.Id;
            _requestData = new DisbursementRequest
            {
                Id = args.RowData.Id,
                RequestId = args.RowData.RequestId,
                ApplicantId = args.RowData.ApplicantId,
                ItemId = args.RowData.ItemId
            };
            VisiblePopupDelete = true;
            DeleteType = "DeleteDisbursement";
            StateHasChanged();
            return;
        }

        if (args.CommandColumn.Type == CommandButtonType.Edit)
        {

            var query = _items.Where(x => x.Id == args.RowData.ItemId).SingleOrDefault();
            var result = query.StudyAssignmentFee.Select(x => x.Country).Distinct().ToList();
            _countryDropdownList = result.Select(x => new DropdownItemStringResponse { Id = x, Description = x }).Distinct().ToList();

            var resultCity = query.StudyAssignmentFee.Where(x => x.Country == args.RowData.Country).Select(x => x.City).Distinct().ToList();
            _cityDropdownList = resultCity.Select(x => new DropdownItemStringResponse { Id = x, Description = x }).Distinct().ToList();
            IsEnabled = true;
            VisiblePopup = true;
            _requestData.CurrencyType = (int)args.RowData.CurrencyType;
            _requestData.Amount = args.RowData.Amount;
            _requestData.StudyAssignmentFeeId = (int)args.RowData.StudyAssignmentFeeId;
            //_requestData.Ite = (int)args.RowData.StudyAssignmentFeeId;
            _frequensi = args.RowData.PaymentFrequency;
            _requestData.Notes = args.RowData.Notes;
            _requestData.RequestDate = args.RowData.RequestDate;
            _requestData.Id = args.RowData.Id;
            _requestData.City = args.RowData.City;
            _requestData.Country = args.RowData.Country;
            _requestData.DocumentId = args.RowData.Document.Id;
            _requestData.PaymentStatus = (int)args.RowData.PaymentStatus;
            _requestData.ItemId = args.RowData.ItemId;
            DeleteType = "DeleteDisbursement";
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
        }
        else if (args.CommandColumn.Type == CommandButtonType.None)
        {
            var query = _items.Where(x => x.Id == args.RowData.ItemId).SingleOrDefault();
            var result = query.StudyAssignmentFee.Select(x => x.Country).Distinct().ToList();
            _countryDropdownList = result.Select(x => new DropdownItemStringResponse { Id = x, Description = x }).Distinct().ToList();

            var resultCity = query.StudyAssignmentFee.Where(x => x.Country == args.RowData.Country).Select(x => x.City).Distinct().ToList();
            _cityDropdownList = resultCity.Select(x => new DropdownItemStringResponse { Id = x, Description = x }).Distinct().ToList();

            IsEnabled = false;
            VisiblePopup = true;
            _requestData.CurrencyType = (int)args.RowData.CurrencyType;
            _requestData.Amount = args.RowData.Amount;
            _requestData.StudyAssignmentFeeId = (int)args.RowData.StudyAssignmentFeeId;
            //_requestData.Ite = (int)args.RowData.StudyAssignmentFeeId;
            _frequensi = args.RowData.PaymentFrequency;
            _requestData.Notes = args.RowData.Notes;
            _requestData.RequestDate = args.RowData.RequestDate;
            _requestData.Id = args.RowData.Id;
            _requestData.City = args.RowData.City;
            _requestData.Country = args.RowData.Country;
            _requestData.DocumentId = args.RowData.Document.Id;
            _requestData.PaymentStatus = (int)args.RowData.PaymentStatus;
            _requestData.ItemId = args.RowData.ItemId;
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
        }
    }
    private void OnActionClick(GetApplicantDisbursementResponse row, string actionType)
    {
        _username = row.DisplayName;
        _employeeId = row.EmployeeNumber;
        _getListDocuments = new();

        // --- LOGIKA DELETE ---
        if (actionType == "Delete")
        {
            _deleteId = row.Id;
            _requestData = new DisbursementRequest
            {
                Id = row.Id,
                RequestId = row.RequestId,
                ApplicantId = row.ApplicantId,
                ItemId = row.ItemId
            };
            VisiblePopupDelete = true;
            DeleteType = "DeleteDisbursement";
            StateHasChanged();
            return;
        }

        // --- LOGIKA VIEW ---
        if (actionType == "View")
        {
            TitlePopup = "View Disbursement";

            var query = _items.Where(x => x.Id == row.ItemId).SingleOrDefault();

            _countryDropdownList = new List<DropdownItemStringResponse>();
            _cityDropdownList = new List<DropdownItemStringResponse>();

            if (query != null)
            {
                if (query.StudyAssignmentFee.Count() > 1)
                {
                    var result = query.StudyAssignmentFee.Select(x => x.Country).Distinct().ToList();
                    _countryDropdownList = result.Select(x => new DropdownItemStringResponse { Id = x, Description = x }).Distinct().ToList();

                    var resultCity = query.StudyAssignmentFee.Where(x => x.Country == row.Country).Select(x => x.City).Distinct().ToList();
                    _cityDropdownList = resultCity.Select(x => new DropdownItemStringResponse { Id = x, Description = x }).Distinct().ToList();
                }
            }

            IsEnabled = false;
            VisiblePopup = true;

            _requestData = new DisbursementRequest
            {
                Id = row.Id,
                RequestId = row.RequestId,
                ApplicantId = row.ApplicantId,
                ItemId = row.ItemId,
                CurrencyType = (int)row.CurrencyType,
                Amount = row.Amount,
                StudyAssignmentFeeId = (int)row.StudyAssignmentFeeId,
                Notes = row.Notes,
                RequestDate = row.RequestDate,
                City = row.City,
                Country = row.Country,
                PaymentStatus = (int)row.PaymentStatus
            };

            _country = row.Country;
            _city = row.City;
            _frequensi = row.PaymentFrequency;

            if (row.Document != null)
            {
                _requestData.DocumentId = row.Document.Id;
                _documentId = row.Document.Id;
                IsAuploadedDocument = true;
                _getListDocuments.Add(new GetDocumentsDocument
                {
                    Id = row.Document.Id,
                    FileName = row.Document.FileName,
                    DocumentType = DocumentType.Reimbursement,
                });
            }
            else
            {
                IsAuploadedDocument = false;
            }

            StateHasChanged();
        }
    }

    // field untuk cache
    private List<GetApplicantDisbursementResponse> _filteredDisbursements = new();

    // panggil ini saat data awal di-load atau saat tab berubah
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

    // property cuma mengembalikan cache (no LINQ evaluation here)
    private IEnumerable<GetApplicantDisbursementResponse> FilteredDisbursements => _filteredDisbursements;

    private void OnTabChanged(int index)
    {
        if (index < 0)
        {
            index = 0;
        }

        var newTab = index switch
        {
            0 => "Progress",
            1 => "Completed",
            _ => "Progress"
        };

        _selectedTab = newTab;

        UpdateFilteredReimbursements();
        StateHasChanged();
    }
    private void DialogOpenDelete()
    {
        VisiblePopupDelete = true;
    }

    private void DialogCloseDelete()
    {
        VisiblePopupDelete = false;
    }

    private async Task ClickedYesDelete()
    {
        VisiblePopupDelete = false;
        _isLoadingDialog = true;

        try
        {
            await InvokeAsync(StateHasChanged);

            _requestData.Id = _deleteId;

            var response = await _disbursementService.DeleteDisbursementAsync(_requestData);

            if (response?.Error != null)
            {
                _snackbar.AddError(response.Error.Detail);
                return;
            }

            _snackbar.AddSuccess("Success delete disbursement");

            await SetDataOnLoadAsync();
        }
        catch (Exception ex)
        {
            _snackbar.AddError($"Error: {ex.Message}");
        }
        finally
        {
            _isLoadingDialog = false;
            VisiblePopupDelete = false;
            _deleteId = Guid.Empty;
            _requestData = new();

            await InvokeAsync(StateHasChanged);
        }
    }

    protected void OnChangeItemHandler(ChangeEventArgs<int, DropdownItemResponse> args)
    {
        var query = _items.Where(x => x.Id == args.Value).SingleOrDefault();
        if (query == null)
        {
            return;
        }

        // Reset frequency string
        if (query.StudyAssignmentFee.FirstOrDefault()!.PeriodType == "Month")
        {
            _frequensi = $"Period - {query.StudyAssignmentFee.FirstOrDefault()!.Perod} Month";
        }
        else if (query.StudyAssignmentFee.FirstOrDefault()!.PeriodType == "Times")
        {
            _frequensi = $"Period - {query.StudyAssignmentFee.FirstOrDefault()!.Perod} Times";
        }

        if (query!.StudyAssignmentFee.Count() > 1)
        {
            var result = query.StudyAssignmentFee.Select(x => x.Country).Distinct().ToList();
            _countryDropdownList = result.Select(x => new DropdownItemStringResponse { Id = x, Description = x }).Distinct().ToList();

            _cityDropdownList = new List<DropdownItemStringResponse>();
            _requestData!.Country = null;
            _requestData!.City = null;
        }
        else
        {
            _requestData!.Amount = query.StudyAssignmentFee.FirstOrDefault()!.Fee;
            _requestData!.StudyAssignmentFeeId = query.StudyAssignmentFee.Select(x => x.Id).First();

            _countryDropdownList = new List<DropdownItemStringResponse>();
            _cityDropdownList = new List<DropdownItemStringResponse>();
            _requestData!.Country = null;
            _requestData!.City = null;
        }
    }
    protected void OnChangeCountryHandler(ChangeEventArgs<string, DropdownItemStringResponse> args)
    {
        var query = _items.Where(x => x.Id == _requestData!.ItemId).SingleOrDefault();
        if (query != null && query.StudyAssignmentFee.Count() > 1)
        {
            var result = query.StudyAssignmentFee.Where(x => x.Country == args.Value).Select(x => x.City).Distinct().ToList();
            _cityDropdownList = result.Select(x => new DropdownItemStringResponse { Id = x, Description = x }).Distinct().ToList();
            _requestData!.Country = args.Value;
            _requestData!.City = null;
            _country = args.Value;
            StateHasChanged();
        }
    }
    protected void OnChangeCityHandler(ChangeEventArgs<string, DropdownItemStringResponse> args)
    {
        var query = _items.Where(x => x.Id == _requestData!.ItemId).SingleOrDefault();
        if (query != null && query.StudyAssignmentFee.Count() > 1)
        {
            _requestData!.Amount = (decimal)query.StudyAssignmentFee.Where(x => x.City == args.Value).Select(x => x.Fee).First();
            _requestData!.StudyAssignmentFeeId = query.StudyAssignmentFee.Where(x => x.City == args.Value).Select(x => x.Id).First();
            _requestData!.City = args.Value;
            _city = args.Value;
            StateHasChanged();
        }
    }
}
