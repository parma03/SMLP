using MediatR;
using Microsoft.EntityFrameworkCore;
using Smlp.Application.Common.Exceptions;
using Smlp.Application.Services.Persistence;
using Smlp.Domain.Entities.Master;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Master.Rewards.Commands.Update;

namespace Smlp.Application.Rewards.Commands.Update;

public class UpdateRewardCommand : IRequest<CommonUpdateResponse>
{
    public Guid Id { get; set; }
    public UpdateRewardRequest Request { get; set; } = default!;
}

public class UpdateRewardCommandHandler : IRequestHandler<UpdateRewardCommand, CommonUpdateResponse>
{
    private readonly ISmlpDbContext _context;

    public UpdateRewardCommandHandler(ISmlpDbContext context) => _context = context;

    public async Task<CommonUpdateResponse> Handle(UpdateRewardCommand request, CancellationToken cancellationToken)
    {
        var reward = await _context.Rewards
            .FirstOrDefaultAsync(r => r.Id == request.Id && !r.IsDeleted, cancellationToken)
            ?? throw new NotFoundException(nameof(Reward), request.Id);

        var req = request.Request;
        reward.Name = req.Name;
        reward.Description = req.Description;
        reward.Type = req.Type;
        reward.PointCost = req.PointCost;
        reward.DiscountAmount = req.DiscountAmount;
        reward.DiscountPercent = req.DiscountPercent;
        reward.FreeItemName = req.FreeItemName;
        reward.StockLimit = req.StockLimit;
        reward.ValidFrom = req.ValidFrom;
        reward.ValidUntil = req.ValidUntil;
        reward.IsActive = req.IsActive;
        reward.MinimumTier = req.MinimumTier;

        await _context.SaveChangesAsync(this, cancellationToken);

        return new CommonUpdateResponse { Id = reward.Id };
    }
}
