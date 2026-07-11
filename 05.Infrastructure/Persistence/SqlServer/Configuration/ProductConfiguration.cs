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

        // ProductImage sekarang dipetakan sebagai entity concrete tersendiri
        // (lihat ProductImageConfiguration), bukan lagi tipe abstrak FileEntity,
        // agar tidak membentuk hierarki tabel yang tidak sengaja dengan Document.
        builder.HasOne(e => e.ProductImage)
            .WithMany()
            .HasForeignKey(e => e.ProductImageId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ProductCategory)
            .WithOne(e => e.Product)
            .HasForeignKey<Product>(e => e.ProductId);

        builder.HasIndex(e => e.ProductId);
        builder.HasIndex(e => e.ProductImageId);
        builder.HasIndex(e => e.IsActive);
        builder.HasIndex(e => e.Name);
    }
}
