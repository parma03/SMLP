using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Smlp.Application.Common.Exceptions;
using Smlp.Application.Services.Persistence;
using Smlp.Base.Customer.Enums;
using Smlp.Domain.Entities.Master;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Master.Customers.Commands.Create;

namespace Smlp.Application.Customers.Commands.Create;

public class CreateCustomerCommand : IRequest<CommonCreateResponse>
{
    public CreateCustomerRequest Request { get; set; } = default!;
}

public class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(v => v.Request)
            .NotNull()
            .SetValidator(new CreateCustomerRequestValidator());
    }
}

public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, CommonCreateResponse>
{
    private readonly ISmlpDbContext _context;

    public CreateCustomerCommandHandler(ISmlpDbContext context) => _context = context;

    public async Task<CommonCreateResponse> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var req = request.Request;

        var exists = await _context.Customers.AnyAsync(c => c.PhoneNumber == req.PhoneNumber && !c.IsDeleted, cancellationToken);
        if (exists)
            throw new AlreadyExistsExceptions(nameof(Customer), req.PhoneNumber);

        var memberCode = await GenerateMemberCodeAsync(cancellationToken);

        var customer = new Customer
        {
            MemberCode = memberCode,
            FullName = req.FullName,
            PhoneNumber = req.PhoneNumber,
            EmailAddress = req.EmailAddress,
            BirthDate = req.BirthDate,
            Gender = req.Gender,
            Address = req.Address,
            Notes = req.Notes,
            Tier = CustomerTier.Bronze,
            TotalPoints = 0,
            LifetimePoints = 0,
            TotalSpend = 0
        };

        await _context.Customers.AddAsync(customer, cancellationToken);
        await _context.SaveChangesAsync(this, cancellationToken);

        return new CommonCreateResponse { Id = customer.Id };
    }

    private async Task<string> GenerateMemberCodeAsync(CancellationToken ct)
    {
        var count = await _context.Customers.CountAsync(ct);
        return $"MBR-{count + 1:D5}";
    }
}
