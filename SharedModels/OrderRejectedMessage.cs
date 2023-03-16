namespace SharedModels
{
    public class OrderRejectedMessage
    {
        public OrderDto orderDto { get; set; }

        public string message { get; set; }

        public OrderRejectedMessage() { }

    }
}
