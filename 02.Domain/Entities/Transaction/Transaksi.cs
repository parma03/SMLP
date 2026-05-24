using Smlp.Base.Transaction.Enums;
using Smlp.Domain.Abstracts;
using Smlp.Domain.Entities.Master;

namespace Smlp.Domain.Entities.Transaction
{
    public class Transaksi : AuditableEntity
    {
        public string TransactionCode { get; set; } = default!;
        public Guid CustomerId { get; set; }
        public Customer Customer { get; set; } = default!;
        public DateTimeOffset TransactionDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal GrandTotal { get; set; }
        public int PointsEarned { get; set; }
        public int BonusPoints { get; set; }

        /// <summary>Metode pembayaran utama (backward-compat, atau jika single payment).</summary>
        public PaymentMethod PaymentMethod { get; set; }

        public string? Notes { get; set; }
        public TransactionStatus Status { get; set; }

        // Snapshot tier saat transaksi
        public string CustomerTierSnapshot { get; set; } = default!;
        public decimal PointMultiplierSnapshot { get; set; }

        // Navigation
        public ICollection<TransactionItem> Items { get; set; } = new List<TransactionItem>();

        /// <summary>Daftar pembayaran (mendukung split payment).</summary>
        public ICollection<TransactionPayment> Payments { get; set; } = new List<TransactionPayment>();
    }
}
