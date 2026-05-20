namespace Smlp.Shared.Master.PointRules.Constants;

public static class ApiEndpoint
{
    public static class V1
    {
        public static class PointRules
        {
            public const string Segment = $"{nameof(V1)}/{nameof(PointRules)}";

            public static class RouteTemplateFor
            {
                public const string GetById = "{id:guid}";
                public const string Update = "{id:guid}";
                public const string Delete = "{id:guid}";
            }
        }
    }
}
