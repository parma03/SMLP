using MediatR;
using Microsoft.EntityFrameworkCore;
using Smlp.Application.Services.Persistence;
using Smlp.Base.Customer.Enums;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Master.Customers.Queries;

namespace Smlp.Application.Customers.Queries;

public class GetCustomerByListQuery : IRequest<PaginatedListResponse<GetCustomerResponse>>
{
    public GetCustomerRequest Request { get; set; } = default!;
}

public class GetCustomerByListQueryHandler : IRequestHandler<GetCustomerByListQuery, PaginatedListResponse<GetCustomerResponse>>
{
    private readonly ISmlpDbContext _context;

    public GetCustomerByListQueryHandler(ISmlpDbContext context) => _context = context;

    public async Task<PaginatedListResponse<GetCustomerResponse>> Handle(GetCustomerByListQuery request, CancellationToken cancellationToken)
    {
        var req = request.Request;
        var query = _context.Customers.Where(c => !c.IsDeleted);

        if (!string.IsNullOrWhiteSpace(req.Search))
            query = query.Where(c => c.FullName.Contains(req.Search) ||
                                     c.PhoneNumber.Contains(req.Search) ||
                                     c.MemberCode.Contains(req.Search));
        if (req.Tier.HasValue)
            query = query.Where(c => c.Tier == req.Tier);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(c => c.LastTransactionDate ?? c.Created)
            .Skip((req.PageNumber - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(c => new GetCustomerResponse
            {
                Id = c.Id,
                MemberCode = c.MemberCode,
                FullName = c.FullName,
                PhoneNumber = c.PhoneNumber,
                EmailAddress = c.EmailAddress,
                Tier = c.Tier,
                TotalPoints = c.TotalPoints,
                LifetimePoints = c.LifetimePoints,
                TotalSpend = c.TotalSpend,
                LastTransactionDate = c.LastTransactionDate,
                CreatedAt = c.Created
            })
            .ToListAsync(cancellationToken);

        return new PaginatedListResponse<GetCustomerResponse> { Items = items, TotalCount = total };
    }
}
