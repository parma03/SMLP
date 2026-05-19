/*
*/

namespace Smlp.Shared.Emails.Options;

public class EmailOptions
{
    public static readonly string SectionKey = $"Email:EmailBypass";

    public bool IsBypass { get; set; }
    public string ToEmail { get; set; } = default!;
    public string ToEmailNonBypass { get; set; } = default!;
}
