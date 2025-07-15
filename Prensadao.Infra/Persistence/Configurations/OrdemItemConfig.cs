using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prensadao.Domain.Entities;

namespace Prensadao.Infra.Persistence.Configurations
{
    public class OrdemItemConfig : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.HasKey(x => x.OrderItemId);

            builder.Property(x => x.Quantity);
            builder.Property(x => x.Value);
            builder.Property(x => x.OrderId);
            builder.Property(x => x.ProductId);

            builder
                .HasOne(x => x.Order) // OrderItem tem um Order.
                .WithMany(i => i.OrderItems) // Orders tem muitos OrderItem
                .HasForeignKey(i => i.OrderId); // Define a FK no lado de OrderItems.

            builder
                .HasOne(x => x.Product) // OrderItem tem um Product.
                .WithMany(i => i.OrderItems) // Product tem muitos OrderItem
                .HasForeignKey(i => i.ProductId); // Define a FK no lado de OrderItems.
        }
    }
}
