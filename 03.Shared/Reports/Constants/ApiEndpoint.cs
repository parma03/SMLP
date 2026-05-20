namespace Smlp.Shared.Reports.Constants;

public static class ApiEndpoint
{
    public static class V1
    {
        public static class Reports
        {
            public const string Segment = $"{nameof(V1)}/{nameof(Reports)}";

            public static class RouteTemplateFor
            {
                public const string Transactions = "transactions";
                public const string Redeems = "redeems";
                public const string Points = "points";
            }
        }
    }
}
