using Smlp.Base.User.Enums;
using Smlp.Shared.Common.Attributes;
using Smlp.Shared.Common.Constants;

namespace Smlp.Shared.Master.Users.Queries
{
    public class GetUserRequest
    {
        [OpenApiContentType(ContentTypes.TextPlain)]
        public Guid? Id { get; set; }

        [OpenApiContentType(ContentTypes.TextPlain)]
        public string? Username { get; set; }

        [OpenApiContentType(ContentTypes.TextPlain)]
        public string? DisplayName { get; set; }

        [OpenApiContentType(ContentTypes.TextPlain)]
        public UserRole? Role { get; set; }

        [OpenApiContentType(ContentTypes.TextPlain)]
        public string? PhoneNumber { get; set; }

        [OpenApiContentType(ContentTypes.TextPlain)]
        public string? EmailAddress { get; set; }

        [OpenApiContentType(ContentTypes.TextPlain)]
        public bool? IsActive { get; set; }
    }
}
