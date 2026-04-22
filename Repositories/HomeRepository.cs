using Microsoft.EntityFrameworkCore;
using DSJsBookStore.Models;
using DSJsBookStore.Models.DTOs;

namespace DSJsBookStore.Repositories
{
    public class HomeRepository : IHomeRepository
    {
        private readonly ApplicationDbContext _db;

        public HomeRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<BookDisplayModel>> GetBooks(string sTerm = "", int genreId = 0, string priceRange = "", string sortBy = "")
        {
            var bookQuery = _db.Books
                .AsNoTracking()
                .Include(x => x.Genre)
                .Include(x => x.Stock)
                .AsQueryable();

            // Search term filter
            if (!string.IsNullOrWhiteSpace(sTerm))
            {
                bookQuery = bookQuery.Where(b =>
                    b.BookName.Contains(sTerm) ||
                    b.AuthorName.Contains(sTerm));
            }

            // Genre filter
            if (genreId > 0)
            {
                bookQuery = bookQuery.Where(b => b.GenreId == genreId);
            }

            // Price range filter
            if (!string.IsNullOrWhiteSpace(priceRange))
            {
                switch (priceRange)
                {
                    case "0-10":
                        bookQuery = bookQuery.Where(b => b.Price >= 0 && b.Price <= 10);
                        break;
                    case "10-25":
                        bookQuery = bookQuery.Where(b => b.Price > 10 && b.Price <= 25);
                        break;
                    case "25-50":
                        bookQuery = bookQuery.Where(b => b.Price > 25 && b.Price <= 50);
                        break;
                    case "50-100":
                        bookQuery = bookQuery.Where(b => b.Price > 50 && b.Price <= 100);
                        break;
                    case "100+":
                        bookQuery = bookQuery.Where(b => b.Price > 100);
                        break;
                }
            }

            // Sorting
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                switch (sortBy)
                {
                    case "name":
                        bookQuery = bookQuery.OrderBy(b => b.BookName);
                        break;
                    case "name_desc":
                        bookQuery = bookQuery.OrderByDescending(b => b.BookName);
                        break;
                    case "price":
                        bookQuery = bookQuery.OrderBy(b => b.Price);
                        break;
                    case "price_desc":
                        bookQuery = bookQuery.OrderByDescending(b => b.Price);
                        break;
                    case "newest":
                        bookQuery = bookQuery.OrderByDescending(b => b.Id);
                        break;
                    default:
                        bookQuery = bookQuery.OrderBy(b => b.BookName);
                        break;
                }
            }
            else
            {
                // Default sorting by name
                bookQuery = bookQuery.OrderBy(b => b.BookName);
            }

            return await bookQuery.Select(book => new BookDisplayModel
            {
                Id = book.Id,
                BookName = book.BookName,
                AuthorName = book.AuthorName,
                Price = book.Price,
                Image = book.Image,
                GenreId = book.GenreId,
                GenreName = book.Genre != null ? book.Genre.GenreName : "",
                Quantity = book.Stock != null ? book.Stock.Quantity : 0
            }).ToListAsync();
        }

        public async Task<IEnumerable<Genre>> Genres()
        {
            return await _db.Genres.ToListAsync();
        }
    }
}
