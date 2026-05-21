namespace Smlp.Application.Services.Authentication;

public class AuthenticationOptions
{
    public const string SectionKey = nameof(Authentication);
    public string Provider { get; set; } = "None";
}
