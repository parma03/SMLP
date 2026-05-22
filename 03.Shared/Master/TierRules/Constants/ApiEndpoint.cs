namespace Smlp.Shared.Master.TierRules.Constants;

public static class ApiEndpoint
{
    public static class V1
    {
        public static class TierRules
        {
            public const string Segment = $"{nameof(V1)}/{nameof(TierRules)}";

            public static class RouteTemplateFor
            {
                public const string GetById = "{id:guid}";
                public const string Update = "{id:guid}";
                public const string Delete = "{id:guid}";
            }
        }
    }
}
