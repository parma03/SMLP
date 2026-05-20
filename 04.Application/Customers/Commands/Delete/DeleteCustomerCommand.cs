using MediatR;
using Microsoft.EntityFrameworkCore;
using Smlp.Application.Services.Persistence;
using Smlp.Application.Common.Exceptions;
using Smlp.Domain.Entities.Master;

namespace Smlp.Application.Customers.Commands.Delete;

public class DeleteCustomerCommand : IRequest
{
    public Guid Id { get; set; }
}

public class DeleteCustomerCommandHandler : IRequestHandler<DeleteCustomerCommand>
{
    private readonly ISmlpDbContext _context;

    public DeleteCustomerCommandHandler(ISmlpDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == request.Id && !c.IsDeleted, cancellationToken)
            ?? throw new NotFoundException(nameof(Customer), request.Id);

        _context.Customers.Remove(customer);

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
