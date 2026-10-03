namespace JewelleryCommerce.Web.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalVendors { get; set; }
        public int PendingVendors { get; set; }
        public int TotalProducts { get; set; }
        public int PendingProducts { get; set; }
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int TotalCustomers { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalCommission { get; set; }

        public List<RecentOrderVM> RecentOrders { get; set; } = new();
        public List<TopVendorVM> TopVendors { get; set; } = new();
    }

    public class RecentOrderVM
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class TopVendorVM
    {
        public int VendorId { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public int ProductCount { get; set; }
        public int OrdersCount { get; set; }
        public decimal Revenue { get; set; }
    }
}