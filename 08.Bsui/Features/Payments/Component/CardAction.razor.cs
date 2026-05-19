using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Psmp.Bsui.Features.Payments.Component;

public partial class CardAction
{
    [Parameter]
    public int Count { get; set; }

    [Parameter]
    public string? Sub { get; set; }

    [Parameter]
    public string? Judul { get; set; }

    [Parameter]
    public decimal? CountCurrency { get; set; }

    [Parameter]
    public string Icon { get; set; } = Icons.Material.Rounded.People;

    [Parameter]
    public string IconMoney { get; set; } = Icons.Material.Rounded.AttachMoney;
    [Parameter]
    public string? CurrencyType { get; set; }

    [Parameter]
    public Color AvatarColor { get; set; } = Color.Tertiary;

    [Parameter]
    public string? ImageSrc { get; set; }
}
