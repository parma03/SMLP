namespace Smlp.Shared.Master.Customers.Constants;

public static class ApiEndpoint
{
    public static class V1
    {
        public static class Customers
        {
            public const string Segment = $"{nameof(V1)}/{nameof(Customers)}";

            public static class RouteTemplateFor
            {
                public const string GetById = "{id:guid}";
                public const string Update = "{id:guid}";
                public const string Delete = "{id:guid}";
            }
        }
    }
}
