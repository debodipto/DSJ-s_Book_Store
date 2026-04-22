namespace DSJsBookStore.Models.DTOs
{
    public class BookDisplayModel
    {
        public int Id { get; set; }
        public string BookName { get; set; } = "";
        public string AuthorName { get; set; } = "";
        public double Price { get; set; }
        public string? Image { get; set; }

        public int GenreId { get; set; }
        public string GenreName { get; set; } = "";

        public int Quantity { get; set; }
    }
}