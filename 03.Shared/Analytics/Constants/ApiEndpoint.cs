namespace Smlp.Shared.Analytics.Constants;

public static class ApiEndpoint
{
    public static class V1
    {
        public static class Analytics
        {
            public const string Segment = $"{nameof(V1)}/{nameof(Analytics)}";

            public static class RouteTemplateFor
            {
                public const string Dashboard = "dashboard";
                public const string Revenue = "revenue";
                public const string CustomerInsights = "customer-insights";
                public const string LoyaltyAnalysis = "loyalty-analysis";
            }
        }
    }
}
