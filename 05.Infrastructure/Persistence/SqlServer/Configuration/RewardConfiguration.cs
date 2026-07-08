using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Smlp.Domain.Entities.Master;
using Smlp.Infrastructure.Persistence.Common.Constants;
using Smlp.Infrastructure.Persistence.Common.Extensions;
using Smlp.Shared.Common.Constants;

namespace Smlp.Infrastructure.Persistence.SqlServer.Configuration;

public class RewardConfiguration : IEntityTypeConfiguration<Reward>
{
    public void Configure(EntityTypeBuilder<Reward> builder)
    {
        builder.ToTable(TableNameFor.Reward, TableGroupFor.Master);
        builder.ConfigureCreatableProperties();
        builder.ConfigureModifiableProperties();

        builder.Property(e => e.Name)
            .IsRequired()
            .HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.MediumText));

        builder.Property(e => e.Description)
            .HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.LargeText));

        builder.Property(e => e.Type)
            .HasConversion<string>()
            .HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.SimpleName));

        builder.Property(e => e.FreeItemName)
            .HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.MediumText));

        // Catatan: navigasi RewardImage (tipe abstrak FileEntity) sengaja tidak
        // dikonfigurasi ulang di sini untuk menghindari perubahan pada mapping yang
        // sudah berjalan saat ini.

        builder.HasIndex(e => e.IsActive);
        builder.HasIndex(e => new { e.ValidFrom, e.ValidUntil });
        builder.HasIndex(e => e.PointCost);
    }
}
