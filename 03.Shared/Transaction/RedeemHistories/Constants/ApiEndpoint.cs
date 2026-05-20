namespace Smlp.Shared.Transaction.RedeemHistories.Constants;

public static class ApiEndpoint
{
    public static class V1
    {
        public static class RedeemHistories
        {
            public const string Segment = $"{nameof(V1)}/{nameof(RedeemHistories)}";

            public static class RouteTemplateFor
            {
                public const string GetById = "{id:guid}";
                public const string Verify = "{id:guid}/verify";
            }
        }
    }
}
