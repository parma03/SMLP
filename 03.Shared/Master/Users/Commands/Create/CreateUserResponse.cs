using Smlp.Base.User.Enums;
using Smlp.Domain.Abstracts;
using Smlp.Shared.Common.Responses;

namespace Smlp.Shared.Master.Users.Commands.Create
{
    public class CreateUserResponse : Response
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = default!;
        public string DisplayName { get; set; } = default!;
        public UserRole Role { get; set; }
        public string PhoneNumber { get; set; } = default!;
        public string EmailAddress { get; set; } = default!;
        public bool IsActive { get; set; }
        public FileEntity? ProfilePict { get; set; }
    }
}
