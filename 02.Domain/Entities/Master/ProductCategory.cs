using Smlp.Domain.Abstracts;

namespace Smlp.Domain.Entities.Master
{
    public class ProductCategory : AuditableEntity
    {
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        // Navigation
        public Product? Product { get; set; }
    }
}
