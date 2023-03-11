
namespace SharedModels
{
    public class CustomerDTO
    {
        public int CustomerId { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public string BillingAddress { get; set; }

        public string ShippingAddress { get; set; }

        public bool CreditStanding { get; set; }
    }
}
