/*
* Pertamina Scholarship Management Portal
* Copyright (c) 2024-2025 PT. Pertamina Persero
*/

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Psmp.Application.Services.Persistence;
using Psmp.Domain.Entities.Scholarships;
using Psmp.Infrastructure.Persistence.Common.Constants;
using Psmp.Infrastructure.Persistence.Common.Extensions;
using Psmp.Shared.Common.Constants;

namespace Psmp.Infrastructure.Persistence.SqlServer.Configuration;

public class ScholarshipPostingConfiguration : IEntityTypeConfiguration<ScholarshipPosting>
{
    public void Configure(EntityTypeBuilder<ScholarshipPosting> builder)
    {
        builder.ToTable($"{nameof(TableGroupFor.Scholarship)}.{nameof(IPsmpDbContext.ScholarshipPostings)}", nameof(Psmp));
        builder.ConfigureCreatableProperties();
        builder.ConfigureModifiableProperties();

        if (builder is not null)
        {
            builder.Property(e => e.Code).HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.SimpleName));
            builder.Property(e => e.Faculty).HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.LargeText));
            builder.Property(e => e.Major).HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.LargeText));
            builder.Property(e => e.UniversityReference).HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.MediumText));
            builder.Property(e => e.LatestEducation).HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.MediumText));
            builder.Property(e => e.WorkingExperience).HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.MediumText));

            builder.Property(e => e.AwardeeQuota).HasColumnType(CommonColumnTypes.SmallInt);
            builder.Property(e => e.Batch).HasColumnType(CommonColumnTypes.SmallInt);
            builder.Property(e => e.PostingYear).HasColumnType(CommonColumnTypes.SmallInt);
            builder.Property(e => e.Prl).HasColumnType(CommonColumnTypes.SmallInt);
            builder.Property(e => e.Age).HasColumnType(CommonColumnTypes.SmallInt);
            builder.Property(e => e.WorkPeriod).HasColumnType(CommonColumnTypes.SmallInt);

            builder.HasOne(e => e.ScholarshipDecree)
                .WithOne(e => e.ScholarshipPosting)
                .HasForeignKey<ScholarshipDecree>(e => e.ScholarshipPostingId)
                .IsRequired(false);
        }
    }
}
