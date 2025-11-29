namespace E_commerce.Application.DTOs;

public class ProductCategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public bool IsActive { get; set; }
}
