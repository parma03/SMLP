using Microsoft.AspNetCore.Http;
using Smlp.Base.User.Enums;
using Smlp.Shared.Common.Attributes;
using Smlp.Shared.Common.Constants;

namespace Smlp.Shared.Master.Users.Commands.Update
{
    public class UpdateUserRequest
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

        /// <summary>Upload foto profil baru (opsional). Jika tidak diisi, foto lama tetap digunakan.</summary>
        [OpenApiContentType(ContentTypes.OctetStream)]
        public IFormFile? ProfilePictureFile { get; set; }
    }
}
