using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Smlp.Domain.Entities.Master;
using Smlp.Infrastructure.Persistence.Common.Constants;
using Smlp.Infrastructure.Persistence.Common.Extensions;

namespace Smlp.Infrastructure.Persistence.SqlServer.Configuration;

public class AppSettingConfiguration : IEntityTypeConfiguration<AppSetting>
{
    public void Configure(EntityTypeBuilder<AppSetting> builder)
    {
        builder.ToTable(TableNameFor.AppSetting, TableGroupFor.Master);
        builder.ConfigureCreatableProperties();
        builder.ConfigureModifiableProperties();

        builder.HasIndex(e => e.Key).IsUnique();

        builder.Property(e => e.Key)
            .IsRequired()
            .HasColumnType(CommonColumnTypes.Nvarchar(100));

        builder.Property(e => e.Value)
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.Label)
            .HasColumnType(CommonColumnTypes.Nvarchar(200));

        builder.Property(e => e.Group)
            .HasColumnType(CommonColumnTypes.Nvarchar(100));

        builder.Property(e => e.Description)
            .HasColumnType(CommonColumnTypes.Nvarchar(500));
    }
}
