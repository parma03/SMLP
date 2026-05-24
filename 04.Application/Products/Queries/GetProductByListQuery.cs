using MediatR;
using Microsoft.EntityFrameworkCore;
using Smlp.Application.Services.Persistence;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Master.Products.Queries;

namespace Smlp.Application.Products.Queries;

public class GetProductByListQuery : IRequest<PaginatedListResponse<GetProductResponse>>
{
    public GetProductRequest Request { get; set; } = default!;
}

public class GetProductByListQueryHandler : IRequestHandler<GetProductByListQuery, PaginatedListResponse<GetProductResponse>>
{
    private readonly ISmlpDbContext _context;
    public GetProductByListQueryHandler(ISmlpDbContext context) => _context = context;

    public async Task<PaginatedListResponse<GetProductResponse>> Handle(GetProductByListQuery request, CancellationToken cancellationToken)
    {
        var req = request.Request;
        var query = _context.Products.Include(p => p.ProductCategory).AsQueryable();

        if (!string.IsNullOrWhiteSpace(req.Search))
            query = query.Where(p => p.Name.Contains(req.Search) || (p.Description != null && p.Description.Contains(req.Search)));

        if (req.IsActive.HasValue)
            query = query.Where(p => p.IsActive == req.IsActive.Value);

        if (req.CategoryId.HasValue)
            query = query.Where(p => p.ProductId == req.CategoryId.Value);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(p => p.Name)
            .Skip((req.PageNumber - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(p => new GetProductResponse
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                IsActive = p.IsActive,
                Stock = p.Stock,
                CategoryId = p.ProductId,
                CategoryName = p.ProductCategory != null ? p.ProductCategory.Name : null,
            })
            .ToListAsync(cancellationToken);

        return new PaginatedListResponse<GetProductResponse> { Items = items, TotalCount = total };
    }
}
