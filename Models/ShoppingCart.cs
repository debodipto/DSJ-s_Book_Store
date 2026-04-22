using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DSJsBookStore.Models
{
    [Table("ShoppingCart")]

public class ShoppingCart
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public bool IsDeleted { get; set; } = false;

    public ICollection<CartDetail> CartDetails { get; set; } = new List<CartDetail>();
}
}