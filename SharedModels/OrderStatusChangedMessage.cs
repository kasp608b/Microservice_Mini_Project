namespace SharedModels
{
    public class OrderStatusChangedMessage
    {
        public int? CustomerId { get; set; }
        public List<OrderLine> OrderLines { get; set; }
    }
}
