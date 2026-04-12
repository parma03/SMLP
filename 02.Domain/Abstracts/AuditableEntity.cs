using Smlp.Domain.Interfaces;

namespace Smlp.Domain.Abstracts
{
    public abstract class AuditableEntity : Entity, IDeletable, IModifiable
    {
        public bool IsDeleted { get; set; }
        public DateTimeOffset? Modified { get; set; }
        public string? ModifiedBy { get; set; }
    }
}
