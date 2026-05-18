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

public class IeltsConfiguration : IEntityTypeConfiguration<Ielts>
{
    public void Configure(EntityTypeBuilder<Ielts> builder)
    {
        builder.ToTable($"{nameof(TableGroupFor.StudyPreparation)}.{nameof(IPsmpDbContext.Ieltses)}", nameof(Psmp));
        builder.ConfigureCreatableProperties();
        builder.ConfigureModifiableProperties();

        if (builder is not null)
        {
            builder.Property(e => e.TestCenter).HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.MediumText));
            builder.Property(e => e.LocationTest).HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.MediumText));

            builder.HasOne(e => e.Applicant)
                       .WithMany(e => e.Ielts)
                       .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
