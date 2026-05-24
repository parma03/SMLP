using Smlp.Base.Transaction.Enums;
using Smlp.Domain.Abstracts;

namespace Smlp.Domain.Entities.Transaction;

/// <summary>
/// Satu entri pembayaran dalam transaksi (mendukung split payment).
/// </summary>
public class TransactionPayment : Entity
{
    public Guid TransactionId { get; set; }
    public Transaksi Transaction { get; set; } = default!;

    public PaymentMethod PaymentMethod { get; set; }
    public decimal Amount { get; set; }

    /// <summary>Nomor referensi pembayaran (opsional: nomor transfer, nomor bukti, dll.)</summary>
    public string? Reference { get; set; }
}
