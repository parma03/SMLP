/*
* Pertamina Scholarship Management Portal
* Copyright (c) 2024-2025 PT. Pertamina Persero
*/

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Psmp.Application.Services.Persistence;
using Psmp.Domain.Entities.Selections;
using Psmp.Infrastructure.Persistence.Common.Constants;
using Psmp.Infrastructure.Persistence.Common.Extensions;
using Psmp.Shared.Common.Constants;

namespace Psmp.Infrastructure.Persistence.SqlServer.Configuration;

public class SelectionPsychotestScheduleConfiguration : IEntityTypeConfiguration<SelectionPsychotestSchedule>
{
    public void Configure(EntityTypeBuilder<SelectionPsychotestSchedule> builder)
    {
        builder.ToTable($"{nameof(TableGroupFor.Selection)}.{nameof(IPsmpDbContext.SelectionPsychotestSchedules)}", nameof(Psmp));
        builder.ConfigureCreatableProperties();
        builder.ConfigureModifiableProperties();

        if (builder is not null)
        {
            builder.Property(e => e.DailySession).HasColumnType(CommonColumnTypes.SmallInt);
            builder.Property(e => e.DailyApplicant).HasColumnType(CommonColumnTypes.SmallInt);
            builder.Property(e => e.Location).HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.LongText));

            builder.HasOne(e => e.ScholarshipPosting)
                .WithMany(e => e.SelectionPsychotestSchedules)
                .HasForeignKey(e => e.ScholarshipPostingId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
