using Smlp.Shared.Common.Responses;

namespace Smlp.Shared.Master.Users.Commands.Delete
{
    public class DeleteUserResponse : Response
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = default!;
        public string DisplayName { get; set; } = default!;
    }
}
