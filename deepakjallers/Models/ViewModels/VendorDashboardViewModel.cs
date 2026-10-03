namespace JewelleryCommerce.Web.Models.ViewModels
{
    public class VendorDashboardViewModel
    {
        public string ShopName { get; set; } = string.Empty;
        public decimal CommissionRate { get; set; }

        public int TotalProducts { get; set; }
        public int ApprovedProducts { get; set; }
        public int PendingProducts { get; set; }

        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int DeliveredOrders { get; set; }

        public decimal TotalEarnings { get; set; }
        public decimal PendingEarnings { get; set; }
        public decimal PaidEarnings { get; set; }

        public List<RecentVendorOrderVM> RecentOrders { get; set; } = new();
        public List<TopProductVM> TopProducts { get; set; } = new();
    }

    public class RecentVendorOrderVM
    {
        public int OrderItemId { get; set; }
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string ProductTitle { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal VendorEarning { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class TopProductVM
    {
        public int ProductId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public int SoldCount { get; set; }
        public decimal Revenue { get; set; }
    }
}