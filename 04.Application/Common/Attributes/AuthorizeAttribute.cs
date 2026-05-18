namespace Smlp.Application.Common.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public class AuthorizeAttribute : Attribute
{
    public string Policy { get; set; } = default!;
}
