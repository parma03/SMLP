using MudBlazor;
using Smlp.Base.Transaction.Enums;
using Smlp.Bsui.Common.Extensions;
using Smlp.Shared.Master.Customers.Queries;
using Smlp.Shared.Transaction.PointAdjustments.Commands.Create;
using Smlp.Shared.Transaction.PointAdjustments.Queries;
namespace Smlp.Bsui.Features.PointAdjustments;

public partial class Index
{
    // Form
    private string _memberSearch = string.Empty;
    private bool _isSearching = false;
    private GetCustomerResponse? _selectedCustomer;
    private AdjustmentType _adjustmentType = AdjustmentType.Bonus;
    private int _points = 0;
    private string _reason = string.Empty;
    private bool _isSaving = false;

    // List
    private bool _loading = false;
    private List<GetPointAdjustmentResponse> _adjustments = new();
    private readonly int _pageNumber = 1;
    private readonly int _pageSize = 10;

    protected override async Task OnInitializedAsync()
    {
        await LoadAdjustmentsAsync();
    }

    private async Task LoadAdjustmentsAsync()
    {
        _loading = true;
        try
        {
            var request = new GetPointAdjustmentRequest
            {
                PageNumber = _pageNumber,
                PageSize = _pageSize
            };

            var result = await _pointAdjustmentService.GetAdjustmentsAsync(request);
            if (result.Error is not null)
            {
                _snackbar.AddError(result.Error.ToDetail());
                return;
            }

            _adjustments = result.Result!.Items.ToList();
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

    private async Task SearchCustomerAsync()
    {
        if (string.IsNullOrWhiteSpace(_memberSearch))
            return;

        _isSearching = true;
        try
        {
            var request = new GetCustomerRequest { Search = _memberSearch, PageSize = 5 };
            var result = await _customerService.GetCustomersAsync(request);
            if (result.Error is not null)
            {
                _snackbar.AddError(result.Error.ToDetail());
                return;
            }

            var found = result.Result?.Items.FirstOrDefault();
            if (found is null)
            {
                _snackbar.AddWarning("Pelanggan tidak ditemukan.");
                _selectedCustomer = null;
                return;
            }

            _selectedCustomer = found;
        }
        finally
        {
            _isSearching = false;
        }
    }

    private async Task SaveAdjustmentAsync()
    {
        if (_selectedCustomer is null)
        {
            _snackbar.AddWarning("Cari dan pilih pelanggan terlebih dahulu.");
            return;
        }

        if (_points <= 0)
        {
            _snackbar.AddWarning("Jumlah poin harus lebih dari 0.");
            return;
        }

        if (string.IsNullOrWhiteSpace(_reason))
        {
            _snackbar.AddWarning("Alasan penyesuaian wajib diisi.");
            return;
        }

        _isSaving = true;
        try
        {
            var request = new CreatePointAdjustmentRequest
            {
                CustomerId = _selectedCustomer.Id,
                Points = _points,
                AdjustmentType = _adjustmentType,
                Reason = _reason
            };

            var result = await _pointAdjustmentService.CreateAdjustmentAsync(request);
            if (result.Error is not null)
            {
                _snackbar.AddError(result.Error.ToDetail());
                return;
            }

            var action = _adjustmentType == AdjustmentType.Bonus ? "ditambahkan ke" : "dikurangi dari";
            _snackbar.AddSuccess($"{_points} poin berhasil {action} akun {_selectedCustomer.FullName}.");

            // Reset form
            _selectedCustomer = null;
            _memberSearch = string.Empty;
            _points = 0;
            _reason = string.Empty;

            await LoadAdjustmentsAsync();
        }
        catch (Exception ex)
        {
            _snackbar.AddError($"Gagal menyimpan: {ex.Message}");
        }
        finally
        {
            _isSaving = false;
        }
    }

    private string GetAdjustmentTypeLabel(AdjustmentType type) => type switch
    {
        AdjustmentType.Bonus => "Bonus",
        AdjustmentType.Correction => "Koreksi",
        AdjustmentType.Penalty => "Penalti",
        AdjustmentType.Expiry => "Kadaluarsa",
        AdjustmentType.Reversal => "Reversal",
        _ => type.ToString()
    };

    private Color GetAdjustmentColor(AdjustmentType type) => type switch
    {
        AdjustmentType.Bonus => Color.Success,
        AdjustmentType.Correction => Color.Info,
        AdjustmentType.Penalty => Color.Warning,
        AdjustmentType.Expiry => Color.Default,
        AdjustmentType.Reversal => Color.Secondary,
        _ => Color.Default
    };
}
