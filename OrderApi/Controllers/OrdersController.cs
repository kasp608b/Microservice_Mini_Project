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

            var order = OrderConverter.Convert(orderDto);

            if (ProductItemsAvailable(order))
            {
                try
                {
                    // Publish OrderStatusChangedMessage. If this operation
                    // fails, the order will not be created
                    messagePublisher.PublishOrderStatusChangedMessage(
                        order.CustomerId, OrderConverter.Convert(order).Orderlines, "completed");

                    // Create order.
                    order.Status = OrderStatus.completed;
                    var newOrder = repository.Add(order);
                    var neworderDto = OrderConverter.Convert(newOrder);
                    return CreatedAtRoute("GetOrder", new { id = neworderDto.OrderId }, neworderDto);
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
            //Check if the order exists
            var item = repository.Get(id);
            if (item == null)
            {
                return NotFound();
            }

            item.Status = OrderStatus.cancelled;

            repository.Edit(item);
            // Publish OrderStatusChangedMessage
            messagePublisher.PublishOrderStatusChangedMessage(
                item.CustomerId, OrderConverter.Convert(item).Orderlines, "cancelled");
            return new NoContentResult();


        }

        // PUT orders/5/ship
        // This action method ships an order and publishes an OrderStatusChangedMessage.
        // with topic set to "shipped".
        [HttpPut("{id}/ship")]
        public IActionResult Ship(int id)
        {
            //Check if the order exists
            var item = repository.Get(id);
            if (item == null)
            {
                return NotFound();
            }

            item.Status = OrderStatus.shipped;

            repository.Edit(item);
            // Publish OrderStatusChangedMessage
            messagePublisher.PublishOrderStatusChangedMessage(
                item.CustomerId, OrderConverter.Convert(item).Orderlines, "shipped");
            return new NoContentResult();
        }

        // PUT orders/5/pay
        // This action method marks an order as paid and publishes a CreditStandingChangedMessage
        // (which have not yet been implemented), if the credit standing changes.
        [HttpPut("{id}/pay")]
        public IActionResult Pay(int id)
        {
            //Check if the order exists
            var item = repository.Get(id);
            if (item == null)
            {
                return NotFound();
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



            
            
            return new NoContentResult();
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
