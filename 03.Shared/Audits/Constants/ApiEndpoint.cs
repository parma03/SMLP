namespace Smlp.Shared.Audits.Constants;

public static class ApiEndpoint
{
    public static class V1
    {
        public static class Audits
        {
            public const string Segment = $"{nameof(V1)}/{nameof(Audits)}";

            public static class RouteTemplateFor
            {
                public const string AuditId = "{auditId:guid}";
                public const string Export = nameof(Export);
            }
        }
    }
}
