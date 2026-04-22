using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DSJsBookStore.Models
{
    [Table("OrderDetail")]
    public class OrderDetails   // ✅ FIX: plural removed
    {
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }

        [Required]
        public int BookId { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public double UnitPrice { get; set; }

        // navigation
        public Order? Order { get; set; }
        public Book? Book { get; set; }
    }
}
    
