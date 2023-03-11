using CustomerApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CustomerApi.Data
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly CustomerApiContext db;

        public CustomerRepository(CustomerApiContext context)
        {
            db = context;
        }

        public Customer Add(Customer entity)
        {
            var newCustomer = db.Customers.Add(entity).Entity;
            db.SaveChanges();
            return newCustomer;
        }

        public void Edit(Customer entity)
        {
            db.Entry(entity).State = EntityState.Modified;
            db.SaveChanges();
        }

        public Customer Get(int id)
        {
            return db.Customers.FirstOrDefault(c => c.CustomerId == id);
        }

        public IEnumerable<Customer> GetAll()
        {
            return db.Customers.ToList();
        }

        public void Remove(int id)
        {
            var customer = db.Customers.FirstOrDefault(c => c.CustomerId == id);
            db.Customers.Remove(customer);
            db.SaveChanges();
        }
    }
}
