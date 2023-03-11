using CustomerApi.Data;
using CustomerApi.Models;
using Microsoft.AspNetCore.Mvc;
using SharedModels;

namespace CustomerApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerRepository repository;
        private IConverter<Customer, CustomerDto> customerConverter;

        public CustomersController(ICustomerRepository repo, IConverter<Customer, CustomerDto> converter)
        {
            repository = repo;
            customerConverter = converter;
        }

        // GET: customers
        [HttpGet]
        public IEnumerable<CustomerDto> Get()
        {
            var CustomerDtoList = new List<CustomerDto>();

            CustomerDtoList = repository.GetAll().Select(x => customerConverter.Convert(x)).ToList();

            return CustomerDtoList;
        }

        // GET customer/5
        [HttpGet("{id}", Name = "GetCustomer")]
        public IActionResult Get(int id)
        {
            var item = repository.Get(id);
            if (item == null)
            {
                return NotFound();
            }

            var customerDto = customerConverter.Convert(item);
            return new ObjectResult(customerDto);
        }

        // POST customer
        [HttpPost]
        public IActionResult Post([FromBody] CustomerDto customerDto)
        {
            if (customerDto == null)
            {
                return BadRequest();
            }

            var customer = customerConverter.Convert(customerDto);
            var newcustomer = repository.Add(customer);

            return CreatedAtRoute("GetCustomer", new { id = newcustomer.CustomerId }, customerConverter.Convert(newcustomer));
        }

        // PUT products/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] CustomerDto customerDto)
        {
            if (customerDto == null || customerDto.CustomerId != id)
            {
                return BadRequest();
            }

            var modifiedCustomer = repository.Get(id);

            if (modifiedCustomer == null)
            {
                return NotFound();
            }

            modifiedCustomer.Name = CustomerDto.Name;
            modifiedCustomer.Email = CustomerDto.Email;
            modifiedCustomer.Phone = CustomerDto.Phone;
            modifiedCustomer.BillingAddress = CustomerDto.BillingAddress
            modifiedCustomer.ShippingAddress = CustomerDto.ShippingAddress;
            modifiedCustomer.ShippingAddress = CustomerDto.ShippingAddress;
            modifiedCustomer.CreditStanding = CustomerDto.CreditStanding;


            repository.Edit(modifiedCustomer);
            return new NoContentResult();
        }

        // DELETE products/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            if (repository.Get(id) == null)
            {
                return NotFound();
            }

            repository.Remove(id);
            return new NoContentResult();
        }
    }
}