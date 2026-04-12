/*
* Pertamina Scholarship Management Portal
* Copyright (c) 2024-2025 PT. Pertamina Persero
*/

using Smlp.Shared.Common.Attributes;
using Smlp.Shared.Common.Constants;

namespace Smlp.Shared.Emails.Commands.SendEmail;

public class SendEmailRequest
{
    [OpenApiContentType(ContentTypes.TextPlain)]
    public Guid PostingId { get; set; }
}
