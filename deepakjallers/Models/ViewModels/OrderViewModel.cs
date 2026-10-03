namespace JewelleryCommerce.Web.Models.ViewModels
{
    public class OrderListViewModel
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string PaymentMode { get; set; } = string.Empty;
        public int ItemCount { get; set; }
        public string FirstImageUrl { get; set; } = string.Empty;
    }

    public class OrderDetailViewModel
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string PaymentMode { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Pincode { get; set; } = string.Empty;
        public List<OrderItemVM> Items { get; set; } = new();
    }

    public class OrderItemVM
    {
        public int ProductId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string VendorName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal LineTotal => Price * Quantity;
        public string Status { get; set; } = string.Empty;
    }
}