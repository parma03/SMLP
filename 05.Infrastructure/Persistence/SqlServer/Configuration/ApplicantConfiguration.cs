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

public class ApplicantConfiguration : IEntityTypeConfiguration<Applicant>
{
    public void Configure(EntityTypeBuilder<Applicant> builder)
    {
        builder.ToTable($"{nameof(TableGroupFor.Applicant)}.{nameof(IPsmpDbContext.Applicants)}", nameof(Psmp));
        builder.ConfigureCreatableProperties();
        builder.ConfigureModifiableProperties();

        if (builder is not null)
        {
            builder.Property(e => e.Name).HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.LongText));
            builder.Property(e => e.EmployeeNumber).HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.SimpleName));
            builder.Property(e => e.Position).HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.LongText));
            builder.Property(e => e.Email).HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.CreatedBy));
            builder.Property(e => e.Gender).HasColumnType(CommonColumnTypes.SmallInt);

            builder.HasOne(e => e.ApplicantAdministrativeCriteria)
                .WithOne(e => e.Applicant)
                .HasForeignKey<ApplicantAdministrativeCriteria>(e => e.ApplicantId)
                .IsRequired(false);

            builder.HasOne(e => e.ApplicantCancel)
                .WithOne(e => e.Applicant)
                .HasForeignKey<ApplicantCancel>(e => e.ApplicantId)
                .IsRequired(false);

            builder.HasOne(e => e.Company)
                .WithMany(e => e.Applicants)
                .HasForeignKey(e => e.CompanyCode)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(e => e.Directorate)
                .WithMany(e => e.Applicants)
                .HasForeignKey(e => e.DirectorateId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(e => e.Division)
                .WithMany(e => e.Applicants)
                .HasForeignKey(e => e.DivisionId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(e => e.ScholarshipPosting)
                .WithMany(e => e.Applicants)
                .HasForeignKey(e => e.ScholarshipPostingId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
