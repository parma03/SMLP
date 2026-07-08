using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Smlp.Domain.Entities.Master;
using Smlp.Infrastructure.Persistence.Common.Constants;
using Smlp.Infrastructure.Persistence.Common.Extensions;
using Smlp.Shared.Common.Constants;

namespace Smlp.Infrastructure.Persistence.SqlServer.Configuration;

public class PointRuleConfiguration : IEntityTypeConfiguration<PointRule>
{
    public void Configure(EntityTypeBuilder<PointRule> builder)
    {
        builder.ToTable(TableNameFor.PointRule, TableGroupFor.Master);
        builder.ConfigureCreatableProperties();
        builder.ConfigureModifiableProperties();

        builder.Property(e => e.Name)
            .IsRequired()
            .HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.MediumText));

        builder.Property(e => e.Notes)
            .HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.LargeText));

        builder.HasIndex(e => e.IsActive);
        builder.HasIndex(e => new { e.ValidFrom, e.ValidUntil });
    }
}
