using Smlp.Base.ValueObjects;
using Smlp.Domain.Abstracts;

namespace Smlp.Domain.Entities
{
    public class Audit : Entity
    {
        public string TableName { get; set; } = default!;
        public string EntityName { get; set; } = default!;
        public string ActionType { get; set; } = default!;
        public string ActionName { get; set; } = default!;
        public Guid EntityId { get; set; }
        public string? OldValues { get; set; }
        public string NewValues { get; set; } = default!;
        public string ClientApplicationId { get; set; } = default!;
        public string FromIpAddress { get; set; } = default!;
        public Geolocation? FromGeolocation { get; set; }
    }
}
