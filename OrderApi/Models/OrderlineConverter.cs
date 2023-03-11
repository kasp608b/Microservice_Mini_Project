using SharedModels;

namespace OrderApi.Models
{
    public class OrderlineConverter : IConverter<OrderLine, OrderLineDto>
    {
        public OrderLine Convert(OrderLineDto model)
        {
            return new OrderLine
            {
                OrderId = model.OrderId,
                ProductId = model.ProductId,
                NoOfItems = model.NoOfItems
            };


        }

        public OrderLineDto Convert(OrderLine model)
        {
            return new OrderLineDto
            {
                OrderId = model.OrderId,
                ProductId = model.ProductId,
                NoOfItems = model.NoOfItems
            };
        }
    }
}
