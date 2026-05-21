using MudBlazor;
using Smlp.Base.Transaction.Enums;
using Smlp.Bsui.Common.Constants;
using Smlp.Bsui.Common.Extensions;
using Smlp.Shared.Master.Customers.Queries;
using Smlp.Shared.Transaction.Transactions.Commands.Create;

namespace Smlp.Bsui.Features.Transactions;

public partial class Create
{
    private List<BreadcrumbItem> _breadcrumbItems = new();
    private bool _isLoading = false;

    // Customer search
    private string _memberSearch = string.Empty;
    private bool _isSearching = false;
    private GetCustomerResponse? _selectedCustomer;

    // Payment
    private PaymentMethod _paymentMethod = PaymentMethod.Tunai;
    private string? _notes;

    // Items
    private readonly List<CreateTransactionItemRequest> _items = new();
    private decimal Subtotal => _items.Sum(i => i.UnitPrice * i.Quantity);
    private int EstimatedPoints => (int)(Subtotal / 10_000);

    protected override void OnInitialized()
    {
        SetupBreadcrumb();
        AddItem(); // default 1 item
    }

    private void SetupBreadcrumb()
    {
        _breadcrumbItems = new()
        {
            CommonBreadcrumbFor.Home,
            new("Transaksi", href: CommonRouteFor.Transactions),
            CommonBreadcrumbFor.Active("Input Transaksi"),
        };
    }

    private async Task SearchCustomerAsync()
    {
        if (string.IsNullOrWhiteSpace(_memberSearch))
        {
            _snackbar.AddWarning("Masukkan No. HP atau kode member terlebih dahulu.");
            return;
        }

        _isSearching = true;
        _selectedCustomer = null;
        try
        {
            var result = await _customerService.GetCustomersAsync(new Smlp.Shared.Master.Customers.Queries.GetCustomerRequest
            {
                Search = _memberSearch,
                PageSize = 1
            });

            if (result.Error is not null)
            {
                _snackbar.AddError(result.Error.ToDetail());
                return;
            }

            var found = result.Result?.Items?.FirstOrDefault();
            if (found is null)
            {
                _snackbar.AddWarning("Pelanggan tidak ditemukan. Pastikan No. HP atau kode member benar.");
                return;
            }

            _selectedCustomer = found;
        }
        catch (Exception ex)
        {
            _snackbar.AddError($"Gagal mencari pelanggan: {ex.Message}");
        }
        finally
        {
            _isSearching = false;
        }
    }

    private async Task OnMemberSearchKeyUp(Microsoft.AspNetCore.Components.Web.KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
            await SearchCustomerAsync();
    }

    private void ClearCustomer()
    {
        _selectedCustomer = null;
        _memberSearch = string.Empty;
    }

    private void AddItem()
    {
        _items.Add(new CreateTransactionItemRequest());
    }

    private void RemoveItem(CreateTransactionItemRequest item)
    {
        _items.Remove(item);
    }

    private async Task SaveTransaction()
    {
        if (_selectedCustomer is null)
        {
            _snackbar.AddWarning("Pilih pelanggan terlebih dahulu.");
            return;
        }

        var validItems = _items.Where(i => !string.IsNullOrWhiteSpace(i.ProductName) && i.UnitPrice > 0 && i.Quantity > 0).ToList();
        if (!validItems.Any())
        {
            _snackbar.AddWarning("Tambahkan minimal satu item dengan nama produk dan harga yang valid.");
            return;
        }

        _isLoading = true;
        try
        {
            var request = new CreateTransactionRequest
            {
                CustomerId = _selectedCustomer.Id,
                PaymentMethod = _paymentMethod,
                Notes = _notes,
                Items = validItems.Select(i => new CreateTransactionItemRequest
                {
                    ProductName = i.ProductName!,
                    UnitPrice = i.UnitPrice,
                    Quantity = i.Quantity
                }).ToList()
            };

            var result = await _transactionService.CreateTransactionAsync(request);
            if (result.Error is not null)
            {
                _snackbar.AddError(result.Error.ToDetail());
                return;
            }

            _snackbar.AddSuccess($"Transaksi berhasil disimpan! {EstimatedPoints} poin ditambahkan ke akun {_selectedCustomer.FullName}.");
            _navigationManager.NavigateTo(CommonRouteFor.Transactions);
        }
        catch (Exception ex)
        {
            _snackbar.AddError($"Gagal menyimpan transaksi: {ex.Message}");
        }
        finally
        {
            _isLoading = false;
        }
    }
}
