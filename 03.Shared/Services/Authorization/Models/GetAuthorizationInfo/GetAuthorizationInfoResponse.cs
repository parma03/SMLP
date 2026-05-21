namespace Smlp.Shared.Services.Authorization.Models.GetAuthorizationInfo;

public class GetAuthorizationInfoResponse
{
    public IList<AuthorizationRoleDto> Roles { get; set; } = new List<AuthorizationRoleDto>();
}

public class AuthorizationRoleDto
{
    public string Name { get; set; } = default!;
    public IList<string> Permissions { get; set; } = new List<string>();
}
