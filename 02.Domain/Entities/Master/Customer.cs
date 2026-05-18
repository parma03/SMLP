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
        public bool IsActive { get; set; }
        public string? Notes { get; set; }
        public FileEntity? ProfilePict { get; set; }

        // Navigation
        public ICollection<Transaction.Transaction> Transactions { get; set; } = new List<Transaction.Transaction>();
        public ICollection<RedeemHistory> RedeemHistories { get; set; } = new List<RedeemHistory>();
    }
}
