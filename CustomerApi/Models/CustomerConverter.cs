using SharedModels;

namespace CustomerApi.Models
{
    public class CustomerConverter : IConverter<Customer, CustomerDto>
    {
        public Customer Convert(CustomerDto sharedCustomer)
        {
            return new Customer
            {
                CustomerId = sharedCustomer.CustomerId,
                Name = sharedCustomer.Name,
                Email = sharedCustomer.Email,
                Phone = sharedCustomer.Phone,
                BillingAddress = sharedCustomer.BillingAddress,
                ShippingAddress = sharedCustomer.ShippingAddress,
                CreditStanding = sharedCustomer.CreditStanding,

            };
        }

        public CustomerDto Convert(Customer hiddenCustomer)
        {
            return new CustomerDto
            {
                CustomerId = hiddenCustomer.CustomerId,
                Name = hiddenCustomer.Name,
                Email = hiddenCustomer.Email,
                Phone = hiddenCustomer.Phone,
                BillingAddress = hiddenCustomer.BillingAddress,
                ShippingAddress = hiddenCustomer.ShippingAddress,
                CreditStanding = hiddenCustomer.CreditStanding,
            };
        }
    }
}
