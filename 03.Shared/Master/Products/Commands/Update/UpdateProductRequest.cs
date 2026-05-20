namespace Smlp.Shared.Master.Products.Commands.Update;

public class UpdateProductRequest
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
    public Guid? CategoryId { get; set; }
}
