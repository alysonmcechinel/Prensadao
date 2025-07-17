using Microsoft.EntityFrameworkCore;
using Prensadao.Domain.Entities;

namespace Prensadao.Infra.Persistence
{
    // DbContext = Instancia que representa uma seção de acesso ao db(banco de dados)
    public class PrensadaoDbContext : DbContext
    {

        public PrensadaoDbContext(DbContextOptions<PrensadaoDbContext> options)
            : base(options)
        {
            
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(PrensadaoDbContext).Assembly);
            
            base.OnModelCreating(modelBuilder);
        }
    }
}
