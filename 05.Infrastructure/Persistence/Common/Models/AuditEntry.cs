using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;
using Smlp.Base.ValueObjects;
using Smlp.Domain.Entities;

namespace Smlp.Infrastructure.Persistence.Common.Models;

public class AuditEntry
{
    public AuditEntry(EntityEntry entry)
    {
        TableName = entry.Metadata.GetTableName() ?? entry.Metadata.Name;
        EntityName = entry.Metadata.Name;
        ActionType = entry.State.ToString();
        ActionName = entry.State.ToString();

        foreach (var property in entry.Properties)
        {
            if (property.IsTemporary)
            {
                TemporaryProperties.Add(property);
                continue;
            }

            var propertyName = property.Metadata.Name;

            if (property.Metadata.IsPrimaryKey())
            {
                EntityId = (Guid)(property.CurrentValue ?? Guid.Empty);
                continue;
            }

            switch (entry.State)
            {
                case Microsoft.EntityFrameworkCore.EntityState.Added:
                    NewValues[propertyName] = property.CurrentValue;
                    break;
                case Microsoft.EntityFrameworkCore.EntityState.Deleted:
                    OldValues[propertyName] = property.OriginalValue;
                    break;
                case Microsoft.EntityFrameworkCore.EntityState.Modified:
                    if (property.IsModified)
                    {
                        OldValues[propertyName] = property.OriginalValue;
                        NewValues[propertyName] = property.CurrentValue;
                    }

                    break;
            }
        }
    }

    public string CreatedBy { get; set; } = default!;
    public string TableName { get; set; } = default!;
    public string EntityName { get; set; } = default!;
    public string ActionType { get; set; } = default!;
    public string ActionName { get; set; } = default!;
    public Guid EntityId { get; set; }
    public IDictionary<string, object?> OldValues { get; } = new Dictionary<string, object?>();
    public IDictionary<string, object?> NewValues { get; } = new Dictionary<string, object?>();
    public string ClientApplicationId { get; set; } = default!;
    public string FromIpAddress { get; set; } = "0.0.0.0";
    public Geolocation? FromGeolocation { get; set; }
    public IList<PropertyEntry> TemporaryProperties { get; } = new List<PropertyEntry>();

    public bool HasTemporaryProperties => TemporaryProperties.Any();

    public Audit ToAudit()
    {
        var audit = new Audit
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.Now,
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
