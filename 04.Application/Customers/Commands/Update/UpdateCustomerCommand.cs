using MediatR;
using Microsoft.EntityFrameworkCore;
using Smlp.Application.Services.Persistence;
using Smlp.Application.Common.Exceptions;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Master.Customers.Commands.Update;
using Smlp.Domain.Entities.Master;

namespace Smlp.Application.Customers.Commands.Update;

public class UpdateCustomerCommand : IRequest<CommonUpdateResponse>
{
    public Guid Id { get; set; }
    public UpdateCustomerRequest Request { get; set; } = default!;
}

public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, CommonUpdateResponse>
{
    private readonly ISmlpDbContext _context;

    public UpdateCustomerCommandHandler(ISmlpDbContext context) => _context = context;

    public async Task<CommonUpdateResponse> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == request.Id && !c.IsDeleted, cancellationToken)
            ?? throw new NotFoundException(nameof(Customer), request.Id);

        var req = request.Request;
        customer.FullName = req.FullName;
        customer.PhoneNumber = req.PhoneNumber;
        customer.EmailAddress = req.EmailAddress;
        customer.BirthDate = req.BirthDate;
        customer.Gender = req.Gender;
        customer.Address = req.Address;
        customer.Notes = req.Notes;

        await _context.SaveChangesAsync(this, cancellationToken);
        return new CommonUpdateResponse { Id = customer.Id };
    }
}
