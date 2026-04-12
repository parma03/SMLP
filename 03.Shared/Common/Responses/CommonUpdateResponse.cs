
namespace Smlp.Shared.Common.Responses
{
    public class CommonUpdateResponse : Response
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string? Description { get; set; }
    }
}

