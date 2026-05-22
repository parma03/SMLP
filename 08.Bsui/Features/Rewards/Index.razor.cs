using MudBlazor;
using Smlp.Base.Customer.Enums;
using Smlp.Base.Reward.Enums;
using Smlp.Bsui.Common.Extensions;
using Smlp.Bsui.Features.Rewards.Components;
using Smlp.Shared.Master.Rewards.Queries;

namespace Smlp.Bsui.Features.Rewards;

public partial class Index
{
    private string _searchText = string.Empty;
    private RewardType? _filterType;
    private bool _loading = false;
    private List<GetRewardResponse> _rewards = new();

    protected override async Task OnInitializedAsync()
    {
        await LoadRewardsAsync();
    }

    private async Task LoadRewardsAsync()
    {
        _loading = true;
        try
        {
            var request = new GetRewardRequest
            {
                Search = string.IsNullOrWhiteSpace(_searchText) ? null : _searchText,
                Type = _filterType,
                PageSize = 50
            };

            var result = await _rewardService.GetRewardsAsync(request);
            if (result.Error is not null)
            {
                _snackbar.AddError(result.Error.ToDetail());
                return;
            }

            _rewards = result.Result!.Items.ToList();
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

    private async Task OpenRedeemDialogAsync(GetRewardResponse reward)
    {
        var parameters = new DialogParameters<DialogRedeem>
        {
            { x => x.Reward, reward }
        };
        var options = new DialogOptions { MaxWidth = MaxWidth.Small, FullWidth = true, CloseButton = true };
        var dialog = await _dialogService.ShowAsync<DialogRedeem>("Tukar Reward", parameters, options);
        var result = await dialog.Result;
        if (!result.Canceled)
        {
            await LoadRewardsAsync(); // refresh stock count
        }
    }

    private async Task ToggleActiveAsync(GetRewardResponse reward)
    {
        var result = await _rewardService.ToggleActiveAsync(reward.Id);
        if (result.Error is not null)
        {
            _snackbar.AddError(result.Error.ToDetail());
            return;
        }

        var label = reward.IsActive ? "dinonaktifkan" : "diaktifkan";
        _snackbar.AddSuccess($"Reward {reward.Name} berhasil {label}.");
        await LoadRewardsAsync();
    }

    private async Task DeleteAsync(GetRewardResponse reward)
    {
        var result = await _rewardService.DeleteRewardAsync(reward.Id);
        if (result.Error is not null)
        {
            _snackbar.AddError(result.Error.ToDetail());
            return;
        }

        _snackbar.AddSuccess($"Reward {reward.Name} berhasil dihapus.");
        await LoadRewardsAsync();
    }

    private string GetRewardTypeLabel(RewardType type) => type switch
    {
        RewardType.FreeItem => "Produk Gratis",
        RewardType.DiscountAmount => "Diskon Nominal",
        RewardType.DiscountPercent => "Diskon Persen",
        RewardType.Voucher => "Voucher",
        _ => type.ToString()
    };

    private string GetMinTierLabel(int minTier) => (CustomerTier)minTier switch
    {
        CustomerTier.Bronze => "Bronze+",
        CustomerTier.Silver => "Silver+",
        CustomerTier.Gold => "Gold+",
        CustomerTier.Platinum => "Platinum Only",
        _ => "Semua Tier"
    };

    private Color GetRewardColor(RewardType type) => type switch
    {
        RewardType.FreeItem => Color.Success,
        RewardType.DiscountAmount => Color.Info,
        RewardType.DiscountPercent => Color.Warning,
        RewardType.Voucher => Color.Secondary,
        _ => Color.Default
    };

    private bool IsExpired(GetRewardResponse r) =>
        r.ValidUntil.HasValue && r.ValidUntil.Value < DateOnly.FromDateTime(DateTime.Today);

    private bool IsOutOfStock(GetRewardResponse r) =>
        r.StockLimit.HasValue && r.StockRemaining <= 0;
}
