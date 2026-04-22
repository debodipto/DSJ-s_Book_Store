using DSJsBookStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DSJsBookStore.Controllers
{
    [Authorize(Roles = "Admin")]
    public class BulkOperationsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public BulkOperationsController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> BulkUpdateStock(List<int> bookIds, int stockChange, string operation)
        {
            if (!bookIds.Any())
            {
                TempData["Error"] = "No books selected.";
                return RedirectToAction("Index");
            }

            try
            {
                var books = await _db.Stocks.Where(s => bookIds.Contains(s.BookId)).ToListAsync();

                foreach (var stock in books)
                {
                    if (operation == "add")
                    {
                        stock.Quantity += stockChange;
                    }
                    else if (operation == "set")
                    {
                        stock.Quantity = stockChange;
                    }
                    else if (operation == "subtract")
                    {
                        stock.Quantity = Math.Max(0, stock.Quantity - stockChange);
                    }
                }

                await _db.SaveChangesAsync();
                TempData["Success"] = $"Stock updated for {books.Count} books.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error updating stock: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> BulkUpdatePrice(List<int> bookIds, double priceChange, string operation)
        {
            if (!bookIds.Any())
            {
                TempData["Error"] = "No books selected.";
                return RedirectToAction("Index");
            }

            try
            {
                var books = await _db.Books.Where(b => bookIds.Contains(b.Id)).ToListAsync();

                foreach (var book in books)
                {
                    if (operation == "add")
                    {
                        book.Price += priceChange;
                    }
                    else if (operation == "set")
                    {
                        book.Price = priceChange;
                    }
                    else if (operation == "multiply")
                    {
                        book.Price *= priceChange;
                    }
                }

                await _db.SaveChangesAsync();
                TempData["Success"] = $"Price updated for {books.Count} books.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error updating prices: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> BulkDeleteBooks(List<int> bookIds)
        {
            if (!bookIds.Any())
            {
                TempData["Error"] = "No books selected.";
                return RedirectToAction("Index");
            }

            try
            {
                var books = await _db.Books.Where(b => bookIds.Contains(b.Id)).ToListAsync();
                var stocks = await _db.Stocks.Where(s => bookIds.Contains(s.BookId)).ToListAsync();

                _db.Stocks.RemoveRange(stocks);
                _db.Books.RemoveRange(books);

                await _db.SaveChangesAsync();
                TempData["Success"] = $"Deleted {books.Count} books and their stock records.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting books: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> BulkUpdateOrderStatus(List<int> orderIds, OrderStatus newStatus)
        {
            if (!orderIds.Any())
            {
                TempData["Error"] = "No orders selected.";
                return RedirectToAction("Index");
            }

            try
            {
                var orders = await _db.Orders.Where(o => orderIds.Contains(o.Id)).ToListAsync();

                foreach (var order in orders)
                {
                    order.OrderStatus = newStatus;
                }

                await _db.SaveChangesAsync();
                TempData["Success"] = $"Updated status for {orders.Count} orders to {newStatus}.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error updating order status: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> GetBooksData()
        {
            var books = await _db.Books
                .Include(b => b.Genre)
                .Include(b => b.Stock)
                .Select(b => new
                {
                    b.Id,
                    b.BookName,
                    b.AuthorName,
                    b.Price,
                    GenreName = b.Genre.GenreName,
                    StockQuantity = b.Stock != null ? b.Stock.Quantity : 0
                })
                .ToListAsync();

            return Json(books);
        }

        [HttpGet]
        public async Task<IActionResult> GetOrdersData()
        {
            var orders = await _db.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Book)
                .Select(o => new
                {
                    o.Id,
                    CustomerName = o.User.UserName,
                    TotalAmount = o.OrderDetails.Sum(od => od.Book.Price * od.Quantity),
                    ItemCount = o.OrderDetails.Sum(od => od.Quantity),
                    o.OrderStatus,
                    o.CreateDate
                })
                .ToListAsync();

            return Json(orders);
        }
    }
}