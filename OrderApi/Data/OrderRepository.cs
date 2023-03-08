using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using SharedModels;
using System;

namespace OrderApi.Data
{
    public class OrderRepository : IRepository<Order>
    {
        private readonly OrderApiContext db;

        public OrderRepository(OrderApiContext context)
        {
            db = context;
        }

        Order IRepository<Order>.Add(Order entity)
        {
            if (entity.Date == null)
                entity.Date = DateTime.Now;
            
            var newOrder = db.Orders.Add(entity).Entity;
            db.SaveChanges();
            return newOrder;
        }
        
        void IRepository<Order>.Edit(Order entity)
        {
            db.Entry(entity).State = EntityState.Modified;
            db.SaveChanges();
        }

        Order IRepository<Order>.Get(int id)
        {
            return db.Orders.Include(o => o.Orderlines).FirstOrDefault(o => o.OrderId == id);
        }

        IEnumerable<Order> IRepository<Order>.GetAll()
        {
            return db.Orders.Include(o => o.Orderlines).ToList();
        }

        void IRepository<Order>.Remove(int id)
        {
            var order = db.Orders.FirstOrDefault(p => p.OrderId == id);
            db.Orders.Remove(order);
            db.SaveChanges();
        }
    }
}
