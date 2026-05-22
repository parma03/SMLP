using MudBlazor;
using Smlp.Base.Transaction.Enums;
using Smlp.Bsui.Common.Constants;
using Smlp.Bsui.Common.Extensions;
using Smlp.Shared.Master.Customers.Queries;
using Smlp.Shared.Master.Products.Queries;
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

    // Discount
    private decimal _discountPercent = 0;
    private decimal _discountAmount = 0;
    private bool _usePercentDiscount = true;

    // Items
    private readonly List<CreateTransactionItemRequest> _items = new();
    private decimal Subtotal => _items.Sum(i => i.UnitPrice * i.Quantity);
    private decimal DiscountValue => _usePercentDiscount
        ? Math.Round(Subtotal * _discountPercent / 100, 0)
        : _discountAmount;
    private decimal GrandTotal => Math.Max(0, Subtotal - DiscountValue);
    private int EstimatedPoints => (int)(GrandTotal / 10_000);

    // Product cache for autocomplete
    private List<GetProductResponse> _productCache = new();
    private bool _productCacheLoaded = false;

    protected override void OnInitialized()
    {
        SetupBreadcrumb();
        AddItem();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await LoadProductCacheAsync();
        }
    }

    private async Task LoadProductCacheAsync()
    {
        try
        {
            var result = await _productService.GetProductsAsync(new GetProductRequest { IsActive = true, PageSize = 200 });
            if (result.Result?.Items is not null)
            {
                _productCache = result.Result.Items.ToList();
                _productCacheLoaded = true;
                await InvokeAsync(StateHasChanged);
            }
        }
        catch (Exception ex)
        {
            _snackbar.AddWarning($"Gagal memuat daftar produk: {ex.Message}");
        }
    }

    private Task<IEnumerable<string>> SearchProductsAsync(string search, CancellationToken cancellationToken)
    {
        if (!_productCacheLoaded)
            return Task.FromResult(Enumerable.Empty<string>());

        var filtered = string.IsNullOrWhiteSpace(search)
            ? _productCache
            : _productCache.Where(p => p.Name.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();

        return Task.FromResult(filtered.Select(p => p.Name).Take(10));
    }

    private void OnProductSelected(CreateTransactionItemRequest item, string? productName)
    {
        item.ProductName = productName ?? string.Empty;
        // Auto-fill price if product found in cache
        var product = _productCache.FirstOrDefault(p => p.Name == productName);
        if (product is not null && item.UnitPrice == 0)
        {
            item.UnitPrice = product.Price;
        }

        StateHasChanged();
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
            var discountNote = DiscountValue > 0 ? $" | Diskon: Rp {DiscountValue:N0}" : "";
            var finalNotes = string.IsNullOrWhiteSpace(_notes) ? discountNote.TrimStart(' ', '|', ' ') : $"{_notes}{discountNote}";

            var request = new CreateTransactionRequest
            {
                CustomerId = _selectedCustomer.Id,
                PaymentMethod = _paymentMethod,
                Notes = string.IsNullOrWhiteSpace(finalNotes) ? null : finalNotes,
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
