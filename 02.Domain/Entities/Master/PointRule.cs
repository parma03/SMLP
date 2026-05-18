using Smlp.Domain.Abstracts;

namespace Smlp.Domain.Entities.Master
{
    public class PointRule : AuditableEntity
    {
        public string Name { get; set; } = default!;
        public decimal SpendAmount { get; set; }
        public int PointEarned { get; set; }
        public decimal? BonusMultiplier { get; set; }
        public DateOnly? ValidFrom { get; set; }
        public DateOnly? ValidUntil { get; set; }
        public bool IsActive { get; set; }
        public string? Notes { get; set; }
    }
}
