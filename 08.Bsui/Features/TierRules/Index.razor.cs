using MudBlazor;
using Smlp.Base.Customer.Enums;
using Smlp.Bsui.Common.Components;
using Smlp.Bsui.Common.Extensions;
using Smlp.Shared.Master.TierRules.Commands.Create;
using Smlp.Shared.Master.TierRules.Commands.Update;
using Smlp.Shared.Master.TierRules.Queries;

namespace Smlp.Bsui.Features.TierRules;

public partial class Index
{
    private bool _loading = false;
    private bool _showForm = false;
    private bool _isSaving = false;
    private Guid? _editingId = null;

    private List<GetTierRuleResponse> _rules = new();
    private CreateTierRuleRequest _form = new() { IsActive = true, PointMultiplier = 1.0m };

    protected override async Task OnInitializedAsync()
    {
        await LoadRulesAsync();
    }

    private async Task LoadRulesAsync()
    {
        _loading = true;
        try
        {
            var result = await _tierRuleService.GetTierRulesAsync(new GetTierRuleRequest { PageSize = 50 });
            if (result.Error is not null)
            {
                _snackbar.AddError(result.Error.ToDetail());
                return;
            }

            _rules = result.Result!.Items.OrderBy(r => r.Tier).ToList();
        }
        catch (Exception ex)
        {
            _snackbar.AddError($"Gagal memuat aturan tier: {ex.Message}");
        }
        finally
        {
            _loading = false;
        }
    }

    private void ShowAddForm()
    {
        _editingId = null;
        _form = new() { IsActive = true, PointMultiplier = 1.0m, Tier = CustomerTier.Bronze };
        _showForm = true;
    }

    private void ShowEditForm(GetTierRuleResponse rule)
    {
        _editingId = rule.Id;
        _form = new()
        {
            Name = rule.Name,
            Tier = rule.Tier,
            MinimumLifetimePoints = rule.MinimumLifetimePoints,
            MaximumLifetimePoints = rule.MaximumLifetimePoints,
            PointMultiplier = rule.PointMultiplier,
            BenefitDescription = rule.BenefitDescription,
            BadgeColor = rule.BadgeColor,
            IsActive = rule.IsActive,
        };
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

        _isSaving = true;
        try
        {
            if (_editingId.HasValue)
            {
                var updateReq = new UpdateTierRuleRequest
                {
                    Name = _form.Name,
                    Tier = _form.Tier,
                    MinimumLifetimePoints = _form.MinimumLifetimePoints,
                    MaximumLifetimePoints = _form.MaximumLifetimePoints,
                    PointMultiplier = _form.PointMultiplier,
                    BenefitDescription = _form.BenefitDescription,
                    BadgeColor = _form.BadgeColor,
                    IsActive = _form.IsActive,
                };
                var result = await _tierRuleService.UpdateTierRuleAsync(_editingId.Value, updateReq);
                if (result.Error is not null)
                {
                    _snackbar.AddError(result.Error.ToDetail());
                    return;
                }

                _snackbar.AddSuccess($"Aturan tier '{_form.Name}' berhasil diperbarui.");
            }
            else
            {
                var result = await _tierRuleService.CreateTierRuleAsync(_form);
                if (result.Error is not null)
                {
                    _snackbar.AddError(result.Error.ToDetail());
                    return;
                }

                _snackbar.AddSuccess($"Aturan tier '{_form.Name}' berhasil ditambahkan.");
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

    private async Task DeleteAsync(GetTierRuleResponse rule)
    {
        var parameters = new DialogParameters
        {
            [nameof(ConfirmDialog.Title)] = "Hapus Aturan Tier",
            [nameof(ConfirmDialog.Message)] = $"Yakin ingin menghapus aturan tier '{rule.Name}'? Aksi ini tidak bisa dibatalkan.",
            [nameof(ConfirmDialog.Icon)] = Icons.Material.Filled.Delete,
            [nameof(ConfirmDialog.ConfirmText)] = "Ya, Hapus",
        };
        var dialog = await _dialogService.ShowAsync<ConfirmDialog>("Hapus Aturan Tier", parameters);
        var dialogResult = await dialog.Result;
        if (dialogResult is null || dialogResult.Canceled) return;

        var result = await _tierRuleService.DeleteTierRuleAsync(rule.Id);
        if (result.Error is not null)
        {
            _snackbar.AddError(result.Error.ToDetail());
            return;
        }

        _snackbar.AddSuccess($"Aturan tier '{rule.Name}' berhasil dihapus.");
        await LoadRulesAsync();
    }

    private (string label, string style) GetTierDisplay(CustomerTier tier) => tier switch
    {
        CustomerTier.Bronze => ("🥉 Bronze", "background:#CD7F32; color:white;"),
        CustomerTier.Silver => ("🥈 Silver", "background:#9E9E9E; color:white;"),
        CustomerTier.Gold => ("🥇 Gold", "background:#F9A825; color:white;"),
        CustomerTier.Platinum => ("💎 Platinum", "background:#1565C0; color:white;"),
        _ => (tier.ToString(), "background:#6F4E37; color:white;")
    };
}
