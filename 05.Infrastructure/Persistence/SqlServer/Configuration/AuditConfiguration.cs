using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Smlp.Domain.Entities;
using Smlp.Infrastructure.Persistence.Common.Constants;
using Smlp.Infrastructure.Persistence.Common.Extensions;

namespace Smlp.Infrastructure.Persistence.SqlServer.Configuration;

public class AuditConfiguration : IEntityTypeConfiguration<Audit>
{
    public void Configure(EntityTypeBuilder<Audit> builder)
    {
        builder.ToTable(TableNameFor.Audit, TableGroupFor.Common);
        builder.ConfigureCreatableProperties();
        builder.Property(e => e.TableName).HasColumnType(CommonColumnTypes.Nvarchar(200));
        builder.Property(e => e.EntityName).HasColumnType(CommonColumnTypes.Nvarchar(200));
        builder.Property(e => e.ActionType).HasColumnType(CommonColumnTypes.Nvarchar(50));
        builder.Property(e => e.ActionName).HasColumnType(CommonColumnTypes.Nvarchar(200));
        builder.Property(e => e.ClientApplicationId).HasColumnType(CommonColumnTypes.Nvarchar(200));
        builder.Property(e => e.FromIpAddress).HasColumnType(CommonColumnTypes.Nvarchar(50));
        builder.Property(e => e.OldValues).HasColumnType("nvarchar(max)");
        builder.Property(e => e.NewValues).HasColumnType("nvarchar(max)");
        builder.OwnsOne(e => e.FromGeolocation, geolocation =>
        {
            geolocation.Property(g => g.Latitude);
            geolocation.Property(g => g.Longitude);
            geolocation.Property(g => g.Accuracy);
            geolocation.Property(g => g.City).HasColumnType(CommonColumnTypes.Nvarchar(100));
            geolocation.Property(g => g.Country).HasColumnType(CommonColumnTypes.Nvarchar(100));
        });
    }
}
