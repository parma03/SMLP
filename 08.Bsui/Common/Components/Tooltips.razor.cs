using MudBlazor;
using Smlp.Base.Payments.Enums;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Smlp.Bsui.Common.Components;

public partial class Tooltips
{
    private Placement _tooltipPlacement = Placement.Right;

    private void OnBreakpointChanged(Breakpoint breakpoint)
    {
        _tooltipPlacement = breakpoint == Breakpoint.Xs
            ? Placement.Bottom
            : Placement.Right;

        StateHasChanged();
    }

    private readonly List<(PaymentStatus Status, Color? Color, string? BgColor, string Description)> _statusMappings =
    [
        (PaymentStatus.WaitingApproval, null, "#8B4513", "Menunggu persetujuan dari pihak berwenang."),
        (PaymentStatus.Rejected, Color.Error, null, "Ditolak dan tidak dapat dilanjutkan (atau perlu pengajuan ulang)."),
        (PaymentStatus.NeedRevision, Color.Warning, null, "Perlu perbaikan sebelum dapat diproses kembali."),
        (PaymentStatus.Pending, Color.Secondary, null, "Dalam antrean proses atau menunggu tindak lanjut."),
        (PaymentStatus.RequestPreparation, Color.Info, null, "Dalam tahap pengolahan dan persiapan pengunggahan data ke SAP."),
        (PaymentStatus.Completed, Color.Success, null, "Proses selesai dan data sudah diunggah ke SAP.")
    ];

    public List<(string Label, Color? Color, string BgColor, string Description)> PaymentStatuses =>
        _statusMappings.Select(x => (
            Label: x.Status.GetDisplayName(),
            Color: x.Color,
            BgColor: x.BgColor,
            Description: x.Description
        )).ToList();
}

#region Enum Helpers
public static class PaymentStatusExtensions
{
    public static string GetDisplayName(this PaymentStatus value)
    {
        return value.GetType()
            .GetMember(value.ToString())
            .First()
            .GetCustomAttribute<DisplayAttribute>()?.Name
            ?? value.ToString();
    }
}
#endregion
