using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DSJsBookStore.Models
{
    [Table("Book")]
    public class Book
    {
        public int Id { get; set; }

        [Required, MaxLength(40)]
        public string BookName { get; set; } = string.Empty;

        [Required, MaxLength(40)]
        public string AuthorName { get; set; } = string.Empty;

        [Required]
        public double Price { get; set; }

        public string? Image { get; set; }

        [Required]
        public int GenreId { get; set; }

        // ? FIX: make nullable for EF Core safety
        public Genre? Genre { get; set; }

        public List<OrderDetails> OrderDetails { get; set; } = new();
        public List<CartDetail> CartDetails { get; set; } = new();

        // ?? Stock should also be nullable (VERY IMPORTANT)
        public Stock? Stock { get; set; }

        [NotMapped]
        public string GenreName { get; set; } = string.Empty;

        [NotMapped]
        public int Quantity { get; set; }
    }
}
