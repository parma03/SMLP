using MediatR;
using Microsoft.EntityFrameworkCore;
using Smlp.Application.Common.Exceptions;
using Smlp.Application.Services.Persistence;
using Smlp.Domain.Entities.Master;
using Smlp.Shared.Master.PointRules.Queries;

namespace Smlp.Application.PointRules.Queries;

public class GetPointRuleByIdQuery : IRequest<GetPointRuleResponse>
{
    public Guid Id { get; set; }
}

public class GetPointRuleByIdQueryHandler : IRequestHandler<GetPointRuleByIdQuery, GetPointRuleResponse>
{
    private readonly ISmlpDbContext _context;

    public GetPointRuleByIdQueryHandler(ISmlpDbContext context) => _context = context;

    public async Task<GetPointRuleResponse> Handle(GetPointRuleByIdQuery request, CancellationToken cancellationToken)
    {
        var rule = await _context.PointRules
            .Where(r => r.Id == request.Id && !r.IsDeleted)
            .Select(r => new GetPointRuleResponse
            {
                Id = r.Id,
                Name = r.Name,
                SpendAmount = r.SpendAmount,
                PointEarned = r.PointEarned,
                BonusMultiplier = r.BonusMultiplier,
                ValidFrom = r.ValidFrom,
                ValidUntil = r.ValidUntil,
                IsActive = r.IsActive,
                Notes = r.Notes
            })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(nameof(PointRule), request.Id);

        return rule;
    }
}
