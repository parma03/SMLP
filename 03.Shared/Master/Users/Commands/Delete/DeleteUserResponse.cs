using Smlp.Shared.Common.Responses;

namespace Psmp.Shared.Scholarships.ScholarshipPlan.Commands.Delete
{
    public class DeleteUserResponse : Response
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = default!;
        public string DisplayName { get; set; } = default!;
    }
}
