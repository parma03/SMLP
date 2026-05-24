using MudBlazor;
using Smlp.Bsui.Common.Constants;
using Smlp.Bsui.Common.Extensions;
using Smlp.Shared.Master.Products.Commands.Create;

namespace Smlp.Bsui.Features.Products;

public partial class Create
{
    private List<BreadcrumbItem> _breadcrumbItems = new();
    private bool _isLoading = false;
    private readonly CreateProductRequest _model = new() { IsActive = true };

    protected override void OnInitialized()
    {
        SetupBreadcrumb();
    }

    private void SetupBreadcrumb()
    {
        _breadcrumbItems = new()
        {
            CommonBreadcrumbFor.Home,
            new("Produk", href: CommonRouteFor.Products),
            CommonBreadcrumbFor.Active("Tambah Produk"),
        };
    }

    private async Task SaveProductAsync()
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
            var result = await _productService.CreateProductAsync(_model);
            if (result.Error is not null)
            {
                _snackbar.AddError(result.Error.ToDetail());
                return;
            }

            _snackbar.AddSuccess($"Produk \"{_model.Name}\" berhasil ditambahkan!");
            _navigationManager.NavigateTo(CommonRouteFor.Products);
        }
        catch (Exception ex)
        {
            _snackbar.AddError($"Gagal menyimpan: {ex.Message}");
        }
        finally
        {
            _isLoading = false;
        }
    }
}
