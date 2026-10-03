namespace JewelleryCommerce.Web.Models.Entities
{
    public enum VendorStatus
    {
        Pending,
        Approved,
        Rejected,
        Suspended
    }

    public enum MetalType
    {
        Gold,
        Silver,
        Diamond,
        Platinum,
        Imitation
    }

    public enum OrderStatus
    {
        Pending,
        Confirmed,
        Packed,
        Shipped,
        Delivered,
        Cancelled,
        Returned
    }

    public enum OrderItemStatus
    {
        Pending,
        Confirmed,
        Packed,
        Shipped,
        Delivered,
        Cancelled
    }

    public enum PaymentMode
    {
        COD,
        Online
    }
}