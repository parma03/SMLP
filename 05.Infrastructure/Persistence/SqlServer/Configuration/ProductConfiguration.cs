using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Smlp.Domain.Entities.Master;
using Smlp.Infrastructure.Persistence.Common.Constants;
using Smlp.Infrastructure.Persistence.Common.Extensions;
using Smlp.Shared.Common.Constants;

namespace Smlp.Infrastructure.Persistence.SqlServer.Configuration;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable(TableNameFor.Product, TableGroupFor.Master);
        builder.ConfigureCreatableProperties();
        builder.ConfigureModifiableProperties();

        builder.Property(e => e.Name)
            .IsRequired()
            .HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.MediumText));

        builder.Property(e => e.Description)
            .HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.LargeText));

        // Catatan: navigasi ProductImage (tipe abstrak FileEntity) sengaja
        // tidak dikonfigurasi ulang di sini agar tidak mengubah mapping yang
        // sudah berjalan saat ini.

        builder.HasOne(e => e.ProductCategory)
            .WithOne(e => e.Product)
            .HasForeignKey<Product>(e => e.ProductId);

        builder.HasIndex(e => e.ProductId);
        builder.HasIndex(e => e.IsActive);
        builder.HasIndex(e => e.Name);
    }
}
