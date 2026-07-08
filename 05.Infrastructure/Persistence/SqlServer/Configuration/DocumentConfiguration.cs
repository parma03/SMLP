using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Smlp.Domain.Entities;
using Smlp.Infrastructure.Persistence.Common.Constants;
using Smlp.Infrastructure.Persistence.Common.Extensions;

namespace Smlp.Infrastructure.Persistence.SqlServer.Configuration;

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable(TableNameFor.Document, TableGroupFor.Common);
        builder.ConfigureCreatableProperties();
        builder.ConfigureModifiableProperties();
        builder.ConfigureFileProperties();
        builder.IgnoreDomainEventsProperties();

        builder.Property(e => e.DocumentType)
            .HasConversion<string>();

        builder.HasIndex(e => e.DocumentType);
    }
}
