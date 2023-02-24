using System;
namespace OrderApi.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public DateTime? Date { get; set; }
        public OrderStatus Status { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
