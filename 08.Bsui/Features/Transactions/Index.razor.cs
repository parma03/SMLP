using MudBlazor;
using Smlp.Base.Transaction.Enums;
using Smlp.Bsui.Common.Extensions;
using Smlp.Shared.Transaction.Transactions.Queries;

namespace Smlp.Bsui.Features.Transactions;

public partial class Index
{
    private string _searchText = string.Empty;
    private DateTime? _dateFrom;
    private DateTime? _dateTo;
    private bool _loading = false;
    private int _totalCount = 0;
    private int _pageNumber = 1;
    private readonly int _pageSize = 10;

    private List<GetTransactionResponse> _transactions = new();

    protected override async Task OnInitializedAsync()
    {
        _dateFrom = DateTime.Today.AddDays(-30);
        _dateTo = DateTime.Today;
        await LoadTransactionsAsync();
    }

    private async Task LoadTransactionsAsync()
    {
        _loading = true;
        try
        {
            var request = new GetTransactionRequest
            {
                Search = string.IsNullOrWhiteSpace(_searchText) ? null : _searchText,
                DateFrom = _dateFrom.HasValue ? new DateTimeOffset(_dateFrom.Value) : null,
                DateTo = _dateTo.HasValue ? new DateTimeOffset(_dateTo.Value.AddDays(1)) : null,
                PageNumber = _pageNumber,
                PageSize = _pageSize,
            };

            var result = await _transactionService.GetTransactionsAsync(request);
            if (result.Error is not null)
            {
                _snackbar.AddError(result.Error.ToDetail());
                return;
            }

            _transactions = result.Result!.Items.ToList();
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

    private async Task OnSearchChanged()
    {
        _pageNumber = 1;
        await LoadTransactionsAsync();
    }

    private string GetPaymentMethodLabel(PaymentMethod method) => method switch
    {
        PaymentMethod.Tunai => "Tunai",
        PaymentMethod.Qris => "QRIS",
        PaymentMethod.TransferBank => "Transfer",
        PaymentMethod.EWallet => "E-Wallet",
        PaymentMethod.Debit => "Debit",
        PaymentMethod.Kredit => "Kredit",
        _ => method.ToString()
    };

    private Color GetStatusColor(TransactionStatus status) => status switch
    {
        TransactionStatus.Completed => Color.Success,
        TransactionStatus.Pending => Color.Warning,
        TransactionStatus.Cancelled => Color.Error,
        _ => Color.Default
    };

    private string GetStatusLabel(TransactionStatus status) => status switch
    {
        TransactionStatus.Completed => "Selesai",
        TransactionStatus.Pending => "Proses",
        TransactionStatus.Cancelled => "Dibatalkan",
        _ => status.ToString()
    };
}
