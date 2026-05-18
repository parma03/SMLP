using System.Transactions;
using Smlp.Base.Transaction.Enums;
using Smlp.Domain.Abstracts;
using Smlp.Domain.Entities.Master;

namespace Smlp.Domain.Entities.Transaction
{
    public class Transaction : AuditableEntity
    {
        public string TransactionCode { get; set; } = default!;
        public Guid CustomerId { get; set; }
        public Customer Customer { get; set; } = default!;
        public DateTimeOffset TransactionDate { get; set; }
        public decimal TotalAmount { get; set; }
        public int PointsEarned { get; set; }
        public int BonusPoints { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string? Notes { get; set; }
        public TransactionStatus Status { get; set; }

        // Snapshot tier saat transaksi
        public string CustomerTierSnapshot { get; set; } = default!;
        public decimal PointMultiplierSnapshot { get; set; }

        // Navigation
        public ICollection<TransactionItem> Items { get; set; } = new List<TransactionItem>();
    }
}
