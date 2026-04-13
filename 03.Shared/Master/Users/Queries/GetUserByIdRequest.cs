using Smlp.Shared.Common.Attributes;
using Smlp.Shared.Common.Constants;

namespace Smlp.Shared.Master.Users.Queries
{
    public class GetUserByIdRequest
    {
        [OpenApiContentType(ContentTypes.TextPlain)]
        public Guid Id { get; set; }
    }
}
