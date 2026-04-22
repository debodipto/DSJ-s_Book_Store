using DSJsBookStore.Models.DTOs;

namespace DSJsBookStore.Models
{
    public class BookDetailsViewModel
    {
        public Book Book { get; set; } = new();
        public IEnumerable<Review> Reviews { get; set; } = new List<Review>();
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
    }
}