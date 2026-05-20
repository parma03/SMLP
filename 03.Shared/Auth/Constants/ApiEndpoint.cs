namespace Smlp.Shared.Auth.Constants
{
    public static class ApiEndpoint
    {
        public static class V1
        {
            public static class Auths
            {
                public const string Segment = $"{nameof(V1)}/{nameof(Auths)}";

                public static class RouteTemplateFor
                {
                    public const string Login = nameof(Login);
                    public const string Logout = nameof(Logout);
                    public const string RefreshToken = nameof(RefreshToken);
                }
            }
        }
    }
}
