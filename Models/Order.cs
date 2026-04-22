using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace DSJsBookStore.Models
{
    [Table("Order")]
    public class Order
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty; // ✅ FIX

        public DateTime CreateDate { get; set; } = DateTime.UtcNow;

        [Required]
        public int OrderStatusId { get; set; }

        public bool IsDeleted { get; set; } = false;

        [Required, MaxLength(30)]
        public string Name { get; set; } = string.Empty; // ✅ FIX

        [Required, EmailAddress, MaxLength(30)]
        public string Email { get; set; } = string.Empty; // ✅ FIX

        [Required]
        public string MobileNumber { get; set; } = string.Empty; // ✅ FIX

        [Required, MaxLength(200)]
        public string Address { get; set; } = string.Empty; // ✅ FIX

        [Required, MaxLength(30)]
        public string PaymentMethod { get; set; } = string.Empty; // ✅ FIX

        public bool IsPaid { get; set; }

        // ✅ Navigation properties FIX
        public IdentityUser? User { get; set; }
        public OrderStatus? OrderStatus { get; set; }

        public List<OrderDetails> OrderDetails { get; set; } = new();
    }
}