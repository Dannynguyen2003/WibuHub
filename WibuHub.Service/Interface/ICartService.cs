using WibuHub.MVC.ViewModels.ShoppingCart;

namespace WibuHub.Service.Interface
{
    public interface ICartService
    {
        Task<Cart> GetCart(Guid? userId);
        Task AddItem(Guid? userId, Guid productId, int quantity);
        Task UpdateQuantity(Guid? userId, Guid productId, int quantity);
        Task RemoveItem(Guid? userId, Guid productId);
        Task ClearCart(Guid? userId);
    }
}
