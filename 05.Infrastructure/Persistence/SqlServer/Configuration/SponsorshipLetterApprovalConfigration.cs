/*
* Pertamina Scholarship Management Portal
* Copyright (c) 2024-2025 PT. Pertamina Persero
*/

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Psmp.Application.Services.Persistence;
using Psmp.Domain.Entities.StudyPreparations;
using Psmp.Infrastructure.Persistence.Common.Constants;
using Psmp.Infrastructure.Persistence.Common.Extensions;
using Psmp.Shared.Common.Constants;

namespace Psmp.Infrastructure.Persistence.SqlServer.Configuration;

public class SponsorshipLetterApprovalConfigration : IEntityTypeConfiguration<SponsorshipLetterApproval>
{
    public void Configure(EntityTypeBuilder<SponsorshipLetterApproval> builder)
    {
        builder.ToTable($"{nameof(TableGroupFor.StudyPreparation)}.{nameof(IPsmpDbContext.SponsorshipLetterApprovals)}", nameof(Psmp));
        builder.ConfigureCreatableProperties();
        builder.ConfigureModifiableProperties();

        if (builder is not null)
        {
            builder.Property(e => e.Notes).HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.LargeText));

            builder.HasOne(e => e.SponsorshipLetter)
                       .WithMany(e => e.SponsorshipLetterApprovals)
                       .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
