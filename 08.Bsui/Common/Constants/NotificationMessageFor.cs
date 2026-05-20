namespace Smlp.Bsui.Common.Constants;

public static class NotificationMessageFor
{
    public static string DataNotFound(string dataFor)
    {
        return $"Data {dataFor} tidak ditemukan.";
    }

    public static string CannotEmpty(string dataFor)
    {
        return $"Data {dataFor} tidak boleh kosong.";
    }

    public static string MinimumTextRequired(string dataFor)
    {
        return $"Data {dataFor} tidak memenuhi panjang minimum.";
    }

    public static string SaveSuccess(string dataFor)
    {
        return $"Data {dataFor} berhasil disimpan.";
    }

    public static string DeleteSuccess(string dataFor)
    {
        return $"Data {dataFor} berhasil dihapus.";
    }

    public static string UpdateSuccess(string dataFor)
    {
        return $"Data {dataFor} berhasil diperbarui.";
    }

    public static readonly string InsufficientPoints = "Poin pelanggan tidak mencukupi untuk redeem reward ini.";
    public static readonly string TransactionSaved = "Transaksi berhasil disimpan dan poin telah ditambahkan.";
    public static readonly string RedeemSuccess = "Reward berhasil ditukarkan.";
}
