using Smlp.Base.ValueObjects;

namespace Smlp.Client.Services.UserInfo;

public class UserInfoService
{
    public string AccessToken { get; set; } = default!;
    public string? PositionId { get; set; }
    public string? IpAddress { get; set; }
    public Geolocation? Geolocation { get; set; }
}
