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

public class ApplicantTOEFLScheduleConfiguration : IEntityTypeConfiguration<ApplicantTOEFLSchedule>
{
    public void Configure(EntityTypeBuilder<ApplicantTOEFLSchedule> builder)
    {
        builder.ToTable($"{nameof(TableGroupFor.Applicant)}.{nameof(IPsmpDbContext.ApplicantTOEFLSchedules)}", nameof(Psmp));
        builder.ConfigureCreatableProperties();
        builder.ConfigureModifiableProperties();

        if (builder is not null)
        {
            builder.HasOne(e => e.Applicant)
                .WithMany(e => e.ApplicantTOEFLSchedules)
                .HasForeignKey(e => e.ApplicantId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
