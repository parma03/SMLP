namespace Smlp.Shared.Services.Authorization.Models.GetAuthorizationInfo;

public class GetAuthorizationInfoResponse
{
    public IEnumerable<string> Permissions { get; set; } = Enumerable.Empty<string>();
    public IEnumerable<string> Roles { get; set; } = Enumerable.Empty<string>();
}
