using DSJsBookStore.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace DSJsBookStore.Controllers
{
    public class AuthorController : Controller
    {
        private readonly IHomeRepository _homeRepo;

        public AuthorController(IHomeRepository homeRepo)
        {
            _homeRepo = homeRepo;
        }

        public async Task<IActionResult> Index(string author)
        {
            if (string.IsNullOrEmpty(author))
            {
                return BadRequest("Author name is required.");
            }

            var books = await _homeRepo.GetBooks("", 0, "", "");
            var authorBooks = books.Where(b => b.AuthorName.Contains(author, StringComparison.OrdinalIgnoreCase)).ToList();
            var genres = await _homeRepo.Genres();

            var model = new HomeViewModel
            {
                Books = authorBooks,
                Genres = genres
            };

            ViewBag.Author = author;

            return View(model);
        }
    }
}