/*
* Pertamina Scholarship Management Portal
* Copyright (c) 2024-2025 PT. Pertamina Persero
*/

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Psmp.Application.Services.Persistence;
using Psmp.Domain.Entities.Masters;
using Psmp.Infrastructure.Persistence.Common.Constants;
using Psmp.Infrastructure.Persistence.Common.Extensions;
using Psmp.Shared.Common.Constants;

namespace Psmp.Infrastructure.Persistence.SqlServer.Configuration;

public class FacultyConfiguration : IEntityTypeConfiguration<Faculty>
{
    public void Configure(EntityTypeBuilder<Faculty> builder)
    {
        builder.ToTable($"{nameof(TableGroupFor.Master)}.{nameof(IPsmpDbContext.Faculties)}", nameof(Psmp));
        builder.ConfigureCreatableProperties();

        if (builder is not null)
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).ValueGeneratedNever();

            builder.Property(e => e.Id).HasColumnType(CommonColumnTypes.SmallInt);
            builder.Property(e => e.Name).HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.ShortText));
        }
    }
}
