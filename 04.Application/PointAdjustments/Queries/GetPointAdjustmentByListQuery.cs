using MediatR;
using Microsoft.EntityFrameworkCore;
using Smlp.Application.Common.Exceptions;
using Smlp.Application.Services.Persistence;
using Smlp.Domain.Entities.Transaction;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Transaction.PointAdjustments.Queries;

namespace Smlp.Application.PointAdjustments.Queries;

public class GetPointAdjustmentByListQuery : IRequest<PaginatedListResponse<GetPointAdjustmentResponse>>
{
    public GetPointAdjustmentRequest Request { get; set; } = default!;
}

public class GetPointAdjustmentByListQueryHandler : IRequestHandler<GetPointAdjustmentByListQuery, PaginatedListResponse<GetPointAdjustmentResponse>>
{
    private readonly ISmlpDbContext _context;

    public GetPointAdjustmentByListQueryHandler(ISmlpDbContext context) => _context = context;

    public async Task<PaginatedListResponse<GetPointAdjustmentResponse>> Handle(GetPointAdjustmentByListQuery request, CancellationToken cancellationToken)
    {
        var req = request.Request;
        var query = _context.PointAdjustments
            .Include(a => a.Customer)
            .Where(a => !a.IsDeleted);

        if (req.CustomerId.HasValue)
            query = query.Where(a => a.CustomerId == req.CustomerId);

        if (req.AdjustmentType.HasValue)
            query = query.Where(a => a.AdjustmentType == req.AdjustmentType);

        if (req.DateFrom.HasValue)
            query = query.Where(a => a.AdjustmentDate >= req.DateFrom);

        if (req.DateTo.HasValue)
            query = query.Where(a => a.AdjustmentDate <= req.DateTo);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(a => a.AdjustmentDate)
            .Skip((req.PageNumber - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(a => new GetPointAdjustmentResponse
            {
                Id = a.Id,
                CustomerId = a.CustomerId,
                CustomerName = a.Customer.FullName,
                CustomerMemberCode = a.Customer.MemberCode,
                Points = a.Points,
                AdjustmentType = a.AdjustmentType,
                Reason = a.Reason,
                ApprovedBy = a.ApprovedBy,
                AdjustmentDate = a.AdjustmentDate
            })
            .ToListAsync(cancellationToken);

        return new PaginatedListResponse<GetPointAdjustmentResponse> { Items = items, TotalCount = total };
    }
}
