namespace E_commerce.Application.DTOs;

public class OrderDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal TotalAmount { get; set; }
    public AddressDto ShippingAddress { get; set; } = new();
    public string? Notes { get; set; }
    public List<OrderItemDto> OrderItems { get; set; } = new();
}
