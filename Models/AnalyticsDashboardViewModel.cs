using DSJsBookStore.Models;

namespace DSJsBookStore.Models
{
    public class MonthlySalesPoint
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal Sales { get; set; }
    }

    public class TopSellingBookStat
    {
        public string BookName { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public int TotalSold { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class AnalyticsDashboardViewModel
    {
        public decimal TotalSales { get; set; }
        public int TotalOrders { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalBooks { get; set; }

        public IEnumerable<MonthlySalesPoint> MonthlySales { get; set; } = new List<MonthlySalesPoint>();
        public IEnumerable<TopSellingBookStat> TopSellingBooks { get; set; } = new List<TopSellingBookStat>();
        public IEnumerable<Order> RecentOrders { get; set; } = new List<Order>();
        public IEnumerable<Stock> LowStockBooks { get; set; } = new List<Stock>();
    }
}
