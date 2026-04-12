using Smlp.Domain.Interfaces;

namespace Smlp.Domain.Abstracts
{
    public abstract class Entity : IHasKey, ICreatable
    {
        public Guid Id { get; set; }
        public DateTimeOffset Created { get; set; }
        public string CreatedBy { get; set; } = default!;
    }
}
