using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prensadao.Domain.Entities;

namespace Prensadao.Infra.Persistence.Configurations
{
    public class CustomerConfig : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.HasKey(x => x.CustomerId);

            builder.Property(x => x.Name)
                .HasMaxLength(50);
            builder.Property(x => x.Phone)
                .HasMaxLength(11);
            builder.Property(x => x.Street)
                .HasMaxLength(50);
            builder.Property(x => x.District)
                .HasMaxLength(50);
            builder.Property(x => x.Number)
                .HasMaxLength(10);
            builder.Property(x => x.City)
                .HasMaxLength(50);
            builder.Property(x => x.ReferencePoint)
                .HasMaxLength(500);
            builder.Property(x => x.Cep)
                .HasMaxLength(9);

            builder
                .HasMany(x => x.Orders) // Customer tem muitos Orders.
                .WithOne(i => i.Customer) // Cada Order tem um Customer.
                .HasForeignKey(i => i.CustomerId); // Define a FK no lado de Order.
        }
    }
}
