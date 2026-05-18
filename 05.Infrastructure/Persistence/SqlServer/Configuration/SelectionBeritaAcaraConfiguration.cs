using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Psmp.Application.Services.Persistence;
using Psmp.Domain.Entities.Selections;
using Psmp.Infrastructure.Persistence.Common.Constants;
using Psmp.Infrastructure.Persistence.Common.Extensions;
using Psmp.Shared.Common.Constants;

namespace Psmp.Infrastructure.Persistence.SqlServer.Configuration;

public class SelectionBeritaAcaraConfiguration : IEntityTypeConfiguration<SelectionBeritaAcara>
{
    public void Configure(EntityTypeBuilder<SelectionBeritaAcara> builder)
    {
        builder.ToTable($"{nameof(TableGroupFor.Selection)}.{nameof(IPsmpDbContext.SelectionBeritaAcaras)}", nameof(Psmp));
        builder.ConfigureCreatableProperties();
        builder.ConfigureModifiableProperties();

        if (builder is not null)
        {
            builder.Property(e => e.Title).HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.LongText));

            builder.HasOne(e => e.ScholarshipPosting)
                .WithMany(e => e.SelectionBeritaAcara)
                .HasForeignKey(e => e.ScholarshipPostingId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
