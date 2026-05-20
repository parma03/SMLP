using Smlp.Shared.Common.Attributes;
using Smlp.Shared.Common.Constants;

namespace Smlp.Shared.Auth.Requests;

public class RefreshTokenRequest
{
    [OpenApiContentType(ContentTypes.TextPlain)]
    public string RefreshToken { get; set; } = default!;
}
