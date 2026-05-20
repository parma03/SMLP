using MediatR;
using Microsoft.EntityFrameworkCore;
using Smlp.Application.Common.Exceptions;
using Smlp.Application.Services.CurrentUser;
using Smlp.Application.Services.Persistence;
using Smlp.Domain.Entities.Master;
using Smlp.Domain.Entities.Transaction;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Transaction.PointAdjustments.Commands.Create;

namespace Smlp.Application.PointAdjustments.Commands.Create;

public class CreatePointAdjustmentCommand : IRequest<CommonCreateResponse>
{
    public CreatePointAdjustmentRequest Request { get; set; } = default!;
}

public class CreatePointAdjustmentCommandHandler : IRequestHandler<CreatePointAdjustmentCommand, CommonCreateResponse>
{
    private readonly ISmlpDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreatePointAdjustmentCommandHandler(ISmlpDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<CommonCreateResponse> Handle(CreatePointAdjustmentCommand request, CancellationToken cancellationToken)
    {
        var req = request.Request;

        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == req.CustomerId && !c.IsDeleted, cancellationToken)
            ?? throw new NotFoundException(nameof(Customer), req.CustomerId);

        // Validate sufficient points for deduction
        if (req.Points < 0 && customer.TotalPoints < Math.Abs(req.Points))
            throw new InvalidOperationException($"Poin pelanggan tidak cukup. Saldo: {customer.TotalPoints}, diminta: {Math.Abs(req.Points)}");

        var adjustment = new PointAdjustment
        {
            CustomerId = customer.Id,
            Points = req.Points,
            AdjustmentType = req.AdjustmentType,
            Reason = req.Reason,
            ApprovedBy = _currentUser.Username ?? "System",
            AdjustmentDate = DateTimeOffset.UtcNow
        };

        await _context.PointAdjustments.AddAsync(adjustment, cancellationToken);

        // Update customer points
        customer.TotalPoints += req.Points;
        if (req.Points > 0)
            customer.LifetimePoints += req.Points;

        await _context.SaveChangesAsync(this, cancellationToken);

        return new CommonCreateResponse { Id = adjustment.Id };
    }
}
