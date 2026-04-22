using DSJsBookStore.Models;
using DSJsBookStore.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DSJsBookStore.Controllers
{
    [Authorize]
    public class ReviewController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ReviewController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(int bookId, int rating, string comment)
        {
            if (rating < 1 || rating > 5)
            {
                TempData["error"] = "Rating must be between 1 and 5 stars.";
                return RedirectToAction("Details", "Book", new { id = bookId });
            }

            if (string.IsNullOrWhiteSpace(comment))
            {
                TempData["error"] = "Please provide a review comment.";
                return RedirectToAction("Details", "Book", new { id = bookId });
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                TempData["error"] = "User not authenticated.";
                return RedirectToAction("Details", "Book", new { id = bookId });
            }

            // Check if user already reviewed this book
            var existingReview = await _db.Reviews
                .FirstOrDefaultAsync(r => r.BookId == bookId && r.UserId == userId);

            if (existingReview != null)
            {
                TempData["error"] = "You have already reviewed this book.";
                return RedirectToAction("Details", "Book", new { id = bookId });
            }

            var review = new Review
            {
                BookId = bookId,
                UserId = userId,
                Rating = rating,
                Comment = comment,
                CreatedDate = DateTime.Now,
                IsApproved = false // Admin approval required
            };

            _db.Reviews.Add(review);
            await _db.SaveChangesAsync();

            TempData["success"] = "Your review has been submitted and is pending approval.";
            return RedirectToAction("Details", "Book", new { id = bookId });
        }

        [HttpGet]
        public async Task<IActionResult> GetBookReviews(int bookId)
        {
            var reviews = await _db.Reviews
                .Include(r => r.User)
                .Where(r => r.BookId == bookId && r.IsApproved)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();

            return Json(reviews.Select(r => new
            {
                r.Id,
                r.Rating,
                r.Comment,
                r.CreatedDate,
                UserName = r.User?.UserName ?? "Anonymous"
            }));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveReview(int reviewId)
        {
            var review = await _db.Reviews.FindAsync(reviewId);
            if (review != null)
            {
                review.IsApproved = true;
                await _db.SaveChangesAsync();
                TempData["success"] = "Review approved successfully.";
            }
            return RedirectToAction("Index", "Book");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteReview(int reviewId)
        {
            var review = await _db.Reviews.FindAsync(reviewId);
            if (review != null)
            {
                _db.Reviews.Remove(review);
                await _db.SaveChangesAsync();
                TempData["success"] = "Review deleted successfully.";
            }
            return RedirectToAction("Index", "Book");
        }
    }
}