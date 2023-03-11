using SharedModels;

namespace OrderApi.Models
{
    public class OrderStatusConverter : IConverter<OrderStatus, OrderStatusDto>
    {
        public OrderStatus Convert(OrderStatusDto model)
        {
            //convert OrderStatusDto to OrderStatus
            switch (model)
            {
                case OrderStatusDto.completed:
                    return OrderStatus.completed;
                case OrderStatusDto.cancelled:
                    return OrderStatus.cancelled;
                case OrderStatusDto.shipped:
                    return OrderStatus.shipped;
                case OrderStatusDto.paid:
                    return OrderStatus.paid;
                case OrderStatusDto.proccesing:
                    return OrderStatus.proccesing;
                default:
                    return OrderStatus.proccesing;
            }
        }

        public OrderStatusDto Convert(OrderStatus model)
        {
            //convert OrderStatus to OrderStatusDto
            switch (model)
            {
                case OrderStatus.completed:
                    return OrderStatusDto.completed;
                case OrderStatus.cancelled:
                    return OrderStatusDto.cancelled;
                case OrderStatus.shipped:
                    return OrderStatusDto.shipped;
                case OrderStatus.paid:
                    return OrderStatusDto.paid;
                case OrderStatus.proccesing:
                    return OrderStatusDto.proccesing;
                default:
                    return OrderStatusDto.proccesing;
            }
        }
    }
}
