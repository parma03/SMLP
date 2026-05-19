using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using System.Security.Claims;
using Smlp.Shared.Common.Responses;
using Smlp.Base.Scholarships.Enums;
using Smlp.Shared.Masters.Queries;
using Smlp.Shared.Scholarships.ScholarshipPosting.Queries;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Smlp.Shared.Common.Extensions;
using Syncfusion.Blazor.Navigations;
using Smlp.Shared.Selections.BeritaAcara.Queries;
using RouteFor = Smlp.Bsui.Features.Selections.BeritaAcara.Constants.RouteFor;
using Syncfusion.Blazor.Grids;
using SelectionType = Smlp.Base.Selections.Enums.SelectionType;
using Smlp.Bsui.Common.Extensions;

namespace Smlp.Bsui.Features.Selections.BeritaAcara;

public partial class Index
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

    protected ClaimsPrincipal _user = default!;
    [Parameter]
    public int Type { get; set; }
    protected ErrorResponse? _error;
    protected bool _isLoading = false;
    protected bool _isLoadingDialog = false;
    protected bool _isLoadingData = false;
    protected string _applicantSummary = "0";
    protected string _completeSummary = "0";
    protected string _waitingSummary = "0";
    protected string _rejectedSummary = "0";
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
    protected int? _selectionBatch = null;

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

    protected List<GetCompanyQueryResponse> _dropdownCompaniesAdd = new();
    protected List<GetSelectionApplicantBeritaAcaraQueryResponse> _selectionApplicantBeritaAcara = new();
    private SfGrid<GetSelectionApplicantBeritaAcaraQueryResponse>? Grid { get; set; }
    protected List<GetSelectionApplicantBeritaAcaraByBatchQueryResponse> _selectionApplicantBeritaAcaraByBatch = new();
    protected List<GetSelectionBeritaAcaraByBatchQueryResponse> _selectionBeritaAcara = new();

    protected override async Task OnInitializedAsync()
    {
        _user = (await AuthenticationStateTask).User;
        //_companyCode = _user.GetCompanyCode();
        _isLoading = true;
        await GetDropdownCompanyGroup();
        await GetDropdownCompany();
        //await GetListCompany();
        await GetDropdownCategories();
        await GetDropdownCompanyGroupApplicantFilter();
        await GetDropdownCompanyApplicantFilter();
        await GetDropdownCodes();
        await SetDataOnLoadAsync();
        await SetDataOnLoadBeritaAcaraApplicantAsync();
        _isLoading = false;

    }
    private async Task SetDataOnLoadBeritaAcaraApplicantAsync()
    {
        _isLoadingData = true;
        var param = new GetSelectionBeritaAcaraFilterRequest
        {
            CompanyCode = _companyCode,
            Category = _category,
            SelectionBatch = _selectionBatch,
            CompanyGroupId = _companyGroupId,
            DirectorateId = _directorateId,
            Code = _codeId,
            SelectionType = (SelectionType)Type,
            CompanyGroupIdApplicantFilter = _companyGroupIdApplicantFilter,
            CompanyCodeApplicantFilter = _companyCodeApplicantFilter,
            Year = _year,
            AdminCompany = _user.GetCompanyCode(),
        };

        var response = await _selectionBeritaAcaraService.GetSelectionApplicantBeritaListAsync(param);

        if (response.Error is null)
        {
            _selectionApplicantBeritaAcara = response.Result!.Items.ToList();
        }
        else
        {
            _error = response.Error;
            _isLoadingData = false;
        }

        _isLoadingData = false;
    }
    private async Task SetDataOnLoadAsync()
    {
        _isLoadingData = true;
        var param = new GetSelectionBeritaAcaraFilterByBatchRequest
        {
            SelectionBatch = Convert.ToInt32(_selectionBatch),
            Year = _year
        };

        var response = await _selectionBeritaAcaraService.GetSelectionBeritaAcaraByBatchAsync(param);

        if (response.Error is null)
        {
            _selectionBeritaAcara = response.Result!.Items.ToList();
            _applicantSummary = _selectionBeritaAcara.Count().ToString();
            _waitingSummary = _selectionBeritaAcara.Where(x => x.Status == Base.Selections.Enums.BeritaAcaraStatus.Waiting).ToList().Count().ToString() + "/" + _applicantSummary;
            _completeSummary = _selectionBeritaAcara.Where(x => x.Status == Base.Selections.Enums.BeritaAcaraStatus.Completed).ToList().Count().ToString() + "/" + _applicantSummary;
            _rejectedSummary = _selectionBeritaAcara.Where(x => x.Status == Base.Selections.Enums.BeritaAcaraStatus.Rejected).ToList().Count().ToString() + "/" + _applicantSummary;
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
        await SetDataOnLoadBeritaAcaraApplicantAsync();
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

        await SetDataOnLoadBeritaAcaraApplicantAsync();
        StateHasChanged();
    }
    public async Task OnChangeBatchAsync(Syncfusion.Blazor.DropDowns.ChangeEventArgs<int?, DropdownItemResponse> args)
    {
        _selectionBatch = args.ItemData.Id;

        await SetDataOnLoadBeritaAcaraApplicantAsync();
        await SetDataOnLoadAsync();
        StateHasChanged();
    }
    public async Task OnChangeCompanyAsync(Syncfusion.Blazor.DropDowns.ChangeEventArgs<string?, DropdownItemStringResponse> args)
    {
        _companyCode = args.ItemData.Id;
        _scholarshipPostingId = null;
        _codeId = null;
        _year = null;
        _selectionBatch = null;
        await GetDropdownCodes();
        await GetDropdownDirectorate();
        await SetDataOnLoadBeritaAcaraApplicantAsync();
        StateHasChanged();
    }
    public async Task OnChangeDirectorateAsync(Syncfusion.Blazor.DropDowns.ChangeEventArgs<int, GetDirectorateQueryResponse> args)
    {
        _directorateId = args.ItemData.Id;

        await SetDataOnLoadBeritaAcaraApplicantAsync();
        StateHasChanged();
    }
    public async Task OnChangeCodeAsync(Syncfusion.Blazor.DropDowns.ChangeEventArgs<Guid?, DropdownItemGuidResponse> args)
    {
        if (args.ItemData == null)
        {
            _codeId = string.Empty;
            _scholarshipPostingId = Guid.Empty;
        }
        else
        {
            _codeId = args.ItemData.Code;
            _scholarshipPostingId = args.ItemData.Id;
        }

        _year = null;
        _selectionBatch = null;
        await GetDropdownCodes();
        await SetDataOnLoadBeritaAcaraApplicantAsync();
        StateHasChanged();
    }
    public async Task OnChangeCompanyGroupAsync(Syncfusion.Blazor.DropDowns.ChangeEventArgs<int?, DropdownItemResponse> args)
    {
        _companyGroupId = args.ItemData.Id;
        _companyCode = null;
        _scholarshipPostingId = null;
        _codeId = null;
        _year = null;
        _selectionBatch = null;
        await GetDropdownCodes();
        await GetDropdownCompany();
        await SetDataOnLoadBeritaAcaraApplicantAsync();
        StateHasChanged();
    }
    #endregion

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
    public async Task OnCommandClicked(CommandClickEventArgs<GetSelectionApplicantBeritaAcaraQueryResponse> args)
    {
        if (args.CommandColumn.Type == CommandButtonType.Edit)
        {
            _navigationManager.NavigateTo(RouteFor.Details(args.RowData.TalentReviewId, Convert.ToInt32(args.RowData.SelectionType)));
            await Task.CompletedTask;
        }
    }

    private async void OnClickAddBeritaAcaraAsync()
    {
        var param = new GetSelectionBeritaAcaraFilterByBatchRequest
        {
            SelectionBatch = Convert.ToInt32(_selectionBatch),
            Year = _year,
        };

        var response = await _selectionBeritaAcaraService.GetSelectionApplicantBeritaAcaraByBatchAsync(param);

        if (response.Error is null)
        {
            _selectionApplicantBeritaAcaraByBatch = response.Result!.Items.ToList();
        }

        if (_selectionBatch is null or 0)
        {
            _snackbar.AddWarning("Please select batch");
            return;
        }

        if (_selectionApplicantBeritaAcaraByBatch.Where(x => x.Batch == Convert.ToInt32(_selectionBatch) && x.Status == Base.Selections.Enums.ApplicantStatus.Passed).ToList().Count() == 0)
        {
            _snackbar.AddWarning("Please approve applicant before Create Draf");
            return;
        }

        var actionDraft = "Add";
        if (_selectionBeritaAcara.Count() > 0)
        {
            actionDraft = "Edit";
        }

        _navigationManager.NavigateTo(RouteFor.AddByBatch(actionDraft, Convert.ToInt32(_selectionBatch), (int)_year), true);
    }

    #region Grid Item Berita Acara
    public async Task OnCommandClickedBeritaAcara(CommandClickEventArgs<GetSelectionBeritaAcaraByBatchQueryResponse> args)
    {
        if (args.CommandColumn.Type == CommandButtonType.Edit)
        {
            _navigationManager.NavigateTo(RouteFor.BeritaAcaraFinal(args.RowData.Batch, args.RowData.Year), true);
            await Task.CompletedTask;
        }
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
    public void ExcelQueryCellInfoHandler(ExcelQueryCellInfoEventArgs<GetSelectionApplicantBeritaAcaraQueryResponse> args)
    {
        var a = args.Column.Field;
        if (args.Column.Field == "Status")
        {
            args.Cell.Value = args.Data.Status.GetDescription();
        }
    }
}
