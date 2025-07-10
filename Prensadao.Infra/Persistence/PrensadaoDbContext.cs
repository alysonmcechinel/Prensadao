using Microsoft.EntityFrameworkCore;
using Prensadao.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prensadao.Infra.Persistence
{
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
