namespace Smlp.Shared.Master.Users.Constants
{
    public static class ApiEndpoint
    {
        public static class V1
        {
            public static class Users
            {
                public const string Segment = $"{nameof(V1)}/{nameof(Users)}";

                public static class RouteTemplateFor
                {
                    public const string User = nameof(User);
                    public const string GetUserById = "{id:guid}";
                    public const string UpdateUser = $"{nameof(User)}/UpdateUser/{{id:guid}}";
                    public const string DeleteUser = $"{nameof(User)}/DeleteUser/{{id:guid}}";
                    public const string CreateUser = nameof(CreateUser);
                }
            }
        }
    }

}
