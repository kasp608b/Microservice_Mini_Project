namespace CustomerApi.Data
{
    public class DbInitializer : IDbInitializer
    {
        public void Initialize(CustomerApiContext context)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            // Look for any Customers
            if (context.Customers.Any())
            {
                return; // DB has been seeded
            }

            FakeCustomerGenerator generator = new FakeCustomerGenerator();

            context.Customers.AddRange(generator.generateCustomers(10));
            context.SaveChanges();
        }
    }
}
