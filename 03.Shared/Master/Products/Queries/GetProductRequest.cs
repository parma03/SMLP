namespace Smlp.Shared.Master.Products.Queries;

public class GetProductRequest
{
    public string? Search { get; set; }
    public bool? IsActive { get; set; }
    public Guid? CategoryId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
