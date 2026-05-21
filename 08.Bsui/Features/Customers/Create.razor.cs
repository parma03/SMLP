using MudBlazor;
using Smlp.Base.Customer.Enums;
using Smlp.Bsui.Common.Constants;
using Smlp.Bsui.Common.Extensions;
using Smlp.Shared.Master.Customers.Commands.Create;

namespace Smlp.Bsui.Features.Customers;

public partial class Create
{
    private List<BreadcrumbItem> _breadcrumbItems = new();
    private bool _isLoading = false;
    private readonly CreateCustomerRequest _model = new();

    protected override void OnInitialized()
    {
        SetupBreadcrumb();
    }

    private void SetupBreadcrumb()
    {
        _breadcrumbItems = new()
        {
            CommonBreadcrumbFor.Home,
            new("Pelanggan", href: CommonRouteFor.Customers),
            CommonBreadcrumbFor.Active("Tambah Pelanggan"),
        };
    }

    private async Task SaveCustomerAsync()
    {
        if (string.IsNullOrWhiteSpace(_model.FullName) || string.IsNullOrWhiteSpace(_model.PhoneNumber))
        {
            _snackbar.AddWarning("Nama dan nomor HP wajib diisi.");
            return;
        }

        _isLoading = true;
        try
        {
            var result = await _customerService.CreateCustomerAsync(_model);
            if (result.Error is not null)
            {
                _snackbar.AddError(result.Error.ToDetail());
                return;
            }

            _snackbar.AddSuccess($"Pelanggan {_model.FullName} berhasil didaftarkan!");
            _navigationManager.NavigateTo(CommonRouteFor.Customers);
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

    // Helper untuk bind Gender enum dari MudSelect
    private Gender? _selectedGender;
    private Gender? SelectedGender
    {
        get => _selectedGender;
        set
        {
            _selectedGender = value;
            _model.Gender = value;
        }
    }

    // Helper untuk bind DateOnly dari MudDatePicker (DateTime?)
    private DateTime? _birthDatePicker;
    private DateTime? BirthDatePicker
    {
        get => _birthDatePicker;
        set
        {
            _birthDatePicker = value;
            _model.BirthDate = value.HasValue ? DateOnly.FromDateTime(value.Value) : null;
        }
    }
}
