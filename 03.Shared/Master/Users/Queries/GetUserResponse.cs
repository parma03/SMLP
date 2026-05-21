using Smlp.Base.User.Enums;
using Smlp.Shared.Common.Responses;

namespace Smlp.Shared.Master.Users.Queries
{
    public class GetUserResponse : Response
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = default!;
        public string DisplayName { get; set; } = default!;
        public UserRole Role { get; set; }
        public string PhoneNumber { get; set; } = default!;
        public string EmailAddress { get; set; } = default!;
        public bool IsActive { get; set; }
        public ProfilePictureDto? ProfilePicture { get; set; }
    }

    public class ProfilePictureDto
    {
        public Guid Id { get; set; }
        public string FileName { get; set; } = default!;
        public string FileContentType { get; set; } = default!;
        public long FileSize { get; set; }
        /// <summary>URL atau path untuk mengakses foto profil</summary>
        public string? Url { get; set; }
    }
}
