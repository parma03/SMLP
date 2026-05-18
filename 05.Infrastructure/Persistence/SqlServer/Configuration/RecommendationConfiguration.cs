using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Psmp.Application.Services.Persistence;
using Psmp.Domain.Entities.Universities;
using Psmp.Infrastructure.Persistence.Common.Constants;
using Psmp.Infrastructure.Persistence.Common.Extensions;
using Psmp.Shared.Common.Constants;

namespace Psmp.Infrastructure.Persistence.SqlServer.Configuration;

public class RecommendationConfiguration : IEntityTypeConfiguration<Recommendation>
{
    public void Configure(EntityTypeBuilder<Recommendation> builder)
    {
        builder.ToTable($"{nameof(TableGroupFor.University)}.{nameof(IPsmpDbContext.Recommendations)}", nameof(Psmp));
        builder.ConfigureCreatableProperties();
        builder.ConfigureModifiableProperties();

        if (builder is not null)
        {
            builder.Property(e => e.University).HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.MediumText));

            builder.Property(e => e.Country).HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.MediumText));
            builder.Property(e => e.OfficialWebsite).HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.LongText));
            builder.Property(e => e.RegistrationLink).HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.LongText));
        }
    }
}
