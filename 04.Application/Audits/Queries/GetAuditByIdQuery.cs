using MediatR;
using Smlp.Application.Audits;
using Smlp.Application.Common.Exceptions;
using Smlp.Shared.Audits.Queries.GetAudit;

namespace Smlp.Application.Audits.Queries;

public class GetAuditByIdQuery : IRequest<GetAuditResponse>
{
    public Guid Id { get; set; }
}

public class GetAuditByIdQueryHandler : IRequestHandler<GetAuditByIdQuery, GetAuditResponse>
{
    private readonly IAuditRepository _auditRepository;

    public GetAuditByIdQueryHandler(IAuditRepository auditRepository) => _auditRepository = auditRepository;

    public async Task<GetAuditResponse> Handle(GetAuditByIdQuery request, CancellationToken cancellationToken)
    {
        var a = await _auditRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Audit", request.Id);

        return new GetAuditResponse
        {
            Id = a.Id,
            Created = a.CreatedAt,
            CreatedBy = a.CreatedBy,
            TableName = a.TableName,
            EntityName = a.EntityName,
            ActionType = a.ActionType,
            ActionName = a.ActionName,
            EntityId = a.EntityId,
            OldValues = a.OldValues,
            NewValues = a.NewValues,
            ClientApplicationId = a.ClientApplicationId,
            FromIpAddress = a.FromIpAddress,
            FromGeolocation = a.FromGeolocation,
        };
    }
}
