using MediatR;
using Microsoft.EntityFrameworkCore;
using Smlp.Application.Common.Exceptions;
using Smlp.Application.Services.Persistence;
using Smlp.Domain.Entities.Transaction;
using Smlp.Shared.Transaction.PointAdjustments.Queries;

namespace Smlp.Application.PointAdjustments.Queries;

public class GetPointAdjustmentByIdQuery : IRequest<GetPointAdjustmentResponse>
{
    public Guid Id { get; set; }
}

public class GetPointAdjustmentByIdQueryHandler : IRequestHandler<GetPointAdjustmentByIdQuery, GetPointAdjustmentResponse>
{
    private readonly ISmlpDbContext _context;

    public GetPointAdjustmentByIdQueryHandler(ISmlpDbContext context) => _context = context;

    public async Task<GetPointAdjustmentResponse> Handle(GetPointAdjustmentByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.PointAdjustments
            .Include(a => a.Customer)
            .Where(a => a.Id == request.Id && !a.IsDeleted)
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
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(nameof(PointAdjustment), request.Id);
    }
}
