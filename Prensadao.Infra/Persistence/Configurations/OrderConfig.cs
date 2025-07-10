using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prensadao.Domain.Entities;

namespace Prensadao.Infra.Persistence.Configurations
{
    public class OrderConfig : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.HasKey(x => x.OrderId);

            builder.Property(x => x.DateOrder);
            builder.Property(x => x.OrderStatus);
            builder.Property(x => x.Delivery);
            builder.Property(x => x.Value);
            builder.Property(x => x.Observation);
            builder.Property(x => x.CustomerId);

            builder
                .HasOne(x => x.Customer) // Order tem um Customer.
                .WithMany(o => o.Orders) // Customer tem muitos Orders
                .HasForeignKey(o => o.CustomerId);  // Define a FK no lado de Order.

            builder
                .HasMany(x => x.OrderItems) // Order tem muitos OrderItems.
                .WithOne(o => o.Order) // Cada OrderItems tem um Order.
                .HasForeignKey(o => o.OrderItemId); // Define a FK no lado de OrderItem.
        }
    }
}
