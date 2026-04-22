using DSJsBookStore.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DSJsBookStore.Controllers
{
    [Authorize]
    public class WishlistController : Controller
    {
        private readonly IWishlistRepository _wishlistRepo;
        private readonly IHomeRepository _homeRepo;

        public WishlistController(IWishlistRepository wishlistRepo, IHomeRepository homeRepo)
        {
            _wishlistRepo = wishlistRepo;
            _homeRepo = homeRepo;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return RedirectToAction("Login", "Account");

            var wishlist = await _wishlistRepo.GetUserWishlist(userId);
            var genres = await _homeRepo.Genres();

            ViewBag.Genres = genres;
            return View(wishlist);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToWishlist(int bookId)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Json(new { success = false, message = "Please login first" });

            await _wishlistRepo.AddToWishlist(userId, bookId);
            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromWishlist(int bookId)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Json(new { success = false });

            await _wishlistRepo.RemoveFromWishlist(userId, bookId);
            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> IsInWishlist(int bookId)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Json(false);

            var isInWishlist = await _wishlistRepo.IsInWishlist(userId, bookId);
            return Json(isInWishlist);
        }
    }
}