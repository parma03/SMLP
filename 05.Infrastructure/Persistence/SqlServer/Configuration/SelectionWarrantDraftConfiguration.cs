using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Psmp.Application.Services.Persistence;
using Psmp.Domain.Entities.Selections;
using Psmp.Infrastructure.Persistence.Common.Constants;
using Psmp.Infrastructure.Persistence.Common.Extensions;
using Psmp.Shared.Common.Constants;

namespace Psmp.Infrastructure.Persistence.SqlServer.Configuration;

public class SelectionWarrantDraftConfiguration : IEntityTypeConfiguration<SelectionWarrantDraft>
{
    public void Configure(EntityTypeBuilder<SelectionWarrantDraft> builder)
    {
        builder.ToTable($"{nameof(TableGroupFor.Selection)}.{nameof(IPsmpDbContext.SelectionWarrantDrafts)}", nameof(Psmp));
        builder.ConfigureCreatableProperties();
        builder.ConfigureModifiableProperties();

        if (builder is not null)
        {
            builder.Property(e => e.SuperiorLetter).HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.LongText));
        }
    }
}
