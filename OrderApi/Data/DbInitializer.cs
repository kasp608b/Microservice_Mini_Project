using System.Collections.Generic;
using System.Linq;
using SharedModels;
using System;

namespace OrderApi.Data
{
    public class DbInitializer : IDbInitializer
    {
        // This method will create and seed the database.
        public void Initialize(OrderApiContext context)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            // Look for any Products
            if (context.Orders.Any())
            {
                return;   // DB has been seeded
            }
            
            List<Order> orders = new List<Order>
            {
                new Order { Date = DateTime.Today, Status = OrderStatus.proccesing}
            };

            List<OrderLine> orderLines = new List<OrderLine>
            {
                 new OrderLine {OrderId = 1, NoOfItems = 5, ProductId = 1},
            };

            context.Orders.AddRange(orders);
            context.Orderlines.AddRange(orderLines);
            context.SaveChanges();
        }
    }
}
