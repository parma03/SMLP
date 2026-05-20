namespace Smlp.Shared.Master.Rewards.Constants;

public static class ApiEndpoint
{
    public static class V1
    {
        public static class Rewards
        {
            public const string Segment = $"{nameof(V1)}/{nameof(Rewards)}";

            public static class RouteTemplateFor
            {
                public const string GetById = "{id:guid}";
                public const string Update = "{id:guid}";
                public const string Delete = "{id:guid}";
                public const string ToggleActive = "{id:guid}/toggle-active";
            }
        }
    }
}
