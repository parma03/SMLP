using Psmp.Shared.Services.Authentication.Constants;

namespace Psmp.Bsui.Services.Authentication;

public class AuthenticationOptions
{
    public const string SectionKey = nameof(Authentication);

    public int RefreshRateInSeconds { get; set; }
    public bool AllowGuest { get; set; }
    public string Provider { get; set; } = AuthenticationProvider.None;
}
