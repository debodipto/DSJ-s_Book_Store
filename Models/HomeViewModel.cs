using DSJsBookStore.Models.DTOs;

namespace DSJsBookStore.Models
{
    public class HomeViewModel
    {
        public IEnumerable<BookDisplayModel> Books { get; set; } = new List<BookDisplayModel>();
        public IEnumerable<Genre> Genres { get; set; } = new List<Genre>();

        public string STerm { get; set; } = "";
        public int GenreId { get; set; }
    }
}
