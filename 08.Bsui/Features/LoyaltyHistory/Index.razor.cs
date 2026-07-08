using MudBlazor;
using Smlp.Base.Transaction.Enums;
using Smlp.Bsui.Common.Extensions;
using Smlp.Shared.Transaction.LoyaltyHistories.Queries;
using Smlp.Shared.Transaction.PointAdjustments.Queries;
using Smlp.Shared.Transaction.RedeemHistories.Queries;
using Smlp.Shared.Transaction.Transactions.Queries;

namespace Smlp.Bsui.Features.LoyaltyHistory;

public partial class Index
{
    private string _searchText = string.Empty;
    private DateTime? _dateFrom;
    private DateTime? _dateTo;
    private string _filterType = "all";
    private bool _loading = false;

    private List<LoyaltyActivityItem> _allItems = new();
    private List<LoyaltyActivityItem> _filteredItems = new();

    private int TotalEarned => _filteredItems.Where(i => i.Points > 0).Sum(i => i.Points);
    private int TotalUsed => Math.Abs(_filteredItems.Where(i => i.Points < 0).Sum(i => i.Points));

    protected override async Task OnInitializedAsync()
    {
        _dateFrom = DateTime.Today.AddDays(-30);
        _dateTo = DateTime.Today;
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        _loading = true;
        _allItems.Clear();
        try
        {
            // Load transactions (point earnings)
            var txResult = await _transactionService.GetTransactionsAsync(new GetTransactionRequest
            {
                DateFrom = _dateFrom.HasValue ? new DateTimeOffset(_dateFrom.Value) : null,
                DateTo = _dateTo.HasValue ? new DateTimeOffset(_dateTo.Value.AddDays(1)) : null,
                Search = string.IsNullOrWhiteSpace(_searchText) ? null : _searchText,
                PageSize = 500,
            });
            if (txResult.Result?.Items is not null)
            {
                foreach (var tx in txResult.Result.Items)
                {
                    if (tx.Status == TransactionStatus.Cancelled)
                        continue;
                    var earnedPts = tx.PointsEarned + tx.BonusPoints;
                    _allItems.Add(new LoyaltyActivityItem
                    {
                        Date = tx.TransactionDate,
                        CustomerName = tx.CustomerName,
                        CustomerMemberCode = tx.CustomerMemberCode,
                        ActivityType = "transaction",
                        Description = $"Transaksi – Rp {tx.TotalAmount:N0}",
                        Reference = tx.TransactionCode,
                        Points = earnedPts,
                        Status = GetStatusLabel(tx.Status),
                    });
                }
            }

            // Load redeems (point usage)
            var redeemResult = await _redeemService.GetRedeemHistoriesAsync(new GetRedeemHistoryRequest
            {
                DateFrom = _dateFrom.HasValue ? new DateTimeOffset(_dateFrom.Value) : null,
                DateTo = _dateTo.HasValue ? new DateTimeOffset(_dateTo.Value.AddDays(1)) : null,
                Search = string.IsNullOrWhiteSpace(_searchText) ? null : _searchText,
                PageSize = 500,
            });
            if (redeemResult.Result?.Items is not null)
            {
                foreach (var r in redeemResult.Result.Items)
                {
                    if (r.Status == RedeemStatus.Rejected)
                        continue;
                    _allItems.Add(new LoyaltyActivityItem
                    {
                        Date = r.RedeemDate,
                        CustomerName = r.CustomerName,
                        CustomerMemberCode = r.CustomerMemberCode,
                        ActivityType = "redeem",
                        Description = $"Tukar Reward: {r.RewardName}",
                        Reference = r.RedeemCode,
                        Points = -r.PointsUsed,
                        Status = GetRedeemStatusLabel(r.Status),
                    });
                }
            }

            // Load point adjustments
            var adjResult = await _pointAdjustmentService.GetAdjustmentsAsync(new GetPointAdjustmentRequest
            {
                DateFrom = _dateFrom.HasValue ? new DateTimeOffset(_dateFrom.Value) : null,
                DateTo = _dateTo.HasValue ? new DateTimeOffset(_dateTo.Value.AddDays(1)) : null,
                PageSize = 500,
            });
            if (adjResult.Result?.Items is not null)
            {
                foreach (var adj in adjResult.Result.Items)
                {
                    var isDeduction = adj.AdjustmentType is AdjustmentType.Penalty
                        or AdjustmentType.Expiry
                        or AdjustmentType.Reversal;
                    var pts = isDeduction ? -Math.Abs(adj.Points) : Math.Abs(adj.Points);
                    _allItems.Add(new LoyaltyActivityItem
                    {
                        Date = adj.AdjustmentDate,
                        CustomerName = adj.CustomerName,
                        CustomerMemberCode = adj.CustomerMemberCode,
                        ActivityType = "adjustment",
                        Description = $"Penyesuaian: {adj.Reason}",
                        Reference = $"Oleh: {adj.ApprovedBy}",
                        Points = pts,
                        Status = isDeduction ? "Pengurangan" : "Penambahan",
                    });
                }
            }

            _allItems = _allItems.OrderByDescending(i => i.Date).ToList();
            ApplyFilter();
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

    private void ApplyFilter()
    {
        _filteredItems = _filterType == "all"
            ? _allItems
            : _allItems.Where(i => i.ActivityType == _filterType).ToList();
    }

    private async Task OnSearchChanged()
    {
        await LoadDataAsync();
    }

    private Color GetTypeColor(string type) => type switch
    {
        "transaction" => Color.Success,
        "redeem" => Color.Warning,
        "adjustment" => Color.Info,
        _ => Color.Default
    };

    private string GetTypeLabel(string type) => type switch
    {
        "transaction" => "🛍️ Transaksi",
        "redeem" => "🎁 Penukaran",
        "adjustment" => "⚙️ Penyesuaian",
        _ => type
    };

    private Color GetStatusColor(string status) => status switch
    {
        "Selesai" or "Diverifikasi" or "Penambahan" => Color.Success,
        "Menunggu" => Color.Warning,
        "Pengurangan" => Color.Error,
        _ => Color.Default
    };

    private static string GetStatusLabel(TransactionStatus status) => status switch
    {
        TransactionStatus.Completed => "Selesai",
        TransactionStatus.Cancelled => "Dibatalkan",
        _ => status.ToString()
    };

    private static string GetRedeemStatusLabel(RedeemStatus status) => status switch
    {
        RedeemStatus.Pending => "Menunggu",
        RedeemStatus.Verified => "Diverifikasi",
        RedeemStatus.Completed => "Selesai",
        RedeemStatus.Rejected => "Ditolak",
        _ => status.ToString()
    };
}
