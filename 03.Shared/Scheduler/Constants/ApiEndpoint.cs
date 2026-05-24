namespace Smlp.Shared.Scheduler.Constants;

public static class ApiEndpoint
{
    public static class V1
    {
        public static class Scheduler
        {
            public const string Segment = $"{nameof(V1)}/{nameof(Scheduler)}";

            public static class RouteTemplateFor
            {
                public const string RecurringJobs = "recurring-jobs";
                public const string JobStats = "job-stats";
                public const string TriggerJob = "recurring-jobs/{jobId}/trigger";
            }
        }
    }
}
