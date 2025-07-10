using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prensadao.Domain.Entities;

namespace Prensadao.Infra.Persistence.Configurations
{
    public class ProductConfig : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.HasKey(x => x.ProductId);

            builder.Property(x => x.Name);
            builder.Property(x => x.Value);
            builder.Property(x => x.Description);

            builder
                .HasMany(x => x.OrderItems)
                .WithOne(i => i.Product)
                .HasForeignKey(i => i.ProductId);
        }
    }
}
