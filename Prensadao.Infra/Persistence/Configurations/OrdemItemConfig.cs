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
                .HasOne(x => x.Order)
                .WithMany(i => i.OrderItems)
                .HasForeignKey(i => i.OrderItemId);

            builder
                .HasOne(x => x.Product)
                .WithMany(i => i.OrderItems)
                .HasForeignKey(i => i.OrderItemId);
        }
    }
}
