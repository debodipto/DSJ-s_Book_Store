using DSJsBookStore.Models;

namespace DSJsBookStore.Repositories
{
    public interface ICartRepository
    {
        Task<int> AddItem(int bookId, int qty);
        Task<int> RemoveItem(int bookId);

        Task<ShoppingCart?> GetUserCart();
        Task<ShoppingCart?> GetCart(string userId);

        Task<int> GetCartItemCount(string userId = "");

        Task<CheckoutResult> DoCheckout(CheckoutModel model);

        Task<List<Order>> GetUserOrders();
        Task<Order?> GetOrderById(int id);
        Task DeleteOrder(int id);
    }
}
