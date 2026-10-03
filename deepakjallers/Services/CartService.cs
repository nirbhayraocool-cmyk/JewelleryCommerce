using System.Text.Json;
using JewelleryCommerce.Web.Models.ViewModels;

namespace JewelleryCommerce.Web.Services
{
    public class CartService
    {
        private const string CartKey = "MyCart";
        private readonly IHttpContextAccessor _http;

        public CartService(IHttpContextAccessor http)
        {
            _http = http;
        }

        private ISession Session => _http.HttpContext!.Session;

        public List<CartItemDto> GetCart()
        {
            var json = Session.GetString(CartKey);
            if (string.IsNullOrEmpty(json))
                return new List<CartItemDto>();

            return JsonSerializer.Deserialize<List<CartItemDto>>(json)
                   ?? new List<CartItemDto>();
        }

        public void SaveCart(List<CartItemDto> items)
        {
            var json = JsonSerializer.Serialize(items);
            Session.SetString(CartKey, json);
        }

        public void AddItem(int productId, int quantity, int? variantId = null)
        {
            var cart = GetCart();
            var existing = cart.FirstOrDefault(c => c.ProductId == productId);

            if (existing != null)
            {
                existing.Quantity += quantity;
                if (variantId.HasValue) existing.VariantId = variantId;
            }
            else
            {
                cart.Add(new CartItemDto
                {
                    ProductId = productId,
                    Quantity = quantity,
                    VariantId = variantId
                });
            }

            SaveCart(cart);
        }

        public void UpdateQuantity(int productId, int quantity)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.ProductId == productId);
            if (item != null)
            {
                if (quantity <= 0)
                    cart.Remove(item);
                else
                    item.Quantity = quantity;
                SaveCart(cart);
            }
        }

        public void RemoveItem(int productId)
        {
            var cart = GetCart();
            cart.RemoveAll(c => c.ProductId == productId);
            SaveCart(cart);
        }

        public void Clear()
        {
            Session.Remove(CartKey);
        }

        public int GetTotalItems()
        {
            return GetCart().Sum(c => c.Quantity);
        }
    }
}