using Smlp.Base.Customer.Enums;
using Smlp.Domain.Abstracts;
using Smlp.Domain.Entities.Transaction;

namespace Smlp.Domain.Entities.Master
{
    public class Customer : AuditableEntity
    {
        public string MemberCode { get; set; } = default!;
        public string FullName { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;
        public string? EmailAddress { get; set; }
        public DateOnly? BirthDate { get; set; }
        public Gender? Gender { get; set; }
        public string? Address { get; set; }
        public CustomerTier Tier { get; set; }
        public int TotalPoints { get; set; }
        public int LifetimePoints { get; set; }
        public decimal TotalSpend { get; set; }
        public DateTimeOffset? LastTransactionDate { get; set; }
        public string? Notes { get; set; }

        // FK
        public Guid? UserId { get; set; }

        // Navigation
        public User? User { get; set; }
        public ICollection<Transaksi> Transactions { get; set; } = new List<Transaksi>();
        public ICollection<RedeemHistory> RedeemHistories { get; set; } = new List<RedeemHistory>();
    }
}
