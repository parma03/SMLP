using Smlp.Base.Transaction.Enums;

namespace Smlp.Shared.Transaction.Transactions.Commands.Create;

/// <summary>
/// Satu entri pembayaran untuk mendukung multiple payment methods (split payment).
/// </summary>
public class PaymentEntryRequest
{
    public PaymentMethod PaymentMethod { get; set; }

    /// <summary>Jumlah yang dibayar dengan metode ini.</summary>
    public decimal Amount { get; set; }

    /// <summary>Referensi pembayaran (nomor transfer, nomor bukti, dll.) — opsional.</summary>
    public string? Reference { get; set; }
}
