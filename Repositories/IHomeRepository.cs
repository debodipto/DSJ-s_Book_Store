using DSJsBookStore.Models;
using DSJsBookStore.Models.DTOs;

namespace DSJsBookStore.Repositories
{
    public interface IHomeRepository
    {
        Task<IEnumerable<BookDisplayModel>> GetBooks(string sTerm = "", int genreId = 0, string priceRange = "", string sortBy = "");
        Task<IEnumerable<Genre>> Genres();
    }
}