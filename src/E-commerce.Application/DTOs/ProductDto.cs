namespace E_commerce.Application.DTOs;

public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = string.Empty;
    public int Stock { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? Sku { get; set; }
    public int LowStockThreshold { get; set; }
    public bool IsAvailableForPurchase { get; set; }
}
