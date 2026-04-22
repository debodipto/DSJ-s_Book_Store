using DSJsBookStore.Models.DTOs;

namespace DSJsBookStore.Repositories
{
    public interface IReportRepository
    {
        Task<IEnumerable<TopNSoldBookModels>> GetTopNSellingBooksByDate(DateTime startDate, DateTime endDate);
    }
}
