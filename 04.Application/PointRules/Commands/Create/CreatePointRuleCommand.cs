using FluentValidation;
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

public class CreatePointRuleCommandValidator : AbstractValidator<CreatePointRuleCommand>
{
    public CreatePointRuleCommandValidator()
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
            .GreaterThanOrEqualTo(1.0m).WithMessage("Bonus multiplier minimal 1.0×.")
            .LessThanOrEqualTo(10.0m).WithMessage("Bonus multiplier maksimal 10.0×.")
            .When(v => v.Request.BonusMultiplier.HasValue);
        RuleFor(v => v.Request.ValidUntil)
            .GreaterThan(v => v.Request.ValidFrom).WithMessage("Tanggal berakhir harus setelah tanggal mulai.")
            .When(v => v.Request.ValidFrom.HasValue && v.Request.ValidUntil.HasValue);
    }
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
