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
using Psmp.Shared.Common.Constants;

namespace Psmp.Infrastructure.Persistence.SqlServer.Configuration;

public class ApplicantByPassConfiguration : IEntityTypeConfiguration<ApplicantByPass>
{
    public void Configure(EntityTypeBuilder<ApplicantByPass> builder)
    {
        builder.ToTable($"{nameof(TableGroupFor.Applicant)}.{nameof(IPsmpDbContext.ApplicantByPasses)}", nameof(Psmp));
        builder.ConfigureCreatableProperties();

        if (builder is not null)
        {
            builder.HasKey(e => e.EmailIdaman);

            builder.Property(e => e.EmailIdaman).HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.MediumText));
            builder.Property(e => e.EmployeeIdIdaman).HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.ShortText));
            builder.Property(e => e.EmailAlias).HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.MediumText));
            builder.Property(e => e.EmployeeIdAlias).HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.TinyText));
        }
    }
}
