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
            // Get total sales
            var totalSales = await _db.OrderDetails
                .SumAsync(od => od.Book.Price * od.Quantity);

            // Get total orders
            var totalOrders = await _db.Orders.CountAsync();

            // Get total customers
            var totalCustomers = await _db.Users.CountAsync();

            // Get total books
            var totalBooks = await _db.Books.CountAsync();

            // Get monthly sales for the last 12 months
            var monthlySales = await _db.Orders
                .Where(o => o.CreateDate >= DateTime.Now.AddMonths(-12))
                .GroupBy(o => new { o.CreateDate.Year, o.CreateDate.Month })
                .Select(g => new
                {
                    Month = g.Key.Month,
                    Year = g.Key.Year,
                    Sales = g.Sum(o => o.OrderDetails.Sum(od => od.Book.Price * od.Quantity))
                })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToListAsync();

            // Get top selling books
            var topSellingBooks = await _db.OrderDetails
                .GroupBy(od => od.Book)
                .Select(g => new
                {
                    Book = g.Key,
                    TotalSold = g.Sum(od => od.Quantity),
                    TotalRevenue = g.Sum(od => od.Book.Price * od.Quantity)
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(10)
                .ToListAsync();

            // Get recent orders
            var recentOrders = await _db.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Book)
                .OrderByDescending(o => o.CreateDate)
                .Take(10)
                .ToListAsync();

            // Get low stock alerts
            var lowStockBooks = await _db.Stocks
                .Include(s => s.Book)
                .Where(s => s.Quantity <= 5)
                .OrderBy(s => s.Quantity)
                .Take(10)
                .ToListAsync();

            var model = new AnalyticsDashboardViewModel
            {
                TotalSales = (decimal)totalSales,
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
            var salesData = await _db.Orders
                .Where(o => o.CreateDate >= DateTime.Now.AddMonths(-12))
                .GroupBy(o => new { o.CreateDate.Year, o.CreateDate.Month })
                .Select(g => new
                {
                    Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                    Sales = g.Sum(o => o.OrderDetails.Sum(od => od.Book.Price * od.Quantity))
                })
                .OrderBy(x => x.Month)
                .ToListAsync();

            return Json(salesData);
        }

        [HttpGet]
        public async Task<IActionResult> GetGenreSalesData()
        {
            var genreSales = await _db.OrderDetails
                .Include(od => od.Book)
                .ThenInclude(b => b.Genre)
                .GroupBy(od => od.Book.Genre.GenreName)
                .Select(g => new
                {
                    Genre = g.Key,
                    Sales = g.Sum(od => od.Book.Price * od.Quantity),
                    BooksSold = g.Sum(od => od.Quantity)
                })
                .OrderByDescending(x => x.Sales)
                .ToListAsync();

            return Json(genreSales);
        }
    }
}