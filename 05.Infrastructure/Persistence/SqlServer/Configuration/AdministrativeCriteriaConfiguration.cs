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

public class AdministrativeCriteriaConfiguration : IEntityTypeConfiguration<AdministrativeCriteria>
{
    public void Configure(EntityTypeBuilder<AdministrativeCriteria> builder)
    {
        builder.ToTable($"{nameof(TableGroupFor.Master)}.{nameof(IPsmpDbContext.AdministrativeCriterias)}", nameof(Psmp));
        builder.ConfigureCreatableProperties();

        if (builder is not null)
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).ValueGeneratedNever();

            builder.Property(e => e.Id).HasColumnType(CommonColumnTypes.SmallInt);
            builder.Property(e => e.Name).HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.SimpleName));
            builder.Property(e => e.Value).HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.ShortText));
        }
    }
}
