namespace Smlp.Shared.Audits.Options;

public class AuditOptions
{
    public const string SectionKey = nameof(Audits);

    public int FilterMinimumYear { get; set; }
    public int FilterMaximumYear { get; set; }

    public DateTime FilterMinimumCreated => new(FilterMinimumYear, 1, 1, 0, 0, 0);
    public DateTime FilterMaximumCreated => new(FilterMaximumYear, 12, 31, 23, 59, 59);
}
