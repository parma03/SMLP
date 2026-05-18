using Smlp.Domain.Abstracts;
using Smlp.Domain.Entities.Master;

namespace Smlp.Domain.Entities.Transaction
{
    public class TransactionItem : Entity
    {
        public Guid TransactionId { get; set; }
        public Transaction Transaction { get; set; } = default!;
        public Guid? ProductId { get; set; }
        public Product? Product { get; set; }
        public string ProductName { get; set; } = default!;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal Subtotal { get; set; }
    }
}
