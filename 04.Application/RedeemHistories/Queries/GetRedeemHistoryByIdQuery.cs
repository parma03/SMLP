using MediatR;
using Microsoft.EntityFrameworkCore;
using Smlp.Application.Common.Exceptions;
using Smlp.Application.Services.Persistence;
using Smlp.Shared.Transaction.RedeemHistories.Queries;

namespace Smlp.Application.RedeemHistories.Queries;

public class GetRedeemHistoryByIdQuery : IRequest<GetRedeemHistoryResponse>
{
    public Guid Id { get; set; }
}

public class GetRedeemHistoryByIdQueryHandler : IRequestHandler<GetRedeemHistoryByIdQuery, GetRedeemHistoryResponse>
{
    private readonly ISmlpDbContext _context;

    public GetRedeemHistoryByIdQueryHandler(ISmlpDbContext context) => _context = context;

    public async Task<GetRedeemHistoryResponse> Handle(GetRedeemHistoryByIdQuery request, CancellationToken cancellationToken)
    {
        var item = await _context.RedeemHistories
            .Include(r => r.Customer)
            .Include(r => r.Reward)
            .Where(r => r.Id == request.Id && !r.IsDeleted)
            .Select(r => new GetRedeemHistoryResponse
            {
                Id = r.Id,
                RedeemCode = r.RedeemCode,
                CustomerId = r.CustomerId,
                CustomerName = r.Customer.FullName,
                CustomerMemberCode = r.Customer.MemberCode,
                RewardId = r.RewardId,
                RewardName = r.RewardNameSnapshot,
                RedeemDate = r.RedeemDate,
                PointsUsed = r.PointsUsed,
                Status = r.Status,
                VerifiedBy = r.VerifiedBy,
                VerifiedAt = r.VerifiedAt,
                Notes = r.Notes,
                PointCostSnapshot = r.PointCostSnapshot,
            })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("RedeemHistory", request.Id);

        return item;
    }
}
