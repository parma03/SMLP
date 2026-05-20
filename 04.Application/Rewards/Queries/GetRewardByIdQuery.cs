using MediatR;
using Microsoft.EntityFrameworkCore;
using Smlp.Application.Common.Exceptions;
using Smlp.Application.Services.Persistence;
using Smlp.Domain.Entities.Master;
using Smlp.Shared.Master.Rewards.Queries;

namespace Smlp.Application.Rewards.Queries;

public class GetRewardByIdQuery : IRequest<GetRewardResponse>
{
    public Guid Id { get; set; }
}

public class GetRewardByIdQueryHandler : IRequestHandler<GetRewardByIdQuery, GetRewardResponse>
{
    private readonly ISmlpDbContext _context;

    public GetRewardByIdQueryHandler(ISmlpDbContext context) => _context = context;

    public async Task<GetRewardResponse> Handle(GetRewardByIdQuery request, CancellationToken cancellationToken)
    {
        var r = await _context.Rewards
            .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken)
            ?? throw new NotFoundException(nameof(Reward), request.Id);

        return new GetRewardResponse
        {
            Id = r.Id,
            Name = r.Name,
            Description = r.Description,
            Type = r.Type,
            PointCost = r.PointCost,
            DiscountAmount = r.DiscountAmount,
            DiscountPercent = r.DiscountPercent,
            FreeItemName = r.FreeItemName,
            StockLimit = r.StockLimit,
            RedeemedCount = r.RedeemedCount,
            ValidFrom = r.ValidFrom,
            ValidUntil = r.ValidUntil,
            IsActive = r.IsActive,
            MinimumTier = r.MinimumTier,
        };
    }
}
