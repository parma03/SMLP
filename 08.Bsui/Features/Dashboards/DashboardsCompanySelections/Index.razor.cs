using System.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Psmp.Base.Scholarships.Enums;
using Psmp.Base.Selections.Enums;
using Psmp.Shared.Common.Extensions;
using Psmp.Shared.Common.Responses;
using Psmp.Shared.Masters.Queries;
using Psmp.Shared.Scholarships.ScholarshipPosting.Queries;
using Psmp.Shared.Selections.SelectionOverviews.Queries;

//namespace Psmp.Bsui.Features.Selections.Overview;
namespace Psmp.Bsui.Features.Dashboards.DashboardsCompanySelections;

public partial class Index
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

    protected ClaimsPrincipal _user = default!;

    protected List<DropdownItemResponse> _categories = new();
    protected List<GetCompanyQueryResponse> _companies = new();
    protected List<GetScholarshipPostingResponse> _scholarships = new();
    protected List<GetSelectionOverviewQueryResponse> _selectionOverviews = new();

    protected ErrorResponse? _error;
    protected bool _isLoading = false;
    protected bool _isLoadingData = false;
    protected Guid _scholarshipPostingId;
    protected string _companyCode = "0000";
    protected ScholarshipLocation _category = 0;
    protected int _countApplicant = 0;
    protected int _countApplicantPassed = 0;
    protected int _countApplicantRejected = 0;
    protected string _applicantSummary = "0";
    protected string _passedSummary = "0";
    protected string _rejectedSummary = "0";
    protected string _awaitingSummary = "0";
    protected string _greetings = default!;

    protected string? StatusColor { get; set; }

    protected override async Task OnInitializedAsync()
    {
        _user = (await AuthenticationStateTask).User;

        _isLoading = true;
        _greetings = $"Good {DateTimeOffset.Now.ToFriendlyTimeDisplayText()}";
        await SetDataOnLoadAsync();
        _isLoading = false;
    }

    private async Task SetDataOnLoadAsync()
    {
        _isLoadingData = true;
        var param = new GetSelectionOverviewRequest
        {
            CompanyCode = _companyCode,
            Category = _category,
            Batch = _scholarshipPostingId
        };
        _isLoadingData = false;
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

    public async Task OnChangeBatchAsync(Syncfusion.Blazor.DropDowns.ChangeEventArgs<Guid, GetScholarshipPostingResponse> args)
    {
        _scholarshipPostingId = args.ItemData.Id;

        await SetDataOnLoadAsync();
        StateHasChanged();
    }

    public async Task OnChangeCompanyAsync(Syncfusion.Blazor.DropDowns.ChangeEventArgs<string, GetCompanyQueryResponse> args)
    {
        _companyCode = args.ItemData.Code;

        await SetDataOnLoadAsync();
        StateHasChanged();
    }

    protected DataTable _table = new();
    protected List<GetSelectionOverviewQueryResponse> _workbookApplicants = new();

    protected void GenerateListFromTable(DataTable table)
    {
        if (table is not null)
        {
            foreach (DataRow row in table.Rows)
            {
                _workbookApplicants.Add(
                    new GetSelectionOverviewQueryResponse
                    {
                        Id = new Guid((string)row["Id"]),
                        StatusId = (int)Enum.Parse<ApplicantStatus>((string)row["Status"])
                    });
            }
        }
    }

    private DataTable DataTableExport()
    {
        var table = new DataTable();

        table.Columns.Add("Id", typeof(string));
        table.Columns.Add("Name", typeof(string));
        table.Columns.Add("Company_Code", typeof(string));
        table.Columns.Add("Employee_ID", typeof(string));
        table.Columns.Add("Email", typeof(string));
        table.Columns.Add("Status", typeof(string));

        if (_selectionOverviews is not null)
        {
            foreach (var item in _selectionOverviews)
            {
                table.Rows.Add(
                    item.Id,
                    item.Name,
                    item.CompanyCode,
                    item.EmployeeNumber,
                    item.Email,
                    item.StatusDescription);
            }
        }

        return table;
    }

    private readonly int _count1 = 20;
    private readonly int _count2 = 30;
    private readonly int _count3 = 45;
    private readonly string _judul1 = "Total Applicants";
    private readonly string _judul2 = "Company Selection";
    private readonly string _judul3 = "Awaiting University";
    private readonly string _judul4 = "LoA Issued";
    private readonly string _judul5 = "Study Period";
    private readonly string _judul6 = "Graduated";
    private readonly string _judultask1 = "Payment";
    private readonly string _judultask2 = "University Registration";
    private readonly string _valuetask1 = "Tuition fee request from john";
    private readonly string _valuetask2 = "Waiting for approvel from john";
    private readonly string _datetask = "MM/dd/yyyy HH:mm";

    private readonly int _index = -1; //default value cannot be 0 -> first selectedindex is 0.
    private readonly int _dataSize = 4;
    private readonly double[] _data = { 77, 25, 20, 5 };
    private readonly string[] _labels = { "Company Selection", "Awaiting University", "LoA Issued", "Studi Period", "Graduated" };
    private readonly Random _random = new();
}
