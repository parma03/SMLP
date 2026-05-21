using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using Smlp.Base.Transaction.Enums;
using Smlp.Bsui.Common.Constants;
using Smlp.Bsui.Common.Extensions;
using Smlp.Shared.Transaction.Transactions.Queries;

namespace Smlp.Bsui.Features.Transactions;

public partial class Detail
{
    [Parameter] public Guid Id { get; set; }

    private List<BreadcrumbItem> _breadcrumbItems = new();
    private bool _isLoading = true;
    private bool _showReceipt = false;
    private bool _showCancelDialog = false;
    private bool _isCancelling = false;

    private GetTransactionResponse? _transaction;

    protected override async Task OnInitializedAsync()
    {
        SetupBreadcrumb();
        await LoadTransactionAsync();
    }

    protected override async Task OnParametersSetAsync()
    {
        if (_transaction is null || _transaction.Id != Id)
        {
            await LoadTransactionAsync();
        }
    }

    private void SetupBreadcrumb()
    {
        _breadcrumbItems = new()
        {
            CommonBreadcrumbFor.Home,
            new("Transaksi", href: CommonRouteFor.Transactions),
            CommonBreadcrumbFor.Active("Detail Transaksi"),
        };
    }

    private async Task LoadTransactionAsync()
    {
        _isLoading = true;
        try
        {
            var result = await _transactionService.GetTransactionByIdAsync(Id);
            if (result.Error is not null)
            {
                _snackbar.AddError(result.Error.ToDetail());
                return;
            }

            _transaction = result.Result;
        }
        catch (Exception ex)
        {
            _snackbar.AddError($"Gagal memuat data transaksi: {ex.Message}");
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void PrintReceipt()
    {
        _showReceipt = !_showReceipt;
    }

    private async Task DoPrint()
    {
        await _jsRuntime.InvokeVoidAsync("window.print");
    }

    private void ConfirmCancelTransaction()
    {
        _showCancelDialog = true;
    }

    private async Task CancelTransactionAsync()
    {
        _isCancelling = true;
        try
        {
            var result = await _transactionService.CancelTransactionAsync(Id);
            if (result.Error is not null)
            {
                _snackbar.AddError(result.Error.ToDetail());
                return;
            }

            _snackbar.AddSuccess("Transaksi berhasil dibatalkan.");
            _showCancelDialog = false;
            await LoadTransactionAsync();
        }
        catch (Exception ex)
        {
            _snackbar.AddError($"Gagal membatalkan transaksi: {ex.Message}");
        }
        finally
        {
            _isCancelling = false;
        }
    }

    private Color GetStatusColor(TransactionStatus status) => status switch
    {
        TransactionStatus.Completed => Color.Success,
        TransactionStatus.Cancelled => Color.Error,
        TransactionStatus.Pending => Color.Warning,
        _ => Color.Default
    };

    private string GetStatusLabel(TransactionStatus status) => status switch
    {
        TransactionStatus.Completed => "Selesai",
        TransactionStatus.Cancelled => "Dibatalkan",
        TransactionStatus.Pending => "Menunggu",
        _ => status.ToString()
    };

    private string GetPaymentMethodLabel(PaymentMethod method) => method switch
    {
        PaymentMethod.Tunai => "💵 Tunai",
        PaymentMethod.Qris => "📱 QRIS",
        PaymentMethod.TransferBank => "🏦 Transfer Bank",
        PaymentMethod.EWallet => "💳 E-Wallet",
        PaymentMethod.Debit => "💳 Kartu Debit",
        PaymentMethod.Kredit => "💳 Kartu Kredit",
        _ => method.ToString()
    };
}
