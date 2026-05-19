using Psmp.Shared.Services.Authorization.Constants;

namespace Psmp.Bsui.Services.Authorization;

public class AuthorizationOptions
{
    public const string SectionKey = nameof(Authorization);

    public string Provider { get; set; } = AuthorizationProvider.None;
}
