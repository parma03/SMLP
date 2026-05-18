/*
* Pertamina Scholarship Management Portal
* Copyright (c) 2024-2025 PT. Pertamina Persero
*/

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Psmp.Application.Services.Persistence;
using Psmp.Base.ValueObjects;
using Psmp.Domain.Entities;
using Psmp.Infrastructure.Persistence.Common.Constants;
using Psmp.Infrastructure.Persistence.Common.Extensions;
using Psmp.Infrastructure.Persistence.SqlServer.Constants;
using Psmp.Shared.Audits.Constants;

namespace Psmp.Infrastructure.Persistence.SqlServer.Configuration;

public class AuditConfiguration : IEntityTypeConfiguration<Audit>
{
    public void Configure(EntityTypeBuilder<Audit> builder)
    {
        builder.ToTable($"{nameof(TableGroupFor.Common)}.{nameof(IPsmpDbContext.Audits)}", nameof(Psmp));
        builder.ConfigureCreatableProperties();

        if (builder is not null)
        {
            builder.Property(e => e.TableName).HasColumnType(CommonColumnTypes.Nvarchar(MaximumLengthFor.TableName));
            builder.Property(e => e.EntityName).HasColumnType(CommonColumnTypes.Nvarchar(MaximumLengthFor.EntityName));
            builder.Property(e => e.ActionType).HasColumnType(CommonColumnTypes.Nvarchar(MaximumLengthFor.ActionType));
            builder.Property(e => e.ActionName).HasColumnType(CommonColumnTypes.Nvarchar(MaximumLengthFor.ActionName));
            builder.Property(e => e.OldValues).HasColumnType(ColumnTypes.NvarcharMax);
            builder.Property(e => e.NewValues).HasColumnType(ColumnTypes.NvarcharMax);
            builder.Property(e => e.ClientApplicationId).HasColumnType(CommonColumnTypes.Nvarchar(MaximumLengthFor.ClientApplicationId));
            builder.Property(e => e.FromIpAddress).HasColumnType(CommonColumnTypes.Nvarchar(MaximumLengthFor.FromIpAddress));

            builder.OwnsOne(o => o.FromGeolocation, p =>
            {
                p.Property(p => p.Latitude).HasColumnName(nameof(Geolocation.Latitude));
                p.Property(p => p.Longitude).HasColumnName(nameof(Geolocation.Longitude));
                p.Property(p => p.Accuracy).HasColumnName(nameof(Geolocation.Accuracy));
            });
        }
    }
}
