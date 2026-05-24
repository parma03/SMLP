using Smlp.Domain.Abstracts;

namespace Smlp.Domain.Entities.Master
{
    public class Product : AuditableEntity
    {
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
        public int Stock { get; set; } = 0;
        public FileEntity? ProductImage { get; set; }
        // FK
        public Guid? ProductId { get; set; }
        // Navigation
        public ProductCategory? ProductCategory { get; set; }
    }
}
