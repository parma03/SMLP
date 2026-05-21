using MudBlazor;
using Smlp.Base.Customer.Enums;
using Smlp.Bsui.Common.Extensions;
using Smlp.Shared.Master.Customers.Queries;

namespace Smlp.Bsui.Features.Customers;

public partial class Index
{
    private string _searchText = string.Empty;
    private CustomerTier? _filterTier;
    private CustomerSegment? _filterSegment;
    private bool _loading = false;
    private int _totalCount = 0;
    private int _pageNumber = 1;
    private readonly int _pageSize = 10;

    private List<GetCustomerResponse> _customers = new();

    protected override async Task OnInitializedAsync()
    {
        await LoadCustomersAsync();
    }

    private async Task LoadCustomersAsync()
    {
        _loading = true;
        try
        {
            var request = new GetCustomerRequest
            {
                Search = string.IsNullOrWhiteSpace(_searchText) ? null : _searchText,
                Tier = _filterTier,
                Segment = _filterSegment,
                PageNumber = _pageNumber,
                PageSize = _pageSize,
            };

            var result = await _customerService.GetCustomersAsync(request);
            if (result.Error is not null)
            {
                _snackbar.AddError(result.Error.ToDetail());
                return;
            }

            _customers = result.Result!.Items.ToList();
            _totalCount = result.Result.TotalCount;
        }
        catch (Exception ex)
        {
            _snackbar.AddError($"Gagal memuat data: {ex.Message}");
        }
        finally
        {
            _loading = false;
        }
    }

    private async Task OnSearchChanged(string value)
    {
        _searchText = value;
        _pageNumber = 1;
        await LoadCustomersAsync();
    }

    private async Task OnFilterTierChanged(CustomerTier? value)
    {
        _filterTier = value;
        _pageNumber = 1;
        await LoadCustomersAsync();
    }

    private async Task OnFilterSegmentChanged(CustomerSegment? value)
    {
        _filterSegment = value;
        _pageNumber = 1;
        await LoadCustomersAsync();
    }

    private async Task OnDeleteAsync(GetCustomerResponse customer)
    {
        var result = await _customerService.DeleteCustomerAsync(customer.Id);
        if (result.Error is not null)
        {
            _snackbar.AddError(result.Error.ToDetail());
            return;
        }

        _snackbar.AddSuccess($"Pelanggan {customer.FullName} berhasil dihapus.");
        await LoadCustomersAsync();
    }

    private Color GetTierColor(CustomerTier tier) => tier switch
    {
        CustomerTier.Gold => Color.Warning,
        CustomerTier.Silver => Color.Default,
        CustomerTier.Platinum => Color.Primary,
        _ => Color.Default
    };

    private string GetTierLabel(CustomerTier tier) => tier switch
    {
        CustomerTier.Bronze => "Bronze",
        CustomerTier.Silver => "Silver",
        CustomerTier.Gold => "Gold",
        CustomerTier.Platinum => "Platinum",
        _ => tier.ToString()
    };

    private Color GetSegmentColor(CustomerSegment segment) => segment switch
    {
        CustomerSegment.Loyal => Color.Success,
        CustomerSegment.Active => Color.Info,
        CustomerSegment.AtRisk => Color.Warning,
        CustomerSegment.Inactive => Color.Error,
        _ => Color.Default
    };

    private string GetSegmentLabel(CustomerSegment segment) => segment switch
    {
        CustomerSegment.New => "Baru",
        CustomerSegment.Active => "Aktif",
        CustomerSegment.Loyal => "Setia",
        CustomerSegment.AtRisk => "Berisiko",
        CustomerSegment.Inactive => "Tidak Aktif",
        _ => segment.ToString()
    };
}
