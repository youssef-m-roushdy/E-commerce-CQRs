namespace E_commerce.Application.DTOs;

public class CartDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public DateTime LastModified { get; set; }
    public decimal TotalAmount { get; set; }
    public int TotalItems { get; set; }
    public List<CartItemDto> CartItems { get; set; } = new();
}
