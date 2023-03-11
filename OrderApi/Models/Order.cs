namespace SharedModels
{
    public class Order
    {
        public int OrderId { get; set; }
        public DateTime? Date { get; set; }
        public OrderStatus Status { get; set; }
        public int CustomerId { get; set; }
        public List<OrderLine> Orderlines { get; set; }

        public Order()
        {
            Orderlines = new List<OrderLine>();
        }
    }
}
 