using Microsoft.AspNetCore.Mvc;
using OrderApi.Data;
using OrderApi.Infrastructure;
using OrderApi.Models;
using SharedModels;

namespace OrderApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrdersController : ControllerBase
    {
        IOrderRepository repository;
        IServiceGateway<ProductDto> productServiceGateway;
        IServiceGateway<CustomerDto> _customerGateway;
        IMessagePublisher messagePublisher;
        private readonly IConverter<Order, OrderDto> OrderConverter;

        public OrdersController(IRepository<Order> repos,
            IServiceGateway<ProductDto> gateway,
            IServiceGateway<CustomerDto> customerGateway,
            IMessagePublisher publisher,
            IConverter<Order, OrderDto> orderconverter
            )
        {
            repository = repos as IOrderRepository;
            productServiceGateway = gateway;
            messagePublisher = publisher;
            _customerGateway = customerGateway;
            OrderConverter = orderconverter;
        }

        // GET: orders
        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                var orderDtoList = new List<OrderDto>();
                foreach (var order in repository.GetAll())
                {
                    var orderDto = OrderConverter.Convert(order);
                    orderDtoList.Add(orderDto);
                }

                return new ObjectResult(orderDtoList);

            }
            catch (Exception ex)
            {
                return StatusCode(500, "Something went wrong" + $"{ex.Message}");
            }
        }

        /// <summary>
        /// Update order status
        /// </summary>
        /// <param name="id"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] OrderDto orderDto)
        {
            if (orderDto == null || orderDto.OrderId != id)
            {
                return BadRequest();
            }

            var modifiedOrder = repository.Get(id);

            if (modifiedOrder == null)
            {
                return NotFound();
            }

            modifiedOrder.Status = OrderConverter.Convert(orderDto).Status;


            try
            {
                repository.Edit(modifiedOrder);

            }
            catch (Exception ex)
            {
                return StatusCode(500, "Something went wrong" + $"{ex.Message}");
            }

            return new NoContentResult();
        }


        // GET orders/5
        [HttpGet("{id}", Name = "GetOrder")]
        public IActionResult Get(int id)
        {
            Order item;

            try
            {
                item = repository.Get(id);

            }
            catch (Exception ex)
            {
                return StatusCode(500, "Something went wrong" + $"{ex.Message}");
            }

            if (item == null)
            {
                return NotFound();
            }
            var orderDto = OrderConverter.Convert(item);
            return new ObjectResult(orderDto);
        }

        // GET orders/5
        [HttpGet("GetByCustomer/{id}", Name = "GetOrderByCustomer")]
        public IActionResult GetbyCustomer(int id)
        {
            var orderDtoList = new List<OrderDto>();

            IEnumerable<Order> orderList;

            try
            {
                orderList = repository.GetByCustomer(id);

            }
            catch (Exception ex)
            {
                return StatusCode(500, "Something went wrong" + $"{ex.Message}");
            }


            if (orderList.Count() <= 0) return NotFound("No matching orders found");

            foreach (var order in orderList)
            {
                var orderDto = OrderConverter.Convert(order);
                orderDtoList.Add(orderDto);
            }
            return Ok(orderDtoList);

        }

        // POST orders
        [HttpPost]
        public IActionResult Post([FromBody] OrderDto orderDto)
        {
            Console.WriteLine("Order post called");
            var customer = _customerGateway.Get(orderDto.CustomerId);


            if (customer.CustomerId != orderDto.CustomerId || customer.CustomerId == 0)
            {
                return BadRequest("The customer  does not exist");
            }

            if (orderDto == null)
            {
                return BadRequest();
            }

            if (!customer.CreditStanding)
            {
                // Publish OrderRejectedMessage.
                messagePublisher.PublishOrderRejectedMessage(orderDto, "The customer has outstanding bills");

                return BadRequest("The customer has outstanding bills");
            }

            var order = OrderConverter.Convert(orderDto);

            if (ProductItemsAvailable(order))
            {
                try
                {


                    // Create order.
                    order.Status = OrderStatus.completed;

                    Order newOrder;

                    try
                    {
                        newOrder = repository.Add(order);

                    }
                    catch (Exception ex)
                    {
                        return StatusCode(500, "Something went wrong" + $"{ex.Message}");
                    }

                    var neworderDto = OrderConverter.Convert(newOrder);

                    // Publish OrderStatusChangedMessage. If this operation
                    // fails, the order will not be created
                    messagePublisher.PublishOrderStatusChangedMessage(
                        order.CustomerId, neworderDto.Orderlines, "completed");

                    return CreatedAtRoute("GetOrder", new { id = neworderDto.OrderId }, neworderDto);
                }
                catch
                {

                    // Publish OrderRejectedMessage.
                    messagePublisher.PublishOrderRejectedMessage(orderDto, "An error happened. Try again.");
                    return StatusCode(500, "An error happened. Try again.");
                }
            }
            else
            {
                // Publish OrderRejectedMessage.
                messagePublisher.PublishOrderRejectedMessage(orderDto, "Not enough items in stock.");

                // If there are not enough product items available.
                return StatusCode(500, "Not enough items in stock.");
            }
        }

        private bool ProductItemsAvailable(Order order)
        {
            foreach (var orderLine in order.Orderlines)
            {
                // Call product service to get the product ordered.
                var orderedProduct = productServiceGateway.Get(orderLine.ProductId);
                if (orderLine.NoOfItems > orderedProduct.ItemsInStock - orderedProduct.ItemsReserved)
                {
                    return false;
                }
            }
            return true;
        }

        // PUT orders/5/cancel
        // This action method cancels an order and publishes an OrderStatusChangedMessage
        // with topic set to "cancelled".
        [HttpPut("{id}/cancel")]
        public IActionResult Cancel(int id)
        {

            try
            {
                //Check if the order exists
                var item = repository.Get(id);

                if (item == null)
                {
                    return NotFound();
                }

                if (item.Status == OrderStatus.paid)
                {
                    return StatusCode(400, "Order already shipped");
                }


                if (item.Status == OrderStatus.shipped)
                {
                    return StatusCode(400, "Order already shipped");
                }

                if (item.Status == OrderStatus.cancelled)
                {
                    return StatusCode(400, "Order already cancelled");
                }

                item.Status = OrderStatus.cancelled;


                repository.Edit(item);

                // Publish OrderStatusChangedMessage
                messagePublisher.PublishOrderStatusChangedMessage(
                    item.CustomerId, OrderConverter.Convert(item).Orderlines, "cancelled");
                return StatusCode(200, "Order cancelled");

            }
            catch (Exception ex)
            {
                return StatusCode(500, "Something went wrong" + $"{ex.Message}");
            }

        }

        // PUT orders/5/ship
        // This action method ships an order and publishes an OrderStatusChangedMessage.
        // with topic set to "shipped".
        [HttpPut("{id}/ship")]
        public IActionResult Ship(int id)
        {
            try
            {
                //Check if the order exists
                var item = repository.Get(id);
                if (item == null)
                {
                    return NotFound();
                }

                if (item.Status == OrderStatus.shipped)
                {
                    return StatusCode(400, "Order already shipped");
                }

                item.Status = OrderStatus.shipped;

                repository.Edit(item);
                // Publish OrderStatusChangedMessage
                messagePublisher.PublishOrderStatusChangedMessage(
                    item.CustomerId, OrderConverter.Convert(item).Orderlines, "shipped");
                return StatusCode(200, "Order shipped");

            }
            catch (Exception ex)
            {
                return StatusCode(500, "Something went wrong" + $"{ex.Message}");
            }

        }

        // PUT orders/5/pay
        // This action method marks an order as paid and publishes a CreditStandingChangedMessage
        // (which have not yet been implemented), if the credit standing changes.
        [HttpPut("{id}/pay")]
        public IActionResult Pay(int id)
        {
            try
            {
                //Check if the order exists
                var item = repository.Get(id);
                if (item == null)
                {
                    return NotFound();
                }

                if (item.Status == OrderStatus.paid)
                {
                    return StatusCode(400, "Order already paid");
                }

                item.Status = OrderStatus.paid;

                repository.Edit(item);

                //Check if the customers credit standing has changed
                //If it has changed, publish a CreditStandingChangedMessage

                if (CreditStandingHasChanged(item.CustomerId))
                {
                    // Publish CreditStandingChangedMessage
                    messagePublisher.PublishCreditStandingChangedMessage(item.CustomerId, true);
                }

                return StatusCode(200, "Order paid");

            }
            catch (Exception ex)
            {
                return StatusCode(500, "Something went wrong" + $"{ex.Message}");
            }

        }

        //Check if the customers credit standing has changed
        private bool CreditStandingHasChanged(int customerId)
        {

            //If any of the customers orders are shipped then that mens that the customer still has bad creadit standing
            //And so his credit standing has not changed
            return !repository.GetByCustomer(customerId).Any(o => o.Status == OrderStatus.shipped);


        }
    }

}
