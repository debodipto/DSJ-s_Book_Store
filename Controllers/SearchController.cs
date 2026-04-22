using DSJsBookStore.Models;
using DSJsBookStore.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace DSJsBookStore.Controllers
{
    public class SearchController : Controller
    {
        private readonly IHomeRepository _homeRepo;

        public SearchController(IHomeRepository homeRepo)
        {
            _homeRepo = homeRepo;
        }

        public async Task<IActionResult> Index(string title = "", string author = "", int genreId = 0, double minPrice = 0, double maxPrice = 1000, string sortBy = "")
        {
            var books = await _homeRepo.GetBooks(title, genreId, "", sortBy); // Need to extend repository for advanced search
            var genres = await _homeRepo.Genres();

            // Filter further
            if (!string.IsNullOrEmpty(author))
            {
                books = books.Where(b => b.AuthorName.Contains(author, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            if (minPrice > 0)
            {
                books = books.Where(b => b.Price >= minPrice).ToList();
            }
            if (maxPrice < 1000)
            {
                books = books.Where(b => b.Price <= maxPrice).ToList();
            }

            var model = new HomeViewModel
            {
                Books = books,
                Genres = genres,
                STerm = title,
                GenreId = genreId
            };

            ViewBag.Author = author;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.SortBy = sortBy;

            return View(model);
        }
    }
}