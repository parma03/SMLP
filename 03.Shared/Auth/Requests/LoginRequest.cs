using FluentValidation;
using Smlp.Shared.Common.Attributes;
using Smlp.Shared.Common.Constants;

namespace Smlp.Shared.Auth.Requests;

public class LoginRequest
{
    [OpenApiContentType(ContentTypes.TextPlain)]
    public string Username { get; set; } = default!;

    [OpenApiContentType(ContentTypes.TextPlain)]
    public string Password { get; set; } = default!;
}
public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(v => v.Username)
           .NotEmpty();
        RuleFor(v => v.Password)
           .NotEmpty();
    }
}
