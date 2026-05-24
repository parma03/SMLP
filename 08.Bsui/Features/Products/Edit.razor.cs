using Microsoft.AspNetCore.Components;
using MudBlazor;
using Smlp.Bsui.Common.Constants;
using Smlp.Bsui.Common.Extensions;
using Smlp.Shared.Master.Products.Commands.Update;

namespace Smlp.Bsui.Features.Products;

public partial class Edit
{
    [Parameter] public Guid Id { get; set; }

    private List<BreadcrumbItem> _breadcrumbItems = new();
    private bool _isLoading = false;
    private bool _isLoadingData = true;
    private bool _notFound = false;
    private readonly UpdateProductRequest _model = new();

    protected override async Task OnInitializedAsync()
    {
        SetupBreadcrumb();
        await LoadProductAsync();
    }

    private void SetupBreadcrumb()
    {
        _breadcrumbItems = new()
        {
            CommonBreadcrumbFor.Home,
            new("Produk", href: CommonRouteFor.Products),
            CommonBreadcrumbFor.Active("Edit Produk"),
        };
    }

    private async Task LoadProductAsync()
    {
        _isLoadingData = true;
        try
        {
            var result = await _productService.GetProductByIdAsync(Id);
            if (result.Error is not null)
            {
                _notFound = true;
                _snackbar.AddError(result.Error.ToDetail());
                return;
            }

            var product = result.Result!;
            _model.Name = product.Name;
            _model.Description = product.Description;
            _model.Price = product.Price;
            _model.IsActive = product.IsActive;
            _model.CategoryId = product.CategoryId;
        }
        catch (Exception ex)
        {
            _notFound = true;
            _snackbar.AddError($"Gagal memuat data produk: {ex.Message}");
        }
        finally
        {
            _isLoadingData = false;
        }
    }

    private async Task UpdateProductAsync()
    {
        if (string.IsNullOrWhiteSpace(_model.Name))
        {
            _snackbar.AddWarning("Nama produk wajib diisi.");
            return;
        }

        if (_model.Price < 0)
        {
            _snackbar.AddWarning("Harga tidak boleh negatif.");
            return;
        }

        _isLoading = true;
        try
        {
            var result = await _productService.UpdateProductAsync(Id, _model);
            if (result.Error is not null)
            {
                _snackbar.AddError(result.Error.ToDetail());
                return;
            }

            _snackbar.AddSuccess($"Produk \"{_model.Name}\" berhasil diperbarui!");
            _navigationManager.NavigateTo(CommonRouteFor.Products);
        }
        catch (Exception ex)
        {
            _snackbar.AddError($"Gagal memperbarui: {ex.Message}");
        }
        finally
        {
            _isLoading = false;
        }
    }
}
