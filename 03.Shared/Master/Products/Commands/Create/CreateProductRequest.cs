namespace Smlp.Shared.Master.Products.Commands.Create;

public class CreateProductRequest
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid? CategoryId { get; set; }
}
