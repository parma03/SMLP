namespace Smlp.Shared.Transaction.Transactions.Constants;

public static class ApiEndpoint
{
    public static class V1
    {
        public static class Transactions
        {
            public const string Segment = $"{nameof(V1)}/{nameof(Transactions)}";

            public static class RouteTemplateFor
            {
                public const string GetById = "{id:guid}";
                public const string Cancel = "{id:guid}/cancel";
            }
        }
    }
}
