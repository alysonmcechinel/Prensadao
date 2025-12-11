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

            builder.Property(x => x.DateOrder)
                .IsRequired();

            builder.Property(x => x.OrderStatus)
                .IsRequired();

            builder.Property(x => x.Delivery);

            builder.Property(x => x.Value)
                .IsRequired();

            builder.Property(x => x.Observation)
                .HasMaxLength(500);

            builder.Property(x => x.CustomerId)
                .IsRequired();

            builder
                .HasOne(x => x.Customer) // Order tem um Customer.
                .WithMany(o => o.Orders) // Customer tem muitos Orders
                .HasForeignKey(o => o.CustomerId);  // Define a FK no lado de Order.
        }
    }
}
