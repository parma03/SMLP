using FluentValidation;
using Microsoft.Extensions.Options;
using Smlp.Shared.Audits.Options;
using Smlp.Shared.Common.Attributes;
using Smlp.Shared.Common.Constants;
using Smlp.Shared.Common.Requests;

namespace Smlp.Shared.Audits.Queries.GetAudits;

public class GetAuditsRequest : PaginatedListRequest
{
    [OpenApiContentType(ContentTypes.TextPlain)]
    public DateTime? From { get; set; }

    [OpenApiContentType(ContentTypes.TextPlain)]
    public DateTime? To { get; set; }
}

public class GetAuditsRequestValidator : AbstractValidator<GetAuditsRequest>
{
    private readonly DateTime _fromTimestamp;
    private readonly DateTime _toTimestamp;

    public GetAuditsRequestValidator(IOptions<AuditOptions> auditOptions)
    {
        _fromTimestamp = auditOptions.Value.FilterMinimumCreated;
        _toTimestamp = auditOptions.Value.FilterMaximumCreated;

        Include(new PaginatedListRequestValidator());

        RuleFor(v => v.From)
            .InclusiveBetween(_fromTimestamp, _toTimestamp)
            .When(v => v.From is not null);

        When(v => v.To is not null, () =>
        {
            RuleFor(v => v.To).InclusiveBetween(_fromTimestamp, _toTimestamp);
            RuleFor(v => v.To).GreaterThanOrEqualTo(x => x.From);
        });
    }
}
