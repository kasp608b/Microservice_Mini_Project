namespace SharedModels
{
    public class OrderStatusChangedMessage
    {
        public int? CustomerId { get; set; }
        public List<OrderLineDto> OrderLines { get; set; }

        public OrderStatusChangedMessage()
        {
            OrderLines = new List<OrderLineDto>();
        }
    }
}
