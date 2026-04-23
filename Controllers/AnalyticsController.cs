using DSJsBookStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DSJsBookStore.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AnalyticsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AnalyticsController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Dashboard()
        {
            var sinceDate = DateTime.UtcNow.AddMonths(-12);

            var totalSales = await _db.OrderDetails
                .SumAsync(od => (decimal?)od.UnitPrice * od.Quantity) ?? 0m;

            var totalOrders = await _db.Orders.CountAsync();
            var totalCustomers = await _db.Users.CountAsync();
            var totalBooks = await _db.Books.CountAsync();

            var monthlySales = await _db.OrderDetails
                .Where(od => od.Order != null && od.Order.CreateDate >= sinceDate)
                .GroupBy(od => new { od.Order!.CreateDate.Year, od.Order.CreateDate.Month })
                .Select(g => new MonthlySalesPoint
                {
                    Month = g.Key.Month,
                    Year = g.Key.Year,
                    Sales = g.Sum(od => (decimal)od.UnitPrice * od.Quantity)
                })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToListAsync();

            var topSellingBooks = await _db.OrderDetails
                .Where(od => od.Book != null)
                .GroupBy(od => new { od.BookId, od.Book!.BookName, od.Book.AuthorName })
                .Select(g => new TopSellingBookStat
                {
                    BookName = g.Key.BookName,
                    AuthorName = g.Key.AuthorName,
                    TotalSold = g.Sum(od => od.Quantity),
                    TotalRevenue = g.Sum(od => (decimal)od.UnitPrice * od.Quantity)
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(10)
                .ToListAsync();

            var recentOrders = await _db.Orders
                .Include(o => o.User)
                .Include(o => o.OrderStatus)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Book)
                .OrderByDescending(o => o.CreateDate)
                .Take(10)
                .ToListAsync();

            var lowStockBooks = await _db.Stocks
                .Include(s => s.Book)
                .Where(s => s.Quantity <= 5)
                .OrderBy(s => s.Quantity)
                .Take(10)
                .ToListAsync();

            var model = new AnalyticsDashboardViewModel
            {
                TotalSales = totalSales,
                TotalOrders = totalOrders,
                TotalCustomers = totalCustomers,
                TotalBooks = totalBooks,
                MonthlySales = monthlySales,
                TopSellingBooks = topSellingBooks,
                RecentOrders = recentOrders,
                LowStockBooks = lowStockBooks
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetSalesData()
        {
            var sinceDate = DateTime.UtcNow.AddMonths(-12);

            var salesData = await _db.OrderDetails
                .Where(od => od.Order != null && od.Order.CreateDate >= sinceDate)
                .GroupBy(od => new { od.Order!.CreateDate.Year, od.Order.CreateDate.Month })
                .Select(g => new
                {
                    Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                    Sales = g.Sum(od => od.UnitPrice * od.Quantity)
                })
                .OrderBy(x => x.Month)
                .ToListAsync();

            return Json(salesData);
        }

        [HttpGet]
        public async Task<IActionResult> GetGenreSalesData()
        {
            var genreSales = await _db.OrderDetails
                .Where(od => od.Book != null && od.Book.Genre != null)
                .GroupBy(od => od.Book!.Genre!.GenreName)
                .Select(g => new
                {
                    Genre = g.Key,
                    Sales = g.Sum(od => od.UnitPrice * od.Quantity),
                    BooksSold = g.Sum(od => od.Quantity)
                })
                .OrderByDescending(x => x.Sales)
                .ToListAsync();

            return Json(genreSales);
        }
    }
}
