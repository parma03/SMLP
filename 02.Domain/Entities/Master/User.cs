using SMLP.Base.User.Enums;
using SMLP.Domain.Abstracts;

namespace SMLP.Domain.Entities.Master
{
    public class User : AuditableEntity
    {
        public string Username { get; set; } = default!;
        public string DisplayName { get; set; } = default!;
        public UserRole Role { get; set; }
        public string PhoneNumber { get; set; } = default!;
        public string EmailAddress { get; set; } = default!;
        public bool IsActive { get; set; }
    }
}
