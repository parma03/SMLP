namespace Smlp.Shared.Transaction.LoyaltyHistories.Queries;

/// <summary>
/// Item riwayat aktivitas loyalitas gabungan (transaksi, penukaran reward, dan penyesuaian poin)
/// yang ditampilkan pada halaman Riwayat Loyalitas.
/// </summary>
public class LoyaltyActivityItem
{
    public DateTimeOffset Date { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerMemberCode { get; set; } = string.Empty;
    public string ActivityType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Reference { get; set; }
    public int Points { get; set; }
    public string? Status { get; set; }
}
