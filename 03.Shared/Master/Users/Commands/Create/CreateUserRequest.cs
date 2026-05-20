using Smlp.Base.User.Enums;
using Smlp.Domain.Abstracts;
using Smlp.Shared.Common.Attributes;
using Smlp.Shared.Common.Constants;

namespace Smlp.Shared.Master.Users.Commands.Create
{
    public class CreateUserRequest
    {
        [OpenApiContentType(ContentTypes.TextPlain)]
        public string Username { get; set; } = default!;

        [OpenApiContentType(ContentTypes.TextPlain)]
        public string DisplayName { get; set; } = default!;

        [OpenApiContentType(ContentTypes.TextPlain)]
        public UserRole Role { get; set; }

        [OpenApiContentType(ContentTypes.TextPlain)]
        public string PhoneNumber { get; set; } = default!;

        [OpenApiContentType(ContentTypes.TextPlain)]
        public string EmailAddress { get; set; } = default!;

        [OpenApiContentType(ContentTypes.TextPlain)]
        public bool IsActive { get; set; }

        [OpenApiContentType(ContentTypes.TextPlain)]
        public FileEntity? ProfilePict { get; set; }
    }
}
