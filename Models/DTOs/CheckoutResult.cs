namespace DSJsBookStore.Models.DTOs;

public class CheckoutResult
{
    public bool Succeeded { get; set; }
    public int? OrderId { get; set; }
    public string Message { get; set; } = string.Empty;
}
