namespace SharedModels
{
    internal class OrderStatusChangedMessage
    {
        public int? CustomerId { get; set; }
        public List<OrderLine> Orderlines { get; set; }
    }
}
