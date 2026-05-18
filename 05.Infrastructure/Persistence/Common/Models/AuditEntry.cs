using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;
using Psmp.Base.ValueObjects;
using Psmp.Domain.Entities;

namespace Psmp.Infrastructure.Persistence.Common.Models;

public class AuditEntry
{
    public string CreatedBy { get; set; } = default!;
    public string TableName { get; set; } = default!;
    public string EntityName { get; set; } = default!;
    public string ActionType { get; set; } = default!;
    public string ActionName { get; set; } = default!;
    public Guid EntityId { get; set; }
    public IDictionary<string, object?> OldValues { get; } = new Dictionary<string, object?>();
    public IDictionary<string, object?> NewValues { get; } = new Dictionary<string, object?>();
    public string ClientApplicationId { get; set; } = default!;
    public string FromIpAddress { get; set; } = default!;
    public Geolocation? FromGeolocation { get; set; }
    public IList<PropertyEntry> TemporaryProperties { get; } = new List<PropertyEntry>();

    public bool HasTemporaryProperties => TemporaryProperties.Any();

    public Audit ToAudit()
    {
        if (System.Net.IPAddress.TryParse(FromIpAddress, out var parsedIpAddress))
        {
            if (parsedIpAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
            {
                FromIpAddress = parsedIpAddress.MapToIPv4().ToString();
            }
        }
        else
        {
            FromIpAddress = "Invalid";
        }

        var audit = new Audit
        {
            Id = Guid.NewGuid(),
            Created = DateTimeOffset.Now,
            CreatedBy = CreatedBy,
            TableName = TableName,
            EntityName = EntityName,
            ActionType = ActionType,
            ActionName = ActionName,
            EntityId = EntityId,
            OldValues = OldValues.Count == 0 ? null : JsonConvert.SerializeObject(OldValues),
            NewValues = JsonConvert.SerializeObject(NewValues),
            ClientApplicationId = ClientApplicationId,
            FromIpAddress = FromIpAddress,
            FromGeolocation = FromGeolocation
        };

        return audit;
    }
}
