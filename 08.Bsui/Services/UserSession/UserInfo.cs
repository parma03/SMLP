using GeolocationValueObject = Smlp.Base.ValueObjects.Geolocation;

namespace Smlp.Bsui.Services.UserSession;

/// <summary>
/// Holds transient, per-circuit information about the current user's session,
/// such as browser-reported geolocation, for use across components (e.g. telemetry).
/// </summary>
public class UserInfo
{
    public GeolocationValueObject? Geolocation { get; set; }
}
