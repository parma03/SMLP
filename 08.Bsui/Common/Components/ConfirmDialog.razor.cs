using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Smlp.Bsui.Common.Components;

/// <summary>
/// Dialog konfirmasi generik menggunakan MudBlazor murni (bukan Syncfusion).
/// Dipakai untuk konfirmasi hapus, atau aksi destruktif lainnya.
/// </summary>
public partial class ConfirmDialog
{
    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; } = default!;

    /// <summary>Judul dialog.</summary>
    [Parameter]
    public string Title { get; set; } = "Konfirmasi";

    /// <summary>Pesan / pertanyaan yang ditampilkan ke user.</summary>
    [Parameter]
    public string Message { get; set; } = "Apakah Anda yakin ingin melanjutkan aksi ini?";

    /// <summary>Ikon MudBlazor di samping judul, misal Icons.Material.Filled.Delete</summary>
    [Parameter]
    public string Icon { get; set; } = Icons.Material.Filled.Warning;

    /// <summary>Warna ikon.</summary>
    [Parameter]
    public Color IconColor { get; set; } = Color.Warning;

    /// <summary>Warna tombol konfirmasi (default: Error = merah untuk hapus).</summary>
    [Parameter]
    public Color ConfirmColor { get; set; } = Color.Error;

    /// <summary>Label tombol konfirmasi.</summary>
    [Parameter]
    public string ConfirmText { get; set; } = "Ya, Hapus";

    /// <summary>Label tombol batal.</summary>
    [Parameter]
    public string CancelText { get; set; } = "Batal";

    private bool _isLoading = false;

    private void OnCancel()
    {
        MudDialog.Cancel();
    }

    private async Task OnConfirm()
    {
        _isLoading = true;
        StateHasChanged();
        // Beri sedikit delay agar loading state terlihat sebelum dialog ditutup
        await Task.Delay(100);
        MudDialog.Close(DialogResult.Ok(true));
    }
}
