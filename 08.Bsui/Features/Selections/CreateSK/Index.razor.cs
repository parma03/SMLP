using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Psmp.Base.Scholarships.Enums;
using Psmp.Base.Selections.Enums;
using Psmp.Bsui.Common.Constants;
using Psmp.Bsui.Common.Extensions;
using Psmp.Shared.Common.Constants;
using Psmp.Shared.Common.Extensions;
using Psmp.Shared.Common.Responses;
using Psmp.Shared.Documents.Commands.CreateDocument;
using Psmp.Shared.Documents.Queries.GetDocumentFile;
using Psmp.Shared.Documents.Queries.GetDocuments;
using Psmp.Shared.Emails.Commands.SendEmail;
using Psmp.Shared.Masters.Queries;
using Psmp.Shared.Scholarships.ScholarshipDecree.Commands.Create;
using Psmp.Shared.Scholarships.ScholarshipPosting.Commands.Update;
using Psmp.Shared.Scholarships.ScholarshipPosting.Queries;
using Psmp.Shared.Selections.BeritaAcara.Commands.Update;
using Psmp.Shared.Selections.CreateSK.Commands.Create;
using Psmp.Shared.Selections.CreateSK.Commands.Update;
using Psmp.Shared.Selections.CreateSK.Queries;
using Psmp.Shared.Selections.SelectionOverviews.Commands.Update;
using Psmp.Shared.Selections.SelectionTalentReviews.Commands.Update;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Inputs;
using Syncfusion.Blazor.Navigations;
using RouteFor = Psmp.Bsui.Features.Selections.CreateSK.Constants.RouteFor;
using SelectionType = Psmp.Base.Selections.Enums.SelectionType;

namespace Psmp.Bsui.Features.Selections.CreateSK;

public partial class Index
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

    protected ClaimsPrincipal _user = default!;
    protected List<ListScopeResponse> _listScope = new();
    [Parameter]
    public int Type { get; set; }
    protected ErrorResponse? _error;
    protected bool _isLoading = false;
    protected bool _isLoadingData = false;
    protected bool _isLoadingButton = false;
    protected string _applicantSummary = "0";
    protected string _approvedSummary = "0";
    protected string _inProgressSummary = "0";
    protected string _stageManagerCount = "0";
    protected string _stageVPCount = "0";
    protected string _stageSVPCount = "0";
    protected string _stageDirectorCount = "0";

    protected List<Guid> PostingIds { get; set; } = new();
    protected Guid? _scholarshipPostingId = null;
    protected string? _companyCode = null;
    protected string? _companyCodeApplicantFilter = null;
    protected int? _companyGroupId = null;
    protected int? _companyGroupIdApplicantFilter = null;
    protected int _directorateId = 0;
    protected int _divisionId = 0;
    protected string? _codeId = null;
    protected int? _batch = null;
    protected int? _year = null;
    protected ScholarshipLocation _category = 0;
    public string? TypeDelete { get; set; }

    protected string? Stepper1 { get; set; }
    protected string? Stepper2 { get; set; }
    protected string? Stepper3 { get; set; }
    protected string? Stepper4 { get; set; }
    protected int _activeStep = 4;
    private bool VisiblePopupDecree { get; set; }
    private string TitlePopupDecree { get; set; } = "Scholarship Decree";
    private bool VisiblePopupDelete { get; set; } = false;
    protected Guid DocumentId { get; set; } = Guid.Empty;
    protected long _maximumFileSizeInBytes = long.MaxValue;
    protected string? IsLoadingButtonTitle { get; set; } = "Loading...";
    private bool DisabledButtonCreateSP { get; set; } = false;
    private bool DisabledButtonCreateSK { get; set; } = false;
    private bool DisabledButtonSendSK { get; set; } = false;
    private bool VisiblePopupWarrant { get; set; }
    private string TitlePopupWarrant { get; set; } = "Scholarship Warrant";

    protected List<GetSelectionCreateSKQueryResponse> _selectionCreateSK = new();
    private SfGrid<GetSelectionCreateSKQueryResponse> Grid { get; set; } = new();
    protected List<GetSelectionDecreeByBatchQueryResponse> _selectionDecreeByBatch = new();
    public IEnumerable<ApplicantStatusResponse> _applicantStatusResponse = GetEnumDisplayNames<ApplicantStatus>();
    protected List<DropdownItemResponse> _dropdownCategories = new();
    protected List<DropdownItemResponse> _dropdownYear = new() { new() { Id = null, Description = "Select All" } };
    protected List<DropdownItemGuidResponse> _dropdownCodes = new() { new() { Id = null, Description = "Select All" } };
    protected List<DropdownItemStringResponse> _dropdownComapies = new() { new() { Id = null, Description = "Select All" } };
    protected List<GetDirectorateQueryResponse> _dropdownDirecrorates = new();
    protected List<GetDivisionQueryResponse> _dropdownDivisions = new();
    //Applicant filter
    protected List<DropdownItemStringResponse> _dropdownComapiesApplicantFilter = new() { new() { Id = null, Description = "Select All" } };
    public List<DropdownItemResponse> _dropdownBatchs = new() { new() { Id = 0, Description = "Select All" } };

    public List<DropdownItemResponse> _dropdownCompanyGroups = new() { new() { Id = null, Description = "Select All" } };
    public List<DropdownItemResponse> _dropdownCompanyGroupsApplicantFilter = new() { new() { Id = null, Description = "Select All" } };

    public FileUploadResponse FileUploads { get; set; } = new();
    public FileUploadResponse FileUploadWarant { get; set; } = new();
    protected List<GetDocumentsDocument> _getListDocuments = new();
    protected List<GetDocumentsDocument> _getListDocumentWarrant = new();
    protected UpdateScholarshipPostingStatusRequest UpdateDataScholarshipPostingStatus { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        _user = (await AuthenticationStateTask).User;
        if (_user.Identity!.IsAuthenticated)
        {
            _isLoading = true;
            var scope = _user.Claims.Where(p => p.Type.ToLower().Trim() == "permission").ToList();
            if (scope.Count > 0)
            {
                foreach (var item in scope)
                {
                    _listScope.Add(new ListScopeResponse
                    {
                        ScopeName = item.Value
                    });
                }
            }

            await GetDropdownCompanyGroup();
            await GetDropdownCompany();
            await GetDropdownCategories();
            //await GetDropdownBatchs();
            await GetDropdownCompanyGroupApplicantFilter();
            await GetDropdownCompanyApplicantFilter();
            await GetDropdownCodes();
            await SetDataOnLoadAsync();
            //OnDisabledButton();
            _isLoading = false;
        }

        _isLoading = false;
    }
    private async Task SetDataOnLoadAsync()
    {
        _isLoadingData = true;
        var param = new GetSelectionDecreeFilterRequest
        {
            CompanyCode = _companyCode,
            Category = _category,
            SelectionBatch = _batch,
            CompanyGroupId = _companyGroupId,
            DirectorateId = _directorateId,
            DivisionId = _divisionId,
            Code = _codeId,
            CompanyGroupIdApplicantFilter = _companyGroupIdApplicantFilter,
            CompanyCodeApplicantFilter = _companyCodeApplicantFilter,
            SelectionType = (SelectionType)Type,
            Year = _year,
            AdminCompany = _user.GetCompanyCode(),
        };
        var response = await _selectionDecreesService.GetSelectionDecreeListAsync(param);

        if (response.Error is null)
        {
            _selectionCreateSK = response.Result!.Items.ToList();
            _applicantSummary = _selectionCreateSK.Count().ToString();
            _approvedSummary = _selectionCreateSK.Where(x => x.Status == ApplicantStatus.ApprovedDirector).ToList().Count().ToString() + "/" + _applicantSummary;
            _inProgressSummary = _selectionCreateSK.Where(x => x.Status == ApplicantStatus.InProgress).ToList().Count().ToString() + "/" + _applicantSummary;
            //Stepper
            _stageManagerCount = _selectionCreateSK.Where(x => x.Stage == DecreeStage.Manager).ToList().Count().ToString();
            _stageVPCount = _selectionCreateSK.Where(x => x.Stage == DecreeStage.Vp).ToList().Count().ToString();
            _stageSVPCount = _selectionCreateSK.Where(x => x.Stage == DecreeStage.Svp).ToList().Count().ToString();
            _stageDirectorCount = _selectionCreateSK.Where(x => x.Stage == DecreeStage.Director).ToList().Count().ToString();

            Stepper1 = $"Manager CD ({_stageManagerCount}/{_applicantSummary})";
            Stepper2 = $"VP PCU ({_stageVPCount}/{_applicantSummary})";
            Stepper3 = $"SVP HCM ({_stageSVPCount}/{_applicantSummary})";
            Stepper4 = $"Dir. SDM ({_stageDirectorCount}/{_applicantSummary})";

            StateHasChanged();
        }
        else
        {
            _error = response.Error;
            _isLoadingData = false;
        }

        _isLoadingData = false;
    }
    #region Get Dropdown
    private async Task GetDropdownCategories()
    {
        _dropdownCategories = EnumExtentions.ToDropdownItem<ScholarshipLocation>();
        _dropdownCategories = _dropdownCategories.OrderBy(x => x.Id == 0).ToList();
        await Task.CompletedTask;
    }
    private async Task GetDropdownCodes()
    {
        var request = new GetScholarshipPostingDropdownRequest()
        {
            SelectionType = (SelectionType)Type,
            CompanyGroupId = _companyGroupId,
            ComapanyCode = _companyCode,
        };
        var response = await _scholarshipPostingService.GetScholarshipDropdownAsync(request);
        if (response.Error is null)
        {
            List<DropdownItemResponse> dropdownYears = new() { new() { Id = null, Description = "Select All" } };
            List<DropdownItemGuidResponse> dropdownCodess = new() { new() { Id = null, Description = "Select All" } };
            List<DropdownItemResponse> dropdownBatchss = new() { new() { Id = 0, Description = "Select All" } };

            var data = response.Result.Items;
            var dropdownCodes = dropdownCodess.Union(data.Where(x => _scholarshipPostingId != null ? x.Id == _scholarshipPostingId : true)
                .Select(x => new DropdownItemGuidResponse { Id = x.Id, Description = x.Code, Code = x.Code }).ToList()).ToList();
            _dropdownCodes = dropdownCodes;
            var year = data.Where(x => _scholarshipPostingId != null ? x.Id == _scholarshipPostingId : true).Select(x => x.DueDate!.Value.Year).Distinct().ToList();
            var dropdownYear = dropdownYears.Union(year.Select(x => new DropdownItemResponse { Id = x, Description = x.ToString() }).ToList()).ToList();
            _dropdownYear = dropdownYear;
            var batch = data.Where(x => _scholarshipPostingId != null ? x.Id == _scholarshipPostingId : true).Select(x => x.Batch).Distinct().OrderBy(x => x).ToList();
            var dropdownBatchs = dropdownBatchss.Union(batch.Select(x => new DropdownItemResponse { Description = $"Batch {x}", Id = x }).ToList()).ToList();
            _dropdownBatchs = dropdownBatchs;
        }

    }
    private async Task GetDropdownCompanyGroup()
    {
        var response = await _companyGroupService.GetCompanyGroupsAsync();
        if (response.Error is null)
        {
            var data = _dropdownCompanyGroups.Union(response.Result!.Items.Select(x => new DropdownItemResponse { Id = x.Id, Description = x.Name }).ToList()).ToList();
            //_companyGroupId = data.Select(x => x.Id).OrderBy(x => x).FirstOrDefault();
            _dropdownCompanyGroups = data;

        }
    }
    private async Task GetDropdownCompany()
    {
        var param = new GetCompanyRequest()
        {
            CompanyGroup = _companyGroupId
        };

        var response = await _companyService.GetCompanyByCompanyGroupAsync(param);
        if (response.Error is null)
        {
            List<DropdownItemStringResponse> dropdownComapiess = new() { new() { Id = null, Description = "Select All" } };

            var data = response.Result!.Items.ToList();
            var dataComapies = data.OrderBy(x => x.Code).ToList();
            var dropdownComapies = dropdownComapiess.Union(dataComapies.Select(x => new DropdownItemStringResponse { Id = x.Code, Description = x.Name }).ToList()).ToList();
            _dropdownComapies = dropdownComapies;
        }
    }
    private async Task GetDropdownDirectorate()
    {
        var data = await _directorateService.GetDirectotaeByCompanyCodeAsync(_companyCode);
        if (data.Error is null)
        {
            _dropdownDirecrorates = data.Result!.Items.ToList();
        }
    }

    private async Task GetDropdownDivision()
    {
        var data = await _divisionService.GetDivisionByDirectorateIdAsync(_directorateId);
        if (data.Error is null)
        {
            _dropdownDivisions = data.Result!.Items.ToList();
        }
    }
    #endregion
    #region Dropdown Applicant Filter
    private async Task GetDropdownCompanyGroupApplicantFilter()
    {
        //var data = await _companyGroupService.GetCompanyGroupsAsync();
        //if (data.Error is null)
        //{
        //    _dropdownCompanyGroupsApplicantFilter = data.Result!.Items.ToList();
        //}

        var response = await _companyGroupService.GetCompanyGroupsAsync();
        if (response.Error is null)
        {
            var data = response.Result!.Items.Select(x => new DropdownItemResponse { Id = x.Id, Description = x.Name }).ToList();
            _dropdownCompanyGroupsApplicantFilter.AddRange(data);
        }
    }
    private async Task GetDropdownCompanyApplicantFilter()
    {
        var param = new GetCompanyRequest()
        {
            CompanyGroup = _companyGroupIdApplicantFilter
        };
        var response = await _companyService.GetCompanyByCompanyGroupAsync(param);
        if (response.Error is null)
        {
            List<DropdownItemStringResponse> dropdownComapiesApplicantFilters = new() { new() { Id = null, Description = "Select All" } };
            var data = response.Result!.Items.ToList();
            var dataComapies = data.OrderBy(x => x.Code).ToList();
            var dropdownComapies = dropdownComapiesApplicantFilters.Union(dataComapies.Select(x => new DropdownItemStringResponse { Id = x.Code, Description = x.Name }).ToList()).ToList();
            _dropdownComapiesApplicantFilter = dropdownComapies;
        }

        //var data = await _companyService.GetCompanyByCompanyGroupIdAsync(_companyGroupIdApplicantFilter);
        //if (data.Error is null)
        //{
        //    _dropdownComapiesApplicantFilter = data.Result!.Items.ToList();
        //    _dropdownComapiesApplicantFilter = _dropdownComapiesApplicantFilter.OrderBy(x => x.Code).ToList();
        //}
    }
    #endregion
    #region Applicant Filter
    public async Task OnChangeCompanyGroupApplicantFilterAsync(Syncfusion.Blazor.DropDowns.ChangeEventArgs<int?, DropdownItemResponse> args)
    {
        _companyGroupIdApplicantFilter = args.ItemData.Id;
        _companyCodeApplicantFilter = null;
        await GetDropdownCompanyApplicantFilter();
        await SetDataOnLoadAsync();
        StateHasChanged();
    }
    public async Task OnChangeCompanyApplicantFilterAsync(Syncfusion.Blazor.DropDowns.ChangeEventArgs<string?, DropdownItemStringResponse> args)
    {
        _companyCodeApplicantFilter = args.ItemData.Id;

        await SetDataOnLoadAsync();
        StateHasChanged();
    }
    #endregion
    #region Dropdown Change
    public async Task OnChangeYear(Syncfusion.Blazor.DropDowns.ChangeEventArgs<int?, DropdownItemResponse> args)
    {
        _year = args.Value;
        await SetDataOnLoadAsync();
        StateHasChanged();
    }
    public async Task OnChangeCategory(Syncfusion.Blazor.DropDowns.ChangeEventArgs<int, DropdownItemResponse> args)
    {
        if (args.ItemData.Id == 1)
        {
            _category = ScholarshipLocation.Abroad;
        }
        else if (args.ItemData.Id == 2)
        {
            _category = ScholarshipLocation.Domestic;
        }

        await SetDataOnLoadAsync();
        StateHasChanged();
    }
    public async Task OnChangeBatchAsync(Syncfusion.Blazor.DropDowns.ChangeEventArgs<int?, DropdownItemResponse> args)
    {
        if (args.ItemData == null)
        {
            _batch = 0;
        }
        else
        {
            _batch = args.ItemData.Id ?? default!;
        }

        await SetDataOnLoadAsync();
        StateHasChanged();
    }
    public async Task OnChangeCompanyAsync(Syncfusion.Blazor.DropDowns.ChangeEventArgs<string?, DropdownItemStringResponse> args)
    {
        _companyCode = args.ItemData.Id;
        _scholarshipPostingId = null;
        _codeId = null;
        _year = null;
        _batch = null;
        await GetDropdownCodes();
        await GetDropdownDirectorate();
        await SetDataOnLoadAsync();
        StateHasChanged();
    }
    public async Task OnChangeDirectorateAsync(Syncfusion.Blazor.DropDowns.ChangeEventArgs<int, GetDirectorateQueryResponse> args)
    {
        _directorateId = args.ItemData.Id;

        await GetDropdownDivision();
        await SetDataOnLoadAsync();
        StateHasChanged();
    }
    public async Task OnChangeDivisionAsync(Syncfusion.Blazor.DropDowns.ChangeEventArgs<int, GetDivisionQueryResponse> args)
    {
        _divisionId = args.ItemData.Id;

        await SetDataOnLoadAsync();
        StateHasChanged();
    }
    public async Task OnChangeCodeAsync(Syncfusion.Blazor.DropDowns.ChangeEventArgs<Guid?, DropdownItemGuidResponse> args)
    {
        _codeId = args.ItemData.Code;
        _scholarshipPostingId = args.ItemData.Id;
        _year = null;
        _batch = null;
        await GetDropdownCodes();
        await SetDataOnLoadAsync();
        StateHasChanged();
    }
    public async Task OnChangeCompanyGroupAsync(Syncfusion.Blazor.DropDowns.ChangeEventArgs<int?, DropdownItemResponse> args)
    {
        _companyGroupId = args.ItemData.Id;
        _companyCode = null;
        _scholarshipPostingId = null;
        _codeId = null;
        _year = null;
        _batch = null;
        await GetDropdownCodes();
        await GetDropdownCompany();
        await SetDataOnLoadAsync();
        StateHasChanged();
    }
    #endregion
    public static List<ApplicantStatusResponse> GetEnumDisplayNames<T>()
    {
        var type = typeof(T);
        return Enum.GetValues(type)
                   .Cast<T>()
                   .Select(x => new ApplicantStatusResponse
                   {
                       Value = x as ApplicantStatus?,
                       Text = type.GetMember(x!.ToString()!)
                   .First()
                   .GetCustomAttribute<DisplayAttribute>()?.Name ?? x.ToString()

                   }).Where(x => x.Value is ApplicantStatus.Passed || x.Value == ApplicantStatus.Failed || x.Value == ApplicantStatus.Rejected).ToList();
    }

    public void RowBound(RowDataBoundEventArgs<GetSelectionCreateSKQueryResponse> args)
    {
        if (args.Data.Status != ApplicantStatus.SKSend)
        {
            args.Row.AddClass(new string[] { "e-removeDeletecommand" });
        }
    }

    public void OnCommandClicked(CommandClickEventArgs<GetSelectionCreateSKQueryResponse> args)
    {
        if (args.CommandColumn.Type == CommandButtonType.None)
        {
            _navigationManager.NavigateTo(RouteFor.Details(args.RowData.Id));
        }
        else if (args.CommandColumn.Type == CommandButtonType.Delete)
        {
            _navigationManager.NavigateTo(RouteFor.Final(args.RowData.Batch, args.RowData.Year), true);
        }
    }
    #region Popup Delete
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
        if (TypeDelete == "decree")
        {
            var response = await _selectionDecreesService.DeleteSelectionDecreeLetterDocumentAsync(DocumentId);
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

            VisiblePopupDecree = false;
        }
        else
        {
            var response = await _selectionDecreesService.DeleteSelectionDecreeWarrantDocumentAsync(DocumentId);
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

            VisiblePopupWarrant = false;
        }

        _snackbar.AddSuccess("Success delete data");
        VisiblePopupDelete = false;
        _getListDocuments = new();
        await SetDataOnLoadAsync();
        StateHasChanged();
    }
    #endregion
    #region Popup create sk

    private void DialogDecreeOpen()
    {
        VisiblePopupDecree = true;
    }

    private void DialogDecreeClose()
    {
        VisiblePopupDecree = false;
        _getListDocuments = new();
        FileUploads = new();
    }
    private void OnClickNoDialogDecree()
    {
        VisiblePopupDecree = false;
        _getListDocuments = new();
        FileUploads = new();
    }
    private async Task OnClickCreateSK()
    {
        if (_scholarshipPostingId == Guid.Empty)
        {
            _snackbar.AddWarning("Please select code !");
            return;
        }

        if (_selectionCreateSK.Where(x => x.PostingId == _scholarshipPostingId && (x.Status == ApplicantStatus.ApprovedDirector || x.Status == ApplicantStatus.SKCreated || x.Status == ApplicantStatus.SKSend)).ToList().Count() == 0)
        {
            _snackbar.AddWarning("Applicant cannot complete !");
            return;
        }

        if (_selectionCreateSK.Count() > 0)
        {
            var getSelectionCreateSK = _selectionCreateSK.Where(x => x.PostingId == _scholarshipPostingId).FirstOrDefault();
            if (getSelectionCreateSK!.DocumentIdDecree is not null)
            {
                var document = await _documentService.GetDocumentByIdAsync(getSelectionCreateSK.DocumentIdDecree);
                if (document is not null)
                {
                    DocumentId = document.Result!.Id;
                    _getListDocuments.Add(new GetDocumentsDocument
                    {
                        Id = document.Result.Id,
                        FileName = document.Result.FileName,
                        DocumentType = DocumentType.ScholarshipDecree,
                    });
                }
            }

            await Task.Delay(100);
            VisiblePopupDecree = true;
        }
        else
        {
            _snackbar.AddWarning("Data is empty !");
        }
    }
    private async void OnClickYesDialogDecree()
    {
        _isLoadingButton = true;
        if (FileUploads.FileName is null)
        {
            _snackbar.AddWarning("Please browse file !");
            _isLoadingButton = false;
            StateHasChanged();
            return;
        }

        await UploadDocumentSholarshipDecree();

        if (DocumentId == Guid.Empty)
        {
            _snackbar.AddError("Error upload scholarship decree !");
            _isLoadingButton = false;
            StateHasChanged();
            return;
        }

        if (_getListDocuments.Count() > 0)
        {
            _snackbar.AddWarning("Please delete document before re-upload !");
            _isLoadingButton = false;
            StateHasChanged();
            return;
        }

        IsLoadingButtonTitle = "Loading...";
        var decreeDocumentRequest = new CreateScholarshipDecreeRequest
        {
            Type = "decree",
            ScholarshipPostingId = (Guid)_scholarshipPostingId,
            DocumentId = DocumentId,
        };

        var createDecreeLetter = await _selectionDecreesService.CreateSelectionDecreeLetterDocumentAsync(decreeDocumentRequest);
        if (createDecreeLetter.Error is not null)
        {
            var result = string.Empty;
            for (var i = 0; i < createDecreeLetter.Error.Details.Count; i++)
            {
                result = result + createDecreeLetter.Error.Details[i] + ",";
            }

            _snackbar.AddError(result);
            _isLoadingButton = false;
            StateHasChanged();
            return;
        }

        if (_selectionCreateSK.Where(x => x.Status == ApplicantStatus.ApprovedDirector).ToList().Count() > 0)
        {
            var data = _selectionCreateSK.Where(x => x.Status == ApplicantStatus.ApprovedDirector).ToList();
            foreach (var item in data)
            {
                //Update selection decree
                var decreeRequest = new UpdateSelectionDecreeRequest
                {
                    Id = item.Id,
                    Status = ApplicantStatus.SKCreated,
                    DecreeStage = DecreeStage.Director,
                };

                var decree = await _selectionDecreesService.UpdateSelectionDecreeAsync(decreeRequest);
                if (decree.Error is not null)
                {
                    _snackbar.AddError("Decree - Error : " + decree.Error.ToDetail());
                    _isLoadingButton = false;
                    StateHasChanged();
                    return;
                }
                //Create histories
                var historiesRequest = new CreateSelectionDecreeHistoriesRequest
                {
                    DecreeId = item.Id,
                    DecreeStage = DecreeStage.Director,
                };

                var histories = await _selectionDecreesService.CreateSelectionDecreeHistoriesAsync(historiesRequest);

                if (histories.Error is not null)
                {
                    _snackbar.AddError("Histories - Error : " + histories.Error.ToDetail());
                    _isLoadingButton = false;
                    StateHasChanged();
                    return;
                }
                //Update talent review
                var request = new UpdateSelectionTalentReviewRequest
                {
                    Id = item.TalentReviewId,
                    ApplicantId = item.ApplicantId,
                    Status = ApplicantStatus.SKCreated,
                };
                var uodateTalentReview = await _selectionTalentReviewService.UpdateSelectionTalentReviewAsync(request);
                if (uodateTalentReview.Error is not null)
                {
                    _snackbar.AddError("Talent Review - Error : " + uodateTalentReview.Error.ToDetail());
                    _isLoadingButton = false;
                    StateHasChanged();
                    return;
                }
                //Update selection overview
                var updateOverviewRequest = new UpdateSelectionOverviewByApplicantIdRequest
                {
                    ApplicantId = item.ApplicantId,
                    Stage = SelectionStage.Decree,
                    Status = ApplicantStatus.SKCreated,
                };

                var updateOverview = await _selectionOverviewService.UpdateSelectionOverviewByApplicantIdAsync(updateOverviewRequest);
                if (updateOverview.Error is not null)
                {
                    _snackbar.AddError("Overview - Error : " + updateOverview.Error.ToDetail());
                    _isLoadingButton = false;
                    IsLoadingButtonTitle = "";
                    StateHasChanged();
                    return;
                }

                var scholarshipPosting = await _scholarshipPostingService.GetScholarshipPostingByIdAsync(_scholarshipPostingId);
                if (scholarshipPosting.Error is not null)
                {
                    _snackbar.AddError("Get ScholarshipPosting - Error : " + scholarshipPosting.Error.ToDetail());
                    _isLoadingButton = false;
                    IsLoadingButtonTitle = "";
                    StateHasChanged();
                    return;
                }

                var dataScholarshipPosting = scholarshipPosting.Result;

                var postingStatus = new ScholarshipPostingStatus();
                var awardeeQuota = scholarshipPosting is not null ? scholarshipPosting.Result!.AwardeeQuota : 0;
                var awardeeSelection = _selectionCreateSK.Where(x => x.ScholarshipPostingId == _scholarshipPostingId && x.Status == ApplicantStatus.ApprovedDirector).ToList().Count();

                if (awardeeSelection >= awardeeQuota)
                {
                    postingStatus = ScholarshipPostingStatus.Completed;
                }
                else
                {
                    postingStatus = ScholarshipPostingStatus.Closed;
                }

                UpdateDataScholarshipPostingStatus.PostingId = _scholarshipPostingId;
                UpdateDataScholarshipPostingStatus.PostingStatus = postingStatus;
                var updatePostingStatus = await _scholarshipPostingService.UpdateScholarshipPostingStatusAsync(UpdateDataScholarshipPostingStatus);
                if (updatePostingStatus.Error is not null)
                {
                    _snackbar.AddError("Set ScholarshipPosting Status - Error : " + updatePostingStatus.Error.ToDetail());
                    _isLoadingButton = false;
                    IsLoadingButtonTitle = "";
                    StateHasChanged();
                    return;
                }
            }
        }

        _snackbar.AddSuccess("Success upload scholarship decree");
        await SetDataOnLoadAsync();
        _isLoadingButton = false;
        VisiblePopupDecree = false;
        FileUploads = new();
        _getListDocuments = new();
        StateHasChanged();

    }
    #endregion Popup create sk
    #region Popup create SP

    private void DialogWarrantOpen()
    {
        VisiblePopupWarrant = true;
    }

    private void DialogWarrantClose()
    {
        VisiblePopupWarrant = false;
        _getListDocumentWarrant = new();
        FileUploadWarant = new();
    }
    private void OnClickNoDialogWarrant()
    {
        VisiblePopupWarrant = false;
        _getListDocumentWarrant = new();
        FileUploadWarant = new();
    }
    private async Task OnClickCreateWarrant()
    {
        if (_scholarshipPostingId == Guid.Empty)
        {
            _snackbar.AddWarning("Please select code !");
            return;
        }

        if (_selectionCreateSK.Where(x => x.PostingId == _scholarshipPostingId && (x.Status == ApplicantStatus.ApprovedDirector || x.Status == ApplicantStatus.SKCreated || x.Status == ApplicantStatus.SKSend)).ToList().Count() == 0)
        {
            _snackbar.AddWarning("Applicant cannot complete !");
            return;
        }

        if (_selectionCreateSK.Count() > 0)
        {
            var getSelectionCreateSK = _selectionCreateSK.Where(x => x.PostingId == _scholarshipPostingId).FirstOrDefault();
            if (getSelectionCreateSK!.DocumentIdWarrant is not null)
            {
                var document = await _documentService.GetDocumentByIdAsync(getSelectionCreateSK.DocumentIdWarrant);
                if (document is not null)
                {
                    DocumentId = document.Result!.Id;
                    _getListDocumentWarrant.Add(new GetDocumentsDocument
                    {
                        Id = document.Result.Id,
                        FileName = document.Result.FileName,
                        DocumentType = DocumentType.ScholarshipWarrant,
                    });
                }
            }

            await Task.Delay(100);
            VisiblePopupWarrant = true;
        }
        else
        {
            _snackbar.AddWarning("Data is empty !");
        }
    }
    private async void OnClickYesDialogWarrant()
    {
        _isLoadingButton = true;
        if (FileUploadWarant.FileName is null)
        {
            _snackbar.AddWarning("Please browse file !");
            _isLoadingButton = false;
            StateHasChanged();
            return;
        }

        await UploadDocumentSholarshipWarrant();

        if (DocumentId == Guid.Empty)
        {
            _snackbar.AddError("Error upload scholarship decree !");
            _isLoadingButton = false;
            StateHasChanged();
            return;
        }

        if (_getListDocumentWarrant.Count() > 0)
        {
            _snackbar.AddWarning("Please delete document before re-upload !");
            _isLoadingButton = false;
            StateHasChanged();
            return;
        }

        IsLoadingButtonTitle = "Loading...";
        var decreeDocumentRequest = new CreateScholarshipDecreeRequest
        {
            Type = "warrant",
            ScholarshipPostingId = (Guid)_scholarshipPostingId,
            DocumentId = DocumentId,
        };

        var createDecreeLetter = await _selectionDecreesService.CreateSelectionDecreeLetterDocumentAsync(decreeDocumentRequest);
        if (createDecreeLetter.Error is not null)
        {
            _snackbar.AddError("Create SP - Error : " + createDecreeLetter.Error.ToDetail());
            _isLoadingButton = false;
            StateHasChanged();
            return;
        }

        _snackbar.AddSuccess("Success upload scholarship warrant");
        await SetDataOnLoadAsync();
        _isLoadingButton = false;
        VisiblePopupWarrant = false;
        FileUploadWarant = new();
        _getListDocumentWarrant = new();
        StateHasChanged();

    }
    #endregion Popup create SP

    private async Task<MemoryStream> UploadDocumentSholarshipDecree()
    {
        var memoryStream = new MemoryStream();
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
            DocumentType = DocumentType.ScholarshipDecree,
            File = file,
        };

        var resultUpload = await _documentService.CreateDocumentAsync(requestFile);
        if (resultUpload.Error is null)
        {
            DocumentId = resultUpload.Result!.DocumentId;
        }

        return memoryStream;
    }
    private async Task<MemoryStream> UploadDocumentSholarshipWarrant()
    {
        var memoryStream = new MemoryStream();
        await FileUploadWarant.File.OpenReadStream(_maximumFileSizeInBytes).CopyToAsync(memoryStream);
        memoryStream.Position = 0; // Reset the stream position

        var formFile = new FormFile(memoryStream, 0, FileUploadWarant.File.Size, FileUploadWarant.File.Name, FileUploadWarant.File.Name)
        {
            Headers = new HeaderDictionary(),
            ContentType = FileUploadWarant.File.ContentType
        };

        var file = IBrowserFileExtensions.ToFormFile(FileUploadWarant.File, memoryStream, FileUploadWarant.File.Name);

        var requestFile = new CreateDocumentRequest
        {
            DocumentType = DocumentType.ScholarshipWarrant,
            File = file,
        };

        var resultUpload = await _documentService.CreateDocumentAsync(requestFile);
        if (resultUpload.Error is null)
        {
            DocumentId = resultUpload.Result!.DocumentId;
        }

        return memoryStream;
    }
    protected void OnRemoveUploaderDecree(RemovingEventArgs args)
    {
        FileUploads = new();
        _getListDocuments = new();
        args.PostRawFile = false;
    }
    protected void OnRemoveUploaderWarrant(RemovingEventArgs args)
    {
        FileUploadWarant = new();
        _getListDocumentWarrant = new();
        args.PostRawFile = false;
    }
    protected void ValueChangeUploaderDecree(UploadChangeEventArgs args)
    {
        FileUploads.FileName = args.Files.First().File.Name;
        FileUploads.FileSize = args.Files.First().File.Size;
        FileUploads.FileContentType = args.Files.First().File.ContentType;
        FileUploads.File = args.Files.First().File;
    }
    protected void ValueChangeUploaderWarrant(UploadChangeEventArgs args)
    {
        FileUploadWarant.FileName = args.Files.First().File.Name;
        FileUploadWarant.FileSize = args.Files.First().File.Size;
        FileUploadWarant.FileContentType = args.Files.First().File.ContentType;
        FileUploadWarant.File = args.Files.First().File;
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
                TypeDelete = "decree";
                DocumentId = args.RowData.Id;
                VisiblePopupDelete = true;
            }

        }
    }
    public async void OnCommandClickedDocumentWarrant(CommandClickEventArgs<GetDocumentsDocument> args)
    {
        if (args.CommandColumn.Type == CommandButtonType.None)
        {
            if (args.CommandColumn.Title == "Download")
            {
                await OnClickDownload(args.RowData.Id, args.RowData.DocumentType);
            }
            else if (args.CommandColumn.Title == "Delete")
            {
                TypeDelete = "warrant";
                DocumentId = args.RowData.Id;
                VisiblePopupDelete = true;
            }

        }
    }
    private async Task OnClickDownload(Guid documentId, DocumentType documentType)
    {
        var request = new GetDocumentFileRequest
        {
            DocumentId = documentId,
            DocumentType = documentType,
        };

        var response = await _documentService.GetDocumentFileByDocumentTypeAsync(request);

        if (response.Error is not null)
        {
            _snackbar.AddError(response.Error.ToDetail());

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

    private void OnDisabledButton()
    {
        if (_listScope.Where(x => x.ScopeName!.ToLower().Contains("createsk") || x.ScopeName!.ToLower().Contains("sendsk")).ToList().Count() > 0)
        {
            DisabledButtonCreateSP = false;
            DisabledButtonCreateSK = false;
        }
        else
        {
            DisabledButtonCreateSP = true;
            DisabledButtonCreateSK = true;
        }
    }
    private async void OnClickSendSK()
    {
        _isLoadingButton = true;

        if (_batch is 0 or null)
        {
            _snackbar.AddWarning("Please select batch");
            _isLoadingButton = false;
            return;
        }

        var param = new GetSelectionDecreeByBatchFilterRequest
        {
            SelectionBatch = Convert.ToInt32(_batch),
            Year = _year

        };

        var getPosting = await _selectionDecreesService.GetSelectionDecreeDocumentByBatchAsync(param);
        if (getPosting.Result is not null)
        {
            if (getPosting.Result.Items.Count > 0)
            {
                var itemPosting = getPosting.Result.Items;

                var lastPosting = itemPosting.LastOrDefault();
                if (lastPosting != null)
                {
                    if (lastPosting.DecreeDocumentId is null || lastPosting.DecreeDocumentId == Guid.Empty)
                    {
                        _snackbar.AddWarning("Please upload Surat Keputusan !");
                        _isLoadingButton = false;
                        StateHasChanged();
                        return;
                    }
                }
                else
                {
                    _snackbar.AddWarning("Please upload Surat Keputusan !");
                    _isLoadingButton = false;
                    StateHasChanged();
                    return;
                }

                var getApplicantByBatch = await _selectionDecreesService.GetSelectionDecreeByBatchAsync(param);
                if (getApplicantByBatch.Result is not null && getApplicantByBatch.Result.Items.Count() > 0)
                {
                    _selectionDecreeByBatch = getApplicantByBatch.Result.Items.ToList();
                }

                var statusSk = new List<ApplicantStatus> { ApplicantStatus.SKSend, ApplicantStatus.SKCreated };

                foreach (var itemDecree in itemPosting)
                {
                    _scholarshipPostingId = itemDecree.ScholarshipPostingId;
                    var sendEmailRequest = new SendEmailRequest
                    {
                        PostingId = (Guid)_scholarshipPostingId,
                    };

                    var data = _selectionDecreeByBatch.Where(x => x.ScholarshipPostingId == _scholarshipPostingId && statusSk.Contains(x.Status)).ToList();
                    if (data.Count() > 0)
                    {
                        foreach (var item in data)
                        {
                            //Update selection decree
                            var decreeRequest = new UpdateSelectionDecreeRequest
                            {
                                Id = item.Id,
                                Status = ApplicantStatus.SKSend,
                                DecreeStage = DecreeStage.Director,
                            };

                            var decree = await _selectionDecreesService.UpdateSelectionDecreeAsync(decreeRequest);
                            if (decree.Error is not null)
                            {
                                _snackbar.AddError("Decree - Error : " + decree.Error.ToDetail());
                                _isLoadingButton = false;
                                StateHasChanged();
                                return;
                            }

                            //Update talent review
                            var request = new UpdateSelectionTalentReviewRequest
                            {
                                Id = item.TalentReviewId,
                                ApplicantId = item.ApplicantId,
                                Status = ApplicantStatus.Passed,
                            };
                            var uodateTalentReview = await _selectionTalentReviewService.UpdateSelectionTalentReviewAsync(request);
                            if (uodateTalentReview.Error is not null)
                            {
                                _snackbar.AddError("Talent Review - Error : " + uodateTalentReview.Error.ToDetail());
                                _isLoadingButton = false;
                                StateHasChanged();
                                return;
                            }

                            //Update selection overview
                            var updateOverviewRequest = new UpdateSelectionOverviewByApplicantIdRequest
                            {
                                ApplicantId = item.ApplicantId,
                                Stage = SelectionStage.TalentReview,
                                Status = ApplicantStatus.Passed,
                            };

                            var updateOverview = await _selectionOverviewService.UpdateSelectionOverviewByApplicantIdAsync(updateOverviewRequest);
                            if (updateOverview.Error is not null)
                            {
                                _snackbar.AddError("Overview - Error : " + updateOverview.Error.ToDetail());
                                _isLoadingButton = false;
                                IsLoadingButtonTitle = "";
                                StateHasChanged();
                                return;
                            }

                            var requestStatus = new UpdateSelectionBeritaAcaraApplicantRequest
                            {
                                ScholarshipPostingApplicantId = item.ApplicantId,
                                Status = ApplicantStatus.SKSend
                            };
                            var updateStatus = await _selectionBeritaAcaraService.UpdateSelectionApplicantBeritAcaraApplicantAsync(requestStatus);
                            if (updateStatus.Error is not null)
                            {
                                _snackbar.AddError("Berita Acara Error : " + updateStatus.Error.ToDetail());
                                _isLoadingButton = false;
                                StateHasChanged();
                                return;
                            }
                        }

                        //Update scholarship
                        var scholarshipPosting = await _scholarshipPostingService.GetScholarshipPostingByIdAsync(_scholarshipPostingId);
                        if (scholarshipPosting.Error is not null)
                        {
                            _snackbar.AddError("Get ScholarshipPosting - Error : " + scholarshipPosting.Error.ToDetail());
                            _isLoadingButton = false;
                            IsLoadingButtonTitle = "";
                            StateHasChanged();
                            return;
                        }

                        var dataScholarshipPosting = scholarshipPosting.Result;

                        var postingStatus = new ScholarshipPostingStatus();
                        var awardeeQuota = scholarshipPosting is not null ? scholarshipPosting.Result!.AwardeeQuota : 0;
                        var awardeeSelection = _selectionDecreeByBatch.Where(x => x.ScholarshipPostingId == _scholarshipPostingId && statusSk.Contains(x.Status)).ToList().Count();

                        if (awardeeSelection >= awardeeQuota)
                        {
                            postingStatus = ScholarshipPostingStatus.Completed;
                        }
                        else
                        {
                            postingStatus = ScholarshipPostingStatus.Closed;
                        }

                        UpdateDataScholarshipPostingStatus.PostingId = _scholarshipPostingId;
                        UpdateDataScholarshipPostingStatus.PostingStatus = postingStatus;
                        var updatePostingStatus = await _scholarshipPostingService.UpdateScholarshipPostingStatusAsync(UpdateDataScholarshipPostingStatus);
                        if (updatePostingStatus.Error is not null)
                        {
                            _snackbar.AddError("Set ScholarshipPosting Status - Error : " + updatePostingStatus.Error.ToDetail());
                            _isLoadingButton = false;
                            IsLoadingButtonTitle = "";
                            StateHasChanged();
                            return;
                        }

                        var sendEmail = await _emailService.SendMailAsync(sendEmailRequest);
                        _snackbar.AddSuccess(sendEmail.Result!.Items.FirstOrDefault()!.Message!);
                    }
                }
            }
        }

        await SetDataOnLoadAsync();
        _isLoadingButton = false;
        StateHasChanged();
    }

    public static List<ScholarshipBatchResponse> GetEnumScholarshipBatch<T>()
    {
        var type = typeof(T);
        return Enum.GetValues(type)
                   .Cast<T>()
                   .Select(x => new ScholarshipBatchResponse
                   {
                       Value = x as ScholarshipBatch?,
                       Text = type.GetMember(x!.ToString()!)
                   .First()
                   .GetCustomAttribute<DisplayAttribute>()?.Name ?? x.ToString()
                   }).ToList();
    }
    #region Create draft
    private void OnClickCreateFinal()
    {
        if (_batch is 0 or null)
        {
            _snackbar.AddWarning("Please select batch");
            return;
        }

        _navigationManager.NavigateTo(RouteFor.AddByBatch(Convert.ToInt32(_batch), (int)_year), true);
    }

    public void OnClickCreateDraft()
    {
        if (_batch is 0 or null)
        {
            _snackbar.AddWarning("Please select batch");
            return;
        }

        _navigationManager.NavigateTo(RouteFor.EditByBatch(Convert.ToInt32(_batch), (int)_year), true);
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
    public void ExcelQueryCellInfoHandler(ExcelQueryCellInfoEventArgs<GetSelectionCreateSKQueryResponse> args)
    {
        var a = args.Column.Field;
        if (args.Column.Field == "Status")
        {
            args.Cell.Value = args.Data.Status.GetDescription().ToString();
        }
    }
}
