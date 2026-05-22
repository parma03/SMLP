using Microsoft.AspNetCore.Components;
using MudBlazor;
using Smlp.Base.Customer.Enums;
using Smlp.Base.Transaction.Enums;
using Smlp.Bsui.Common.Constants;
using Smlp.Bsui.Common.Extensions;
using Smlp.Shared.Master.Customers.Queries;
using Smlp.Shared.Transaction.RedeemHistories.Queries;
using Smlp.Shared.Transaction.Transactions.Queries;

namespace Smlp.Bsui.Features.Customers;

public partial class Detail
{
    [Parameter] public Guid Id { get; set; }

    private List<BreadcrumbItem> _breadcrumbItems = new();
    private bool _isLoading = true;
    private bool _loadingTransactions = false;
    private bool _loadingRedeems = false;
    private bool _loadingPoints = false;

    private GetCustomerResponse? _customer;
    private List<GetTransactionResponse> _transactions = new();
    private List<GetRedeemHistoryResponse> _redeems = new();
    private List<PointHistoryItem> _pointHistory = new();
    private string? _photoUrl = null;

    protected override async Task OnInitializedAsync()
    {
        SetupBreadcrumb();
        await LoadCustomerAsync();
    }

    protected override async Task OnParametersSetAsync()
    {
        if (_customer is null || _customer.Id != Id)
        {
            await LoadCustomerAsync();
        }
    }

    private void SetupBreadcrumb()
    {
        _breadcrumbItems = new()
        {
            CommonBreadcrumbFor.Home,
            new("Pelanggan", href: CommonRouteFor.Customers),
            CommonBreadcrumbFor.Active("Detail Pelanggan"),
        };
    }

    private async Task LoadCustomerAsync()
    {
        _isLoading = true;
        try
        {
            var result = await _customerService.GetCustomerByIdAsync(Id);
            if (result.Error is not null)
            {
                _snackbar.AddError(result.Error.ToDetail());
                return;
            }

            _customer = result.Result;
            SetupBreadcrumb();

            // Load tabs data in parallel
            await Task.WhenAll(
                LoadTransactionsAsync(),
                LoadRedeemsAsync()
            );
        }
        catch (Exception ex)
        {
            _snackbar.AddError($"Gagal memuat data pelanggan: {ex.Message}");
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async Task LoadTransactionsAsync()
    {
        _loadingTransactions = true;
        try
        {
            var result = await _transactionService.GetTransactionsAsync(new GetTransactionRequest
            {
                CustomerId = Id,
                PageSize = 50
            });
            if (result.Error is null)
            {
                _transactions = result.Result!.Items.ToList();
                BuildPointHistory();
            }
        }
        finally
        {
            _loadingTransactions = false;
        }
    }

    private async Task LoadRedeemsAsync()
    {
        _loadingRedeems = true;
        try
        {
            var result = await _redeemService.GetRedeemHistoriesAsync(new GetRedeemHistoryRequest
            {
                CustomerId = Id,
                PageSize = 50
            });
            if (result.Error is null)
            {
                _redeems = result.Result!.Items.ToList();
                BuildPointHistory();
            }
        }
        finally
        {
            _loadingRedeems = false;
        }
    }

    private void BuildPointHistory()
    {
        var items = new List<PointHistoryItem>();
        var running = 0;

        // Combine transactions and redeems sorted by date
        var txItems = _transactions.Select(t => new PointHistoryItem
        {
            Date = t.TransactionDate,
            Description = $"Transaksi #{t.TransactionCode}",
            Points = t.PointsEarned + t.BonusPoints,
        });
        var rdItems = _redeems.Select(r => new PointHistoryItem
        {
            Date = r.RedeemDate,
            Description = $"Redeem: {r.RewardName}",
            Points = -r.PointsUsed,
        });

        var merged = txItems.Concat(rdItems).OrderBy(x => x.Date).ToList();
        foreach (var item in merged)
        {
            running += item.Points;
            item.Balance = running;
            items.Add(item);
        }

        _pointHistory = items.OrderByDescending(x => x.Date).ToList();
    }

    private Color GetTierColor(CustomerTier tier) => tier switch
    {
        CustomerTier.Gold => Color.Warning,
        CustomerTier.Platinum => Color.Primary,
        CustomerTier.Silver => Color.Default,
        _ => Color.Default
    };

    private string GetTierEmoji(CustomerTier tier) => tier switch
    {
        CustomerTier.Bronze => "🥉",
        CustomerTier.Silver => "🥈",
        CustomerTier.Gold => "🥇",
        CustomerTier.Platinum => "💎",
        _ => ""
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

    private Color GetRedeemStatusColor(RedeemStatus status) => status switch
    {
        RedeemStatus.Pending => Color.Warning,
        RedeemStatus.Verified => Color.Success,
        RedeemStatus.Cancelled => Color.Error,
        _ => Color.Default
    };

    private string GetRedeemStatusLabel(RedeemStatus status) => status switch
    {
        RedeemStatus.Pending => "Menunggu",
        RedeemStatus.Verified => "Terverifikasi",
        RedeemStatus.Cancelled => "Dibatalkan",
        _ => status.ToString()
    };

    // Returns (nextTierName, pointsNeeded, progressPercent)
    private (string, int, double) GetTierProgress(int points, CustomerTier tier)
    {
        return tier switch
        {
            CustomerTier.Bronze => ("Silver", Math.Max(0, 100 - points), Math.Min(100, points)),
            CustomerTier.Silver => ("Gold", Math.Max(0, 500 - points), Math.Min(100, (points - 100.0) / 4.0)),
            CustomerTier.Gold => ("Platinum", Math.Max(0, 1000 - points), Math.Min(100, (points - 500.0) / 5.0)),
            CustomerTier.Platinum => ("Platinum", 0, 100),
            _ => ("-", 0, 0)
        };
    }

    public class PointHistoryItem
    {
        public DateTimeOffset Date { get; set; }
        public string Description { get; set; } = string.Empty;
        public int Points { get; set; }
        public int Balance { get; set; }
    }
}
