namespace Smlp.Application.Services.Authorization;

public class AuthorizationOptions
{
    public const string SectionKey = nameof(Authorization);
    public string Provider { get; set; } = "None";
}
