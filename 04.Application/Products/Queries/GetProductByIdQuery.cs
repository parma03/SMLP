using MediatR;
using Microsoft.EntityFrameworkCore;
using Smlp.Application.Common.Exceptions;
using Smlp.Application.Services.Persistence;
using Smlp.Shared.Master.Products.Queries;

namespace Smlp.Application.Products.Queries;

public class GetProductByIdQuery : IRequest<GetProductResponse>
{
    public Guid Id { get; set; }
}

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, GetProductResponse>
{
    private readonly ISmlpDbContext _context;
    public GetProductByIdQueryHandler(ISmlpDbContext context) => _context = context;

    public async Task<GetProductResponse> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .Include(p => p.ProductCategory)
            .FirstOrDefaultAsync(p => p.Id == request.Id && !p.IsDeleted, cancellationToken)
            ?? throw new NotFoundException("Product", request.Id);

        return new GetProductResponse
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            IsActive = product.IsActive,
            Stock = product.Stock,
            CategoryId = product.ProductId,
            CategoryName = product.ProductCategory?.Name,
        };
    }
}
