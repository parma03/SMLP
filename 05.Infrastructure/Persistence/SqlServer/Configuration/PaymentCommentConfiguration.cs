using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Psmp.Application.Services.Persistence;
using Psmp.Domain.Entities.Payments;
using Psmp.Infrastructure.Persistence.Common.Constants;
using Psmp.Infrastructure.Persistence.Common.Extensions;
using Psmp.Shared.Common.Constants;

namespace Psmp.Infrastructure.Persistence.SqlServer.Configuration;
public class PaymentCommentConfiguration : IEntityTypeConfiguration<PaymentComment>
{
    public void Configure(EntityTypeBuilder<PaymentComment> builder)
    {
        builder.ToTable($"{nameof(TableGroupFor.Payment)}.{nameof(IPsmpDbContext.PaymentComments)}", nameof(Psmp));
        builder.ConfigureCreatableProperties();
        builder.ConfigureModifiableProperties();

        if (builder is not null)
        {
            builder.Property(e => e.Comment).HasColumnType(CommonColumnTypes.Ntext);
            builder.Property(e => e.User).HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.MediumText));
        }
    }
}
