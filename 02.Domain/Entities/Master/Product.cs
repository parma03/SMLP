using Smlp.Base.Product.Enums;
using Smlp.Domain.Abstracts;

namespace Smlp.Domain.Entities.Master
{
    public class Product : AuditableEntity
    {
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public ProductCategory Category { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
        public FileEntity? ProductImage { get; set; }
    }
}
