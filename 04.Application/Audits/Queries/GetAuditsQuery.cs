using MediatR;
using Microsoft.EntityFrameworkCore;
using Smlp.Application.Services.Persistence;
using Smlp.Shared.Audits.Queries.GetAudits;
using Smlp.Shared.Common.Responses;

namespace Smlp.Application.Audits.Queries;

public class GetAuditsQuery : IRequest<PaginatedListResponse<GetAuditsAudit>>
{
    public GetAuditsRequest Request { get; set; } = default!;
}

public class GetAuditsQueryHandler : IRequestHandler<GetAuditsQuery, PaginatedListResponse<GetAuditsAudit>>
{
    private readonly ISmlpDbContext _context;

    public GetAuditsQueryHandler(ISmlpDbContext context) => _context = context;

    public async Task<PaginatedListResponse<GetAuditsAudit>> Handle(GetAuditsQuery request, CancellationToken cancellationToken)
    {
        var req = request.Request;
        var query = _context.Audits.AsQueryable();

        if (!string.IsNullOrWhiteSpace(req.Search))
            query = query.Where(a =>
                a.EntityName.Contains(req.Search) ||
                a.ActionType.Contains(req.Search) ||
                a.ActionName.Contains(req.Search) ||
                a.FromIpAddress.Contains(req.Search));

        if (req.From.HasValue)
            query = query.Where(a => a.CreatedAt >= req.From.Value);

        if (req.To.HasValue)
            query = query.Where(a => a.CreatedAt <= req.To.Value);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((req.PageNumber - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(a => new GetAuditsAudit
            {
                Id = a.Id,
                Created = a.CreatedAt,
                CreatedBy = a.CreatedBy,
                EntityName = a.EntityName,
                EntityId = a.EntityId,
                ActionType = a.ActionType,
                FromIpAddress = a.FromIpAddress,
            })
            .ToListAsync(cancellationToken);

        return new PaginatedListResponse<GetAuditsAudit> { Items = items, TotalCount = total };
    }
}
