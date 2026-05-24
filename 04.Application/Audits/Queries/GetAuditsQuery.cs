using MediatR;
using Smlp.Application.Audits;
using Smlp.Shared.Audits.Queries.GetAudits;
using Smlp.Shared.Common.Responses;

namespace Smlp.Application.Audits.Queries;

public class GetAuditsQuery : IRequest<PaginatedListResponse<GetAuditsAudit>>
{
    public GetAuditsRequest Request { get; set; } = default!;
}

public class GetAuditsQueryHandler : IRequestHandler<GetAuditsQuery, PaginatedListResponse<GetAuditsAudit>>
{
    private readonly IAuditRepository _auditRepository;

    public GetAuditsQueryHandler(IAuditRepository auditRepository) => _auditRepository = auditRepository;

    public async Task<PaginatedListResponse<GetAuditsAudit>> Handle(GetAuditsQuery request, CancellationToken cancellationToken)
    {
        return await _auditRepository.GetAuditsAsync(request.Request, cancellationToken);
    }
}
