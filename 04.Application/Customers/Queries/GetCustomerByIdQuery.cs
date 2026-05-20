using MediatR;
using Microsoft.EntityFrameworkCore;
using Smlp.Application.Services.Persistence;
using Smlp.Application.Common.Exceptions;
using Smlp.Base.Customer.Enums;
using Smlp.Shared.Master.Customers.Queries;
using Smlp.Domain.Entities.Master;

namespace Smlp.Application.Customers.Queries;

public class GetCustomerByIdQuery : IRequest<GetCustomerResponse>
{
    public Guid Id { get; set; }
}

public class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, GetCustomerResponse>
{
    private readonly ISmlpDbContext _context;

    public GetCustomerByIdQueryHandler(ISmlpDbContext context) => _context = context;

    public async Task<GetCustomerResponse> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers
            .Where(c => c.Id == request.Id && !c.IsDeleted)
            .Select(c => new GetCustomerResponse
            {
                Id = c.Id,
                MemberCode = c.MemberCode,
                FullName = c.FullName,
                PhoneNumber = c.PhoneNumber,
                EmailAddress = c.EmailAddress,
                BirthDate = c.BirthDate,
                Gender = c.Gender,
                Address = c.Address,
                Tier = c.Tier,
                Segment = GetSegment(c),
                TotalPoints = c.TotalPoints,
                LifetimePoints = c.LifetimePoints,
                TotalSpend = c.TotalSpend,
                LastTransactionDate = c.LastTransactionDate,
                Notes = c.Notes,
                CreatedAt = c.Created
            })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(nameof(Customer), request.Id);

        return customer;
    }

    private static CustomerSegment GetSegment(Customer c)
    {
        if (c.LastTransactionDate == null)
            return CustomerSegment.New;

        var daysSince = (DateTimeOffset.UtcNow - c.LastTransactionDate.Value).TotalDays;

        if (daysSince > 90)
            return CustomerSegment.Inactive;

        if (daysSince > 45)
            return CustomerSegment.AtRisk;

        if (c.LifetimePoints >= 500)
            return CustomerSegment.Loyal;

        return CustomerSegment.Active;
    }
}
