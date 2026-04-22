using DSJsBookStore.Models;
using DSJsBookStore.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace DSJsBookStore.Controllers
{
    public class RecommendationsController : Controller
    {
        private readonly IHomeRepository _homeRepo;

        public RecommendationsController(IHomeRepository homeRepo)
        {
            _homeRepo = homeRepo;
        }

        public async Task<IActionResult> Index()
        {
            // For simplicity, get all books and show as recommendations
            var books = await _homeRepo.GetBooks("", 0, "", "");
            var genres = await _homeRepo.Genres();

            var model = new HomeViewModel
            {
                Books = books.Take(10).ToList(), // Top 10
                Genres = genres
            };

            return View(model);
        }
    }
}