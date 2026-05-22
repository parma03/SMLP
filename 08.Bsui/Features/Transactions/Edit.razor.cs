using Microsoft.AspNetCore.Components;
using MudBlazor;
using Smlp.Base.Transaction.Enums;
using Smlp.Bsui.Common.Constants;
using Smlp.Bsui.Common.Extensions;
using Smlp.Shared.Transaction.Transactions.Commands.Create;
using Smlp.Shared.Transaction.Transactions.Commands.Update;
using Smlp.Shared.Transaction.Transactions.Queries;

namespace Smlp.Bsui.Features.Transactions;

public partial class Edit
{
    [Parameter] public Guid Id { get; set; }

    private List<BreadcrumbItem> _breadcrumbItems = new();
    private bool _isLoading = true;
    private bool _isSaving = false;

    private GetTransactionResponse? _transaction;

    // Form fields
    private PaymentMethod _paymentMethod = PaymentMethod.Tunai;
    private string? _notes;
    private decimal _discountAmount = 0;
    private readonly List<CreateTransactionItemRequest> _items = new();

    private decimal Subtotal => _items.Sum(i => i.UnitPrice * i.Quantity);
    private decimal GrandTotal => Math.Max(0, Subtotal - _discountAmount);

    protected override async Task OnInitializedAsync()
    {
        SetupBreadcrumb();
        await LoadTransactionAsync();
    }

    protected override async Task OnParametersSetAsync()
    {
        if (_transaction is null || _transaction.Id != Id)
            await LoadTransactionAsync();
    }

    private void SetupBreadcrumb()
    {
        _breadcrumbItems = new()
        {
            CommonBreadcrumbFor.Home,
            new("Transaksi", href: CommonRouteFor.Transactions),
            new("Detail", href: CommonRouteFor.TransactionDetail(Id)),
            CommonBreadcrumbFor.Active("Edit Transaksi"),
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
            if (_transaction is not null)
            {
                _paymentMethod = _transaction.PaymentMethod;
                _notes = _transaction.Notes;
                _items.Clear();
                foreach (var item in _transaction.Items)
                {
                    _items.Add(new CreateTransactionItemRequest
                    {
                        ProductName = item.ProductName,
                        UnitPrice = item.UnitPrice,
                        Quantity = item.Quantity,
                    });
                }
                // Estimate discount from original
                var origSubtotal = _transaction.Items.Sum(i => i.UnitPrice * i.Quantity);
                _discountAmount = Math.Max(0, origSubtotal - _transaction.TotalAmount);
            }
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

    private void AddItem()
    {
        _items.Add(new CreateTransactionItemRequest());
    }

    private void RemoveItem(CreateTransactionItemRequest item)
    {
        _items.Remove(item);
    }

    private async Task SaveAsync()
    {
        var validItems = _items.Where(i => !string.IsNullOrWhiteSpace(i.ProductName) && i.UnitPrice > 0 && i.Quantity > 0).ToList();
        if (!validItems.Any())
        {
            _snackbar.AddWarning("Tambahkan minimal satu item yang valid.");
            return;
        }

        _isSaving = true;
        try
        {
            var request = new UpdateTransactionRequest
            {
                PaymentMethod = _paymentMethod,
                Notes = _notes,
                DiscountAmount = _discountAmount,
                Items = validItems,
            };

            var result = await _transactionService.UpdateTransactionAsync(Id, request);
            if (result.Error is not null)
            {
                _snackbar.AddError(result.Error.ToDetail());
                return;
            }

            _snackbar.AddSuccess("Transaksi berhasil diperbarui.");
            _navigationManager.NavigateTo(CommonRouteFor.TransactionDetail(Id));
        }
        catch (Exception ex)
        {
            _snackbar.AddError($"Gagal menyimpan perubahan: {ex.Message}");
        }
        finally
        {
            _isSaving = false;
        }
    }
}
