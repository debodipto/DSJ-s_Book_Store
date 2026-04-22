using DSJsBookStore.Models;
using Microsoft.EntityFrameworkCore;

namespace DSJsBookStore.Repositories
{
    public class WishlistRepository : IWishlistRepository
    {
        private readonly ApplicationDbContext _db;

        public WishlistRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Wishlist>> GetUserWishlist(string userId)
        {
            return await _db.Wishlists
                .Include(w => w.Book)
                .ThenInclude(b => b.Genre)
                .Where(w => w.UserId == userId)
                .ToListAsync();
        }

        public async Task AddToWishlist(string userId, int bookId)
        {
            if (!await IsInWishlist(userId, bookId))
            {
                var wishlistItem = new Wishlist
                {
                    UserId = userId,
                    BookId = bookId,
                    AddedDate = DateTime.Now
                };
                _db.Wishlists.Add(wishlistItem);
                await _db.SaveChangesAsync();
            }
        }

        public async Task RemoveFromWishlist(string userId, int bookId)
        {
            var wishlistItem = await _db.Wishlists
                .FirstOrDefaultAsync<Wishlist>(w => w.UserId == userId && w.BookId == bookId);

            if (wishlistItem != null)
            {
                _db.Wishlists.Remove(wishlistItem);
                await _db.SaveChangesAsync();
            }
        }

        public async Task<bool> IsInWishlist(string userId, int bookId)
        {
            return await _db.Wishlists
                .AnyAsync<Wishlist>(w => w.UserId == userId && w.BookId == bookId);
        }
    }
}