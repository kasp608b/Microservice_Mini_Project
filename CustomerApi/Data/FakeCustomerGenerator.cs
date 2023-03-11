using Bogus;
using CustomerApi.Models;

namespace CustomerApi.Data
{
    public class FakeCustomerGenerator
    {
        public Faker<Customer> testCustomers;

        public FakeCustomerGenerator()
        {
            // Set the randomizer seed if you wish to generate repeatable data sets.
            // Defualt seed
            Randomizer.Seed = new Random(8675309);
            testCustomers = createCustomerRuleset();
        }

        public FakeCustomerGenerator(int seed)
        {
            // Set the randomizer seed if you wish to generate repeatable data sets.
            // Seed gets injected
            Randomizer.Seed = new Random(seed);
            testCustomers = createCustomerRuleset();
        }

        public Customer generateCustomer() { return testCustomers.Generate(); }

        public List<Customer> generateCustomers(int size)
        {
            return testCustomers.Generate(size);

        }

        private Faker<Customer> createCustomerRuleset()
        {
            return new Faker<Customer>()
           .RuleFor(c => c.CompanyName, (f, c) => f.Company.CompanyName())
           .RuleFor(c => c.RegistrationNumber, (f, c) => f.Commerce.Ean13())
           .RuleFor(c => c.Email, (f, c) => f.Internet.Email(c.CompanyName))
           .RuleFor(c => c.Phone, (f, c) => f.Phone.PhoneNumber())
           .RuleFor(c => c.BillingAddress, (f, c) => f.Address.FullAddress())
           .RuleFor(c => c.ShippingAddress, (f, c) => f.Address.FullAddress())
           .RuleFor(c => c.CreditStanding, true);
        }




    }
}
