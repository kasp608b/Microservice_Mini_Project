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
        IMessagePublisher messagePublisher;
        private readonly IConverter<Order, OrderDto> OrderConverter;

        public OrdersController(IRepository<Order> repos,
            IServiceGateway<ProductDto> gateway,
            IMessagePublisher publisher,
            IConverter<Order, OrderDto> orderconverter
            )
        {
            repository = repos as IOrderRepository;
            productServiceGateway = gateway;
            messagePublisher = publisher;
            OrderConverter = orderconverter;
        }

        // GET: orders
        [HttpGet]
        public IEnumerable<OrderDto> Get()
        {
            var orderDtoList = new List<OrderDto>();
            foreach (var order in repository.GetAll())
            {
                var orderDto = OrderConverter.Convert(order);
                orderDtoList.Add(orderDto);
            }
            return orderDtoList;
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


            repository.Edit(modifiedOrder);
            return new NoContentResult();
        }


        // GET orders/5
        [HttpGet("{id}", Name = "GetOrder")]
        public IActionResult Get(int id)
        {
            var item = repository.Get(id);
            if (item == null)
            {
                return NotFound();
            }
            var orderDto = OrderConverter.Convert(item);
            return new ObjectResult(orderDto);
        }

        // POST orders
        [HttpPost]
        public IActionResult Post([FromBody] OrderDto orderDto)
        {
            Console.WriteLine("Order post called");

            if (orderDto == null)
            {
                return BadRequest();
            }

            if (ProductItemsAvailable(orderDto))
            {
                try
                {
                    // Publish OrderStatusChangedMessage. If this operation
                    // fails, the order will not be created
                    messagePublisher.PublishOrderStatusChangedMessage(
                        orderDto.CustomerId, OrderConverter.Convert(orderDto).Orderlines, "completed");

                    // Create order.
                    orderDto.Status = OrderStatus.completed;
                    var newOrder = repository.Add(orderDto);
                    var orderDto = OrderConverter.Convert(newOrder);
                    return CreatedAtRoute("GetOrder", new { id = orderDto.OrderId }, orderDto);
                }
                catch
                {
                    return StatusCode(500, "An error happened. Try again.");
                }
            }
            else
            {
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
            throw new NotImplementedException();

            // Add code to implement this method.
        }

        // PUT orders/5/ship
        // This action method ships an order and publishes an OrderStatusChangedMessage.
        // with topic set to "shipped".
        [HttpPut("{id}/ship")]
        public IActionResult Ship(int id)
        {
            throw new NotImplementedException();

            // Add code to implement this method.
        }

        // PUT orders/5/pay
        // This action method marks an order as paid and publishes a CreditStandingChangedMessage
        // (which have not yet been implemented), if the credit standing changes.
        [HttpPut("{id}/pay")]
        public IActionResult Pay(int id)
        {
            throw new NotImplementedException();

            // Add code to implement this method.
        }
    }

}
