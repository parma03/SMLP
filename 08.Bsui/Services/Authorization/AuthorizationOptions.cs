using Smlp.Shared.Services.Authorization.Constants;

namespace Smlp.Bsui.Services.Authorization;

public class AuthorizationOptions
{
    public const string SectionKey = nameof(Authorization);

    public string Provider { get; set; } = AuthorizationProvider.None;
}
