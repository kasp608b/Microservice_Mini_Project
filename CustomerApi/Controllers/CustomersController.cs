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
        public IActionResult Get()
        {
            try
            {
                var CustomerDtoList = new List<CustomerDto>();

                CustomerDtoList = repository.GetAll().Select(x => customerConverter.Convert(x)).ToList();

                return new ObjectResult(CustomerDtoList);

            }
            catch (Exception ex)
            {
                return StatusCode(500, "Something went wrong" + $"{ex.Message}");
            }

        }

        // GET customer/5
        [HttpGet("{id}", Name = "GetCustomer")]
        public IActionResult Get(int id)
        {

            try
            {
                var item = repository.Get(id);
                if (item == null)
                {
                    return NotFound();
                }

                var customerDto = customerConverter.Convert(item);
                return new ObjectResult(customerDto);

            }
            catch (Exception ex)
            {
                return StatusCode(500, "Something went wrong" + $"{ex.Message}");
            }

        }

        // POST customer
        [HttpPost]
        public IActionResult Post([FromBody] CustomerDto customerDto)
        {

            try
            {
                if (customerDto == null)
                {
                    return BadRequest();
                }

                var customer = customerConverter.Convert(customerDto);
                var newcustomer = repository.Add(customer);

                return CreatedAtRoute("GetCustomer", new { id = newcustomer.CustomerId }, customerConverter.Convert(newcustomer));

            }
            catch (Exception ex)
            {
                return StatusCode(500, "Something went wrong" + $"{ex.Message}");
            }

        }

        // PUT customer/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] CustomerDto customerDto)
        {

            try
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

                modifiedCustomer.Email = customerDto.Email;
                modifiedCustomer.Phone = customerDto.Phone;
                modifiedCustomer.BillingAddress = customerDto.BillingAddress;
                modifiedCustomer.ShippingAddress = customerDto.ShippingAddress;

                repository.Edit(modifiedCustomer);
                return new NoContentResult();

            }
            catch (Exception ex)
            {
                return StatusCode(500, "Something went wrong" + $"{ex.Message}");
            }

        }

        // DELETE products/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                if (repository.Get(id) == null)
                {
                    return NotFound();
                }

                repository.Remove(id);
                return new NoContentResult();

            }
            catch (Exception ex)
            {
                return StatusCode(500, "Something went wrong" + $"{ex.Message}");
            }


        }
    }
}