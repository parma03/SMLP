namespace Smlp.Infrastructure.BackgroundJob;

public class BackgroundJobOptions
{
    public const string SectionKey = nameof(BackgroundJob);

    public string Provider { get; set; } = BackgroundJobProvider.None;
}

public static class BackgroundJobProvider
{
    public const string None = nameof(None);
    public const string Hangfire = nameof(Hangfire);
}
