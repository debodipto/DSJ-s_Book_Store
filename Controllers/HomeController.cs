using DSJsBookStore.Models;
using DSJsBookStore.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace DSJsBookStore.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHomeRepository _homeRepo;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IHomeRepository homeRepo, ILogger<HomeController> logger)
        {
            _homeRepo = homeRepo;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string sTerm = "", int genreId = 0, string priceRange = "", string sortBy = "")
        {
            var books = await _homeRepo.GetBooks(sTerm, genreId, priceRange, sortBy);
            var genres = await _homeRepo.Genres();

            var model = new HomeViewModel
            {
                Books = books,
                Genres = genres,
                STerm = sTerm,
                GenreId = genreId
            };

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
