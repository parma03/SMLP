using Smlp.Shared.Common.Responses;

namespace Smlp.Shared.Master.Products.Queries;

public class GetProductResponse : Response
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
    public Guid? CategoryId { get; set; }
    public string? CategoryName { get; set; }
}
