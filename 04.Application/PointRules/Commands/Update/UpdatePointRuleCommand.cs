using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Smlp.Application.Common.Exceptions;
using Smlp.Application.Services.Persistence;
using Smlp.Domain.Entities.Master;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Master.PointRules.Commands.Update;

namespace Smlp.Application.PointRules.Commands.Update;

public class UpdatePointRuleCommand : IRequest<CommonUpdateResponse>
{
    public Guid Id { get; set; }
    public UpdatePointRuleRequest Request { get; set; } = default!;
}

public class UpdatePointRuleCommandValidator : AbstractValidator<UpdatePointRuleCommand>
{
    public UpdatePointRuleCommandValidator()
    {
        RuleFor(v => v.Request).NotNull();
        RuleFor(v => v.Request.Name)
            .NotEmpty().WithMessage("Nama aturan wajib diisi.")
            .MaximumLength(100).WithMessage("Nama aturan maksimal 100 karakter.");
        RuleFor(v => v.Request.SpendAmount)
            .GreaterThanOrEqualTo(1000).WithMessage("Minimal belanja adalah Rp 1.000.")
            .LessThanOrEqualTo(10_000_000).WithMessage("Maksimal belanja adalah Rp 10.000.000.");
        RuleFor(v => v.Request.PointEarned)
            .GreaterThanOrEqualTo(1).WithMessage("Poin yang diperoleh minimal 1.")
            .LessThanOrEqualTo(1000).WithMessage("Poin yang diperoleh maksimal 1.000.");
        RuleFor(v => v.Request.BonusMultiplier)
            .GreaterThanOrEqualTo(1.0m).WithMessage("Bonus multiplier minimal 1.0x.")
            .LessThanOrEqualTo(10.0m).WithMessage("Bonus multiplier maksimal 10.0x.")
            .When(v => v.Request.BonusMultiplier.HasValue);
        RuleFor(v => v.Request.ValidUntil)
            .GreaterThan(v => v.Request.ValidFrom).WithMessage("Tanggal berakhir harus setelah tanggal mulai.")
            .When(v => v.Request.ValidFrom.HasValue && v.Request.ValidUntil.HasValue);
    }
}

public class UpdatePointRuleCommandHandler : IRequestHandler<UpdatePointRuleCommand, CommonUpdateResponse>
{
    private readonly ISmlpDbContext _context;

    public UpdatePointRuleCommandHandler(ISmlpDbContext context) => _context = context;

    public async Task<CommonUpdateResponse> Handle(UpdatePointRuleCommand request, CancellationToken cancellationToken)
    {
        var rule = await _context.PointRules
            .FirstOrDefaultAsync(r => r.Id == request.Id && !r.IsDeleted, cancellationToken)
            ?? throw new NotFoundException(nameof(PointRule), request.Id);

        var req = request.Request;
        rule.Name = req.Name;
        rule.SpendAmount = req.SpendAmount;
        rule.PointEarned = req.PointEarned;
        rule.BonusMultiplier = req.BonusMultiplier;
        rule.ValidFrom = req.ValidFrom;
        rule.ValidUntil = req.ValidUntil;
        rule.IsActive = req.IsActive;
        rule.Notes = req.Notes;

        await _context.SaveChangesAsync(this, cancellationToken);
        return new CommonUpdateResponse { Id = rule.Id };
    }
}
