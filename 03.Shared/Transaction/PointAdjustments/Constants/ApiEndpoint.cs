namespace Smlp.Shared.Transaction.PointAdjustments.Constants;

public static class ApiEndpoint
{
    public static class V1
    {
        public static class PointAdjustments
        {
            public const string Segment = $"{nameof(V1)}/{nameof(PointAdjustments)}";

            public static class RouteTemplateFor
            {
                public const string GetById = "{id:guid}";
            }
        }
    }
}
