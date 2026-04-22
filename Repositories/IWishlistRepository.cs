using DSJsBookStore.Models;

namespace DSJsBookStore.Repositories
{
    public interface IWishlistRepository
    {
        Task<IEnumerable<Wishlist>> GetUserWishlist(string userId);
        Task AddToWishlist(string userId, int bookId);
        Task RemoveFromWishlist(string userId, int bookId);
        Task<bool> IsInWishlist(string userId, int bookId);
    }
}