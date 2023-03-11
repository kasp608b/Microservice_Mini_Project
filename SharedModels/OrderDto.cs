namespace SharedModels {
    public class OrderDto
    {
        public int OrderId { get; set; }
        public DateTime? Date { get; set; }
        public OrderStatusDto Status { get; set; }
        public int CustomerId { get; set; }
        public List<OrderLineDto> Orderlines { get; set; }

        public OrderDto()
        {
            Orderlines = new List<OrderLineDto>();
        }



    }
}