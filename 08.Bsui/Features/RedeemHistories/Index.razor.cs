using MudBlazor;
using Smlp.Base.Transaction.Enums;
using Smlp.Bsui.Common.Extensions;
using Smlp.Shared.Transaction.RedeemHistories.Commands.Verify;
using Smlp.Shared.Transaction.RedeemHistories.Queries;

namespace Smlp.Bsui.Features.RedeemHistories;

public partial class Index
{
    private string _searchText = string.Empty;
    private DateTime? _dateFrom;
    private DateTime? _dateTo;
    private RedeemStatus? _filterStatus;
    private bool _loading = false;
    private readonly int _pageNumber = 1;
    private readonly int _pageSize = 10;
    private int _totalCount = 0;

    private List<GetRedeemHistoryResponse> _redeems = new();

    protected override async Task OnInitializedAsync()
    {
        _dateFrom = DateTime.Today.AddDays(-30);
        _dateTo = DateTime.Today;
        await LoadRedeemsAsync();
    }

    private async Task LoadRedeemsAsync()
    {
        _loading = true;
        try
        {
            var request = new GetRedeemHistoryRequest
            {
                Search = string.IsNullOrWhiteSpace(_searchText) ? null : _searchText,
                Status = _filterStatus,
                DateFrom = _dateFrom.HasValue ? new DateTimeOffset(_dateFrom.Value) : null,
                DateTo = _dateTo.HasValue ? new DateTimeOffset(_dateTo.Value.AddDays(1)) : null,
                PageNumber = _pageNumber,
                PageSize = _pageSize
            };

            var result = await _redeemService.GetRedeemHistoriesAsync(request);
            if (result.Error is not null)
            {
                _snackbar.AddError(result.Error.ToDetail());
                return;
            }

            _redeems = result.Result!.Items.ToList();
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

    private async Task VerifyRedeemAsync(GetRedeemHistoryResponse redeem)
    {
        var request = new VerifyRedeemRequest { Status = RedeemStatus.Verified };
        var result = await _redeemService.VerifyRedeemAsync(redeem.Id, request);
        if (result.Error is not null)
        {
            _snackbar.AddError(result.Error.ToDetail());
            return;
        }

        _snackbar.AddSuccess($"Redeem {redeem.RedeemCode} berhasil diverifikasi.");
        await LoadRedeemsAsync();
    }

    private async Task RejectRedeemAsync(GetRedeemHistoryResponse redeem)
    {
        var request = new VerifyRedeemRequest { Status = RedeemStatus.Rejected };
        var result = await _redeemService.VerifyRedeemAsync(redeem.Id, request);
        if (result.Error is not null)
        {
            _snackbar.AddError(result.Error.ToDetail());
            return;
        }

        _snackbar.AddSuccess($"Redeem {redeem.RedeemCode} berhasil ditolak.");
        await LoadRedeemsAsync();
    }

    private Color GetStatusColor(RedeemStatus status) => status switch
    {
        RedeemStatus.Pending => Color.Warning,
        RedeemStatus.Verified => Color.Info,
        RedeemStatus.Completed => Color.Success,
        RedeemStatus.Rejected => Color.Error,
        RedeemStatus.Expired => Color.Default,
        RedeemStatus.Cancelled => Color.Default,
        _ => Color.Default
    };

    private string GetStatusLabel(RedeemStatus status) => status switch
    {
        RedeemStatus.Pending => "Menunggu",
        RedeemStatus.Verified => "Diverifikasi",
        RedeemStatus.Completed => "Selesai",
        RedeemStatus.Rejected => "Ditolak",
        RedeemStatus.Expired => "Kadaluarsa",
        RedeemStatus.Cancelled => "Dibatalkan",
        _ => status.ToString()
    };
}
