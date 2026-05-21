using Microsoft.AspNetCore.Components;
using MudBlazor;
using Smlp.Base.Customer.Enums;
using Smlp.Bsui.Common.Constants;
using Smlp.Bsui.Common.Extensions;
using Smlp.Shared.Master.Customers.Commands.Update;
using Smlp.Shared.Master.Customers.Queries;

namespace Smlp.Bsui.Features.Customers;

public partial class Edit
{
    [Parameter] public Guid Id { get; set; }

    private List<BreadcrumbItem> _breadcrumbItems = new();
    private bool _isLoading = true;
    private bool _isSaving;
    private GetCustomerResponse? _customer;
    private readonly UpdateCustomerRequest _model = new();

    protected override async Task OnInitializedAsync()
    {
        SetupBreadcrumb();
        await LoadCustomerAsync();
    }

    private void SetupBreadcrumb()
    {
        _breadcrumbItems = new()
        {
            CommonBreadcrumbFor.Home,
            new("Pelanggan", href: CommonRouteFor.Customers),
            CommonBreadcrumbFor.Active("Edit Pelanggan")
        };
    }

    private async Task LoadCustomerAsync()
    {
        try
        {
            var result = await _customerService.GetCustomerByIdAsync(Id);
            _customer = result.Result;

            if (_customer is null)
            {
                _snackbar.AddError("Data pelanggan tidak ditemukan");
                return;
            }

            _model.FullName = _customer.FullName;
            _model.PhoneNumber = _customer.PhoneNumber;
            _model.EmailAddress = _customer.EmailAddress;
            _model.BirthDate = _customer.BirthDate;
            _model.Gender = _customer.Gender;
            _model.Address = _customer.Address;
            _model.Notes = _customer.Notes;

            BirthDatePicker = _customer.BirthDate?.ToDateTime(TimeOnly.MinValue);
            SelectedGender = _customer.Gender;
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async Task SaveAsync()
    {
        _isSaving = true;
        try
        {
            var result = await _customerService.UpdateCustomerAsync(Id, _model);
            if (result.Error is not null)
            {
                _snackbar.AddError(result.Error.ToDetail());
                return;
            }

            _snackbar.AddSuccess("Data pelanggan berhasil diperbarui");
            _navigationManager.NavigateTo(CommonRouteFor.CustomerDetail(Id));
        }
        finally
        {
            _isSaving = false;
        }
    }

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
