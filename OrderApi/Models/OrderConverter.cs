using SharedModels;

namespace OrderApi.Models
{
    public class OrderConverter : IConverter<Order,OrderDto>
    {
        public Order Convert(OrderDto sharedOrder)
        {
            return new Order
            {
                OrderId = sharedOrder.OrderId,
                Date = sharedOrder.Date,
                Status = new OrderStatusConverter().Convert(sharedOrder.Status),
                CustomerId = sharedOrder.CustomerId,
                Orderlines = sharedOrder.Orderlines.Select(x => new OrderlineConverter().Convert(x)).ToList()
            };
        }

        public OrderDto Convert(Order model)
        {
            return new OrderDto
            {
                OrderId = model.OrderId,
                Date = model.Date,
                Status = new OrderStatusConverter().Convert(model.Status),
                CustomerId = model.CustomerId,
                Orderlines = model.Orderlines.Select(x => new OrderlineConverter().Convert(x)).ToList()
            };
        }
    }
}
