using Smlp.Base.Transaction.Enums;
using Smlp.Domain.Abstracts;
using Smlp.Domain.Entities.Master;

namespace Smlp.Domain.Entities.Transaction
{
    public class PointAdjustment : AuditableEntity
    {
        public Guid CustomerId { get; set; }
        public Customer Customer { get; set; } = default!;
        public int Points { get; set; }
        public AdjustmentType AdjustmentType { get; set; }
        public string Reason { get; set; } = default!;
        public string ApprovedBy { get; set; } = default!;
        public DateTimeOffset AdjustmentDate { get; set; }
    }
}
