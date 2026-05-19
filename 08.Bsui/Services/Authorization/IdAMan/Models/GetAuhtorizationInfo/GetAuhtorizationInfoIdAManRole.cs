using Newtonsoft.Json;

namespace Psmp.Bsui.Services.Authorization.IdAMan.Models.GetAuhtorizationInfo;

public class GetAuhtorizationInfoIdAManRole
{
    [JsonProperty("name")]
    public string Name { get; set; } = default!;

    [JsonProperty("permissions")]
    public string[]? Permissions { get; set; }
}
