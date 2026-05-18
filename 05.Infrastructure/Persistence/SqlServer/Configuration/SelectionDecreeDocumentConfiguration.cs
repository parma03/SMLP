using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Psmp.Application.Services.Persistence;
using Psmp.Domain.Entities.Selections;
using Psmp.Infrastructure.Persistence.Common.Constants;
using Psmp.Infrastructure.Persistence.Common.Extensions;

namespace Psmp.Infrastructure.Persistence.SqlServer.Configuration;

public class SelectionDecreeDocumentConfiguration : IEntityTypeConfiguration<SelectionDecreeDocument>
{
    public void Configure(EntityTypeBuilder<SelectionDecreeDocument> builder)
    {
        builder.ToTable($"{nameof(TableGroupFor.Selection)}.{nameof(IPsmpDbContext.SelectionDecreeDocuments)}", nameof(Psmp));
        builder.ConfigureCreatableProperties();
        builder.ConfigureModifiableProperties();
    }
}
