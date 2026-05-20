using MediatR;
using Microsoft.EntityFrameworkCore;
using Smlp.Application.Common.Exceptions;
using Smlp.Application.Services.Persistence;
using Smlp.Domain.Entities.Master;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Master.Rewards.Commands.Create;

namespace Smlp.Application.Rewards.Commands.Create;

public class CreateRewardCommand : IRequest<CommonCreateResponse>
{
    public CreateRewardRequest Request { get; set; } = default!;
}

public class CreateRewardCommandHandler : IRequestHandler<CreateRewardCommand, CommonCreateResponse>
{
    private readonly ISmlpDbContext _context;

    public CreateRewardCommandHandler(ISmlpDbContext context) => _context = context;

    public async Task<CommonCreateResponse> Handle(CreateRewardCommand request, CancellationToken cancellationToken)
    {
        var req = request.Request;

        var exists = await _context.Rewards.AnyAsync(r => r.Name == req.Name && !r.IsDeleted, cancellationToken);
        if (exists)
            throw new AlreadyExistsExceptions(nameof(Reward), req.Name);

        var reward = new Reward
        {
            Name = req.Name,
            Description = req.Description,
            Type = req.Type,
            PointCost = req.PointCost,
            DiscountAmount = req.DiscountAmount,
            DiscountPercent = req.DiscountPercent,
            FreeItemName = req.FreeItemName,
            StockLimit = req.StockLimit,
            RedeemedCount = 0,
            ValidFrom = req.ValidFrom,
            ValidUntil = req.ValidUntil,
            IsActive = req.IsActive,
            MinimumTier = req.MinimumTier,
        };

        await _context.Rewards.AddAsync(reward, cancellationToken);
        await _context.SaveChangesAsync(this, cancellationToken);

        return new CommonCreateResponse { Id = reward.Id };
    }
}
