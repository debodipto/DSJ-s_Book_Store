using DSJsBookStore.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using DSJsBookStore.Repositories;
using DSJsBookStore.Models;
using DSJsBookStore.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace DSJsBookStore.Controllers
{
    [Authorize(Roles = nameof(Roles.Admin))]
    public class BookController : Controller
    {
        private readonly IBookRepository _bookRepo;
        private readonly IGenreRepository _genreRepo;
        private readonly IFileService _fileService;
        private readonly ApplicationDbContext _db;

        public BookController(
            IBookRepository bookRepo,
            IGenreRepository genreRepo,
            IFileService fileService,
            ApplicationDbContext db)
        {
            _bookRepo = bookRepo;
            _genreRepo = genreRepo;
            _fileService = fileService;
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var books = await _bookRepo.GetBooks();
            return View(books);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var book = await _bookRepo.GetBookById(id);
            if (book == null)
            {
                return NotFound();
            }

            // Get approved reviews for this book
            var reviews = await _db.Reviews
                .Include(r => r.User)
                .Where(r => r.BookId == id && r.IsApproved)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();

            var model = new BookDetailsViewModel
            {
                Book = book,
                Reviews = reviews,
                AverageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0,
                TotalReviews = reviews.Count
            };

            return View(model);
        }

        public async Task<IActionResult> AddBook()
        {
            var genres = await _genreRepo.GetGenres();

            BookDTO model = new()
            {
                GenreList = genres.Select(g => new SelectListItem
                {
                    Text = g.GenreName,
                    Value = g.Id.ToString()
                })
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddBook(BookDTO bookToAdd)
        {
            bookToAdd.GenreList = (await _genreRepo.GetGenres())
                .Select(g => new SelectListItem
                {
                    Text = g.GenreName,
                    Value = g.Id.ToString()
                });

            if (!ModelState.IsValid)
                return View(bookToAdd);

            try
            {
                if (bookToAdd.ImageFile != null)
                {
                    if (bookToAdd.ImageFile.Length > 1 * 1024 * 1024)
                        throw new InvalidOperationException("Image file cannot exceed 1 MB");

                    string[] allowedExtensions = [".jpeg", ".jpg", ".png"];

                    bookToAdd.Image = await _fileService.SaveFile(bookToAdd.ImageFile, allowedExtensions);
                }

                Book book = new()
                {
                    BookName = bookToAdd.BookName,
                    AuthorName = bookToAdd.AuthorName,
                    GenreId = bookToAdd.GenreId,
                    Price = bookToAdd.Price,
                    Image = bookToAdd.Image
                };

                await _bookRepo.AddBook(book);

                TempData["successMessage"] = "Book added successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["errorMessage"] = ex.Message;
                return View(bookToAdd);
            }
        }

        public async Task<IActionResult> UpdateBook(int id)
        {
            var book = await _bookRepo.GetBookById(id);

            if (book == null)
            {
                TempData["errorMessage"] = "Book not found";
                return RedirectToAction(nameof(Index));
            }

            var genres = await _genreRepo.GetGenres();

            BookDTO model = new()
            {
                Id = book.Id,
                BookName = book.BookName,
                AuthorName = book.AuthorName,
                GenreId = book.GenreId,
                Price = book.Price,
                Image = book.Image,
                GenreList = genres.Select(g => new SelectListItem
                {
                    Text = g.GenreName,
                    Value = g.Id.ToString(),
                    Selected = g.Id == book.GenreId
                })
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateBook(BookDTO bookToUpdate)
        {
            bookToUpdate.GenreList = (await _genreRepo.GetGenres())
                .Select(g => new SelectListItem
                {
                    Text = g.GenreName,
                    Value = g.Id.ToString(),
                    Selected = g.Id == bookToUpdate.GenreId
                });

            if (!ModelState.IsValid)
                return View(bookToUpdate);

            try
            {
                string? oldImage = bookToUpdate.Image;

                if (bookToUpdate.ImageFile != null)
                {
                    if (bookToUpdate.ImageFile.Length > 1 * 1024 * 1024)
                        throw new InvalidOperationException("Image file cannot exceed 1 MB");

                    string[] allowedExtensions = [".jpeg", ".jpg", ".png"];

                    bookToUpdate.Image = await _fileService.SaveFile(bookToUpdate.ImageFile, allowedExtensions);
                }

                Book book = new()
                {
                    Id = bookToUpdate.Id,
                    BookName = bookToUpdate.BookName,
                    AuthorName = bookToUpdate.AuthorName,
                    GenreId = bookToUpdate.GenreId,
                    Price = bookToUpdate.Price,
                    Image = bookToUpdate.Image
                };

                await _bookRepo.UpdateBook(book);

                if (!string.IsNullOrWhiteSpace(oldImage) && oldImage != bookToUpdate.Image)
                    _fileService.DeleteFile(oldImage);

                TempData["successMessage"] = "Book updated successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["errorMessage"] = ex.Message;
                return View(bookToUpdate);
            }
        }

        public async Task<IActionResult> DeleteBook(int id)
        {
            try
            {
                var book = await _bookRepo.GetBookById(id);

                if (book == null)
                {
                    TempData["errorMessage"] = "Book not found";
                    return RedirectToAction(nameof(Index));
                }

                await _bookRepo.DeleteBook(book);

                if (!string.IsNullOrWhiteSpace(book.Image))
                    _fileService.DeleteFile(book.Image);

                TempData["successMessage"] = "Book deleted successfully";
            }
            catch (Exception ex)
            {
                TempData["errorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}