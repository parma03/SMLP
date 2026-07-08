using MudBlazor;
using Smlp.Bsui.Common.Components;
using Smlp.Bsui.Common.Extensions;
using Smlp.Shared.Master.PointRules.Commands.Create;
using Smlp.Shared.Master.PointRules.Commands.Update;
using Smlp.Shared.Master.PointRules.Queries;

namespace Smlp.Bsui.Features.PointRules;

public partial class Index
{
    private bool _loading = false;
    private bool _showForm = false;
    private bool _isSaving = false;
    private Guid? _editingId = null;

    private List<GetPointRuleResponse> _rules = new();
    private CreatePointRuleRequest _form = new() { IsActive = true, SpendAmount = 10000, PointEarned = 1 };

    // Helper props to convert between DateTime? and DateOnly?
    private decimal? _formBonusMultiplier;
    private DateTime? _formValidFrom;
    private DateTime? _formValidUntil;

    protected override async Task OnInitializedAsync()
    {
        await LoadRulesAsync();
    }

    private async Task LoadRulesAsync()
    {
        _loading = true;
        try
        {
            var result = await _pointRuleService.GetPointRulesAsync(new GetPointRuleRequest { PageSize = 50 });
            if (result.Error is not null)
            {
                _snackbar.AddError(result.Error.ToDetail());
                return;
            }

            _rules = result.Result!.Items.ToList();
        }
        catch (Exception ex)
        {
            _snackbar.AddError($"Gagal memuat aturan poin: {ex.Message}");
        }
        finally
        {
            _loading = false;
        }
    }

    private void ShowAddForm()
    {
        _editingId = null;
        _form = new() { IsActive = true, SpendAmount = 10000, PointEarned = 1 };
        _formBonusMultiplier = null;
        _formValidFrom = null;
        _formValidUntil = null;
        _showForm = true;
    }

    private void ShowEditForm(GetPointRuleResponse rule)
    {
        _editingId = rule.Id;
        _form = new()
        {
            Name = rule.Name,
            SpendAmount = rule.SpendAmount,
            PointEarned = rule.PointEarned,
            BonusMultiplier = rule.BonusMultiplier,
            IsActive = rule.IsActive,
            Notes = rule.Notes,
        };
        _formBonusMultiplier = rule.BonusMultiplier;
        _formValidFrom = rule.ValidFrom.HasValue ? rule.ValidFrom.Value.ToDateTime(TimeOnly.MinValue) : null;
        _formValidUntil = rule.ValidUntil.HasValue ? rule.ValidUntil.Value.ToDateTime(TimeOnly.MinValue) : null;
        _showForm = true;
    }

    private void CancelForm()
    {
        _showForm = false;
        _editingId = null;
    }

    private async Task SaveRuleAsync()
    {
        if (string.IsNullOrWhiteSpace(_form.Name))
        {
            _snackbar.AddWarning("Nama aturan wajib diisi.");
            return;
        }

        _form.BonusMultiplier = _formBonusMultiplier;
        _form.ValidFrom = _formValidFrom.HasValue ? DateOnly.FromDateTime(_formValidFrom.Value) : null;
        _form.ValidUntil = _formValidUntil.HasValue ? DateOnly.FromDateTime(_formValidUntil.Value) : null;

        _isSaving = true;
        try
        {
            if (_editingId.HasValue)
            {
                var updateReq = new UpdatePointRuleRequest
                {
                    Name = _form.Name,
                    SpendAmount = _form.SpendAmount,
                    PointEarned = _form.PointEarned,
                    BonusMultiplier = _form.BonusMultiplier,
                    ValidFrom = _form.ValidFrom,
                    ValidUntil = _form.ValidUntil,
                    IsActive = _form.IsActive,
                    Notes = _form.Notes,
                };
                var result = await _pointRuleService.UpdatePointRuleAsync(_editingId.Value, updateReq);
                if (result.Error is not null)
                {
                    _snackbar.AddError(result.Error.ToDetail());
                    return;
                }

                _snackbar.AddSuccess($"Aturan '{_form.Name}' berhasil diperbarui.");
            }
            else
            {
                var result = await _pointRuleService.CreatePointRuleAsync(_form);
                if (result.Error is not null)
                {
                    _snackbar.AddError(result.Error.ToDetail());
                    return;
                }

                _snackbar.AddSuccess($"Aturan '{_form.Name}' berhasil ditambahkan.");
            }

            _showForm = false;
            _editingId = null;
            await LoadRulesAsync();
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

    private async Task DeleteAsync(GetPointRuleResponse rule)
    {
        var parameters = new DialogParameters
        {
            [nameof(ConfirmDialog.Title)] = "Hapus Aturan Poin",
            [nameof(ConfirmDialog.Message)] = $"Yakin ingin menghapus aturan '{rule.Name}'? Aksi ini tidak bisa dibatalkan.",
            [nameof(ConfirmDialog.Icon)] = Icons.Material.Filled.Delete,
            [nameof(ConfirmDialog.ConfirmText)] = "Ya, Hapus",
        };
        var dialog = await _dialogService.ShowAsync<ConfirmDialog>("Hapus Aturan Poin", parameters);
        var dialogResult = await dialog.Result;
        if (dialogResult is null || dialogResult.Canceled) return;

        var result = await _pointRuleService.DeletePointRuleAsync(rule.Id);
        if (result.Error is not null)
        {
            _snackbar.AddError(result.Error.ToDetail());
            return;
        }

        _snackbar.AddSuccess($"Aturan '{rule.Name}' berhasil dihapus.");
        await LoadRulesAsync();
    }
}
