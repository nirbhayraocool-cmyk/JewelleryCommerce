namespace JewelleryCommerce.Web.Models.Entities
{
    public class OrderItem
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public int VendorId { get; set; }
        public Vendor Vendor { get; set; } = null!;

        public int Quantity { get; set; }
        public decimal Price { get; set; }

        public decimal VendorEarning { get; set; }

        public OrderItemStatus Status { get; set; } = OrderItemStatus.Pending;
    }
}