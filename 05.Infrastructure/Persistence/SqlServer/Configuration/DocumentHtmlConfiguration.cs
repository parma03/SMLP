using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Psmp.Application.Services.Persistence;
using Psmp.Domain.Entities.Masters;
using Psmp.Infrastructure.Persistence.Common.Constants;
using Psmp.Infrastructure.Persistence.Common.Extensions;

namespace Psmp.Infrastructure.Persistence.SqlServer.Configuration;

public class DocumentHtmlConfiguration : IEntityTypeConfiguration<DocumentHtml>
{
    public void Configure(EntityTypeBuilder<DocumentHtml> builder)
    {
        builder.ToTable($"{nameof(TableGroupFor.Master)}.{nameof(IPsmpDbContext.DocumentHtmls)}", nameof(Psmp));
        builder.ConfigureCreatableProperties();

        if (builder is not null)
        {
            builder.HasKey(e => e.DocumentTypeHtml);

            builder.Property(e => e.DocumentTypeHtml).HasColumnType(CommonColumnTypes.SmallInt);
            builder.Property(e => e.Html).HasColumnType(CommonColumnTypes.Ntext);
        }
    }
}
