using Smlp.Base.Transaction.Enums;

namespace Smlp.Shared.Transaction.Transactions.Commands.Create;

public class CreateTransactionRequest
{
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Metode pembayaran utama (dipakai jika Payments kosong / backward-compat).
    /// </summary>
    public PaymentMethod PaymentMethod { get; set; }

    /// <summary>
    /// Daftar pembayaran (untuk split/multiple payment).
    /// Jika diisi, PaymentMethod di atas diabaikan.
    /// </summary>
    public List<PaymentEntryRequest> Payments { get; set; } = new();

    public string? Notes { get; set; }

    /// <summary>Diskon nominal yang diterapkan ke total transaksi.</summary>
    public decimal DiscountAmount { get; set; } = 0;

    public List<CreateTransactionItemRequest> Items { get; set; } = new();
}
