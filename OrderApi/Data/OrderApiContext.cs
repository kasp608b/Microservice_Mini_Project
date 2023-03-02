using Microsoft.EntityFrameworkCore;
using OrderApi.Models;

namespace OrderApi.Data
{
    public class OrderApiContext : DbContext
    {
        public OrderApiContext(DbContextOptions<OrderApiContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            /* We want the orderLine relation primary key to be a composite key 
             * made up of the OrderId and the ProductId
             * so we use FluentAPI to do that
             * Here's a link to the docs where I got it from:
             * https://learn.microsoft.com/en-us/ef/core/modeling/keys?tabs=fluent-api
             */
            modelBuilder.Entity<OrderLine>()
                .HasKey(c => new { c.OrderId, c.ProductId });
            
        }

        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderLine> Orderlines { get; set; }

    }
}
