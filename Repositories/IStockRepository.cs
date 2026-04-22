using DSJsBookStore.Models;
using DSJsBookStore.Models.DTOs;

namespace DSJsBookStore.Repositories
{
    public interface IStockRepository
    {
        Task<IEnumerable<StockDisplayModel>> GetStocks(string sTerm = "");
        Task<Stock?> GetStockByBookId(int bookId);
        Task ManageStock(StockDTO stockToManage);
    }
}
