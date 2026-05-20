using MediatR;
using Microsoft.EntityFrameworkCore;
using Smlp.Application.Common.Exceptions;
using Smlp.Application.Services.Persistence;
using Smlp.Domain.Entities.Master;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Master.PointRules.Commands.Create;

namespace Smlp.Application.PointRules.Commands.Create;

public class CreatePointRuleCommand : IRequest<CommonCreateResponse>
{
    public CreatePointRuleRequest Request { get; set; } = default!;
}

public class CreatePointRuleCommandHandler : IRequestHandler<CreatePointRuleCommand, CommonCreateResponse>
{
    private readonly ISmlpDbContext _context;

    public CreatePointRuleCommandHandler(ISmlpDbContext context) => _context = context;

    public async Task<CommonCreateResponse> Handle(CreatePointRuleCommand request, CancellationToken cancellationToken)
    {
        var req = request.Request;

        var exists = await _context.PointRules.AnyAsync(r => r.Name == req.Name && !r.IsDeleted, cancellationToken);
        if (exists)
            throw new AlreadyExistsExceptions(nameof(PointRule), req.Name);

        var rule = new PointRule
        {
            Name = req.Name,
            SpendAmount = req.SpendAmount,
            PointEarned = req.PointEarned,
            BonusMultiplier = req.BonusMultiplier,
            ValidFrom = req.ValidFrom,
            ValidUntil = req.ValidUntil,
            IsActive = req.IsActive,
            Notes = req.Notes
        };

        await _context.PointRules.AddAsync(rule, cancellationToken);
        await _context.SaveChangesAsync(this, cancellationToken);

        return new CommonCreateResponse { Id = rule.Id };
    }
}
