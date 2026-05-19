/*
*/

using Smlp.Shared.Common.Attributes;
using Smlp.Shared.Common.Constants;

namespace Smlp.Shared.Emails.Commands.SendEmail;

public class SendEmailRequest
{
    [OpenApiContentType(ContentTypes.TextPlain)]
    public Guid PostingId { get; set; }
}
