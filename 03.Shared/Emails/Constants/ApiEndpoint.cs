
namespace Smlp.Shared.Emails.Constants;
public static class ApiEndpoint
{
    public static class V1
    {
        public static class Emails
        {
            public const string Segment = $"{nameof(V1)}/{nameof(Emails)}";

            public static class RouteTemplateFor
            {
                public const string Id = "{Id:guid}";
                public const string List = nameof(List);
                public const string SendEmail = nameof(SendEmail);
            }
        }
    }
}
