/*
* Pertamina Scholarship Management Portal
* Copyright (c) 2024-2025 PT. Pertamina Persero
*/

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Psmp.Application.Services.Persistence;
using Psmp.Domain.Entities.Applicants;
using Psmp.Infrastructure.Persistence.Common.Constants;
using Psmp.Infrastructure.Persistence.Common.Extensions;

namespace Psmp.Infrastructure.Persistence.SqlServer.Configuration;

public class ApplicantPsychotestScheduleConfiguration : IEntityTypeConfiguration<ApplicantPsychotestSchedule>
{
    public void Configure(EntityTypeBuilder<ApplicantPsychotestSchedule> builder)
    {
        builder.ToTable($"{nameof(TableGroupFor.Applicant)}.{nameof(IPsmpDbContext.ApplicantPsychotestSchedules)}", nameof(Psmp));
        builder.ConfigureCreatableProperties();
        builder.ConfigureModifiableProperties();

        if (builder is not null)
        {
            builder.HasOne(e => e.Applicant)
                .WithMany(e => e.ApplicantPsychotestSchedules)
                .HasForeignKey(e => e.ApplicantId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
