namespace E_commerce.Domain.Entities;

public class ProductCategory : BaseEntity
{
    public string Name { get; private set; } // Category name (e.g., "Electronics", "Laptops")
    public string Description { get; private set; } // Category description
    public string? ImageUrl { get; private set; } // Category image/banner URL (optional)
    public Guid? ParentCategoryId { get; private set; } // For hierarchical categories (e.g., "Laptops" parent is "Electronics")
    public bool IsActive { get; private set; } // Whether category is active and visible to customers
    
    private readonly List<Product> _products = new();
    public IReadOnlyCollection<Product> Products => _products.AsReadOnly();

    private ProductCategory() { } // EF Core

    public ProductCategory(string name, string description, Guid? parentCategoryId = null, string? imageUrl = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Category name cannot be empty", nameof(name));

        Name = name;
        Description = description ?? string.Empty;
        ParentCategoryId = parentCategoryId;
        ImageUrl = imageUrl;
        IsActive = true;
    }

    public void UpdateDetails(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Category name cannot be empty", nameof(name));

        Name = name;
        Description = description ?? string.Empty;
        UpdateTimestamp();
    }

    public void UpdateImage(string imageUrl)
    {
        ImageUrl = imageUrl;
        UpdateTimestamp();
    }

    public void Activate()
    {
        IsActive = true;
        UpdateTimestamp();
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdateTimestamp();
    }
}