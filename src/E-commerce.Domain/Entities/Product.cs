namespace E_commerce.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public decimal Price { get; private set; }
    public int Stock { get; private set; }
    public string Category { get; private set; }

    private Product() { } // EF Core

    public Product(string name, string description, decimal price, int stock, string category)
    {
        Name = name;
        Description = description;
        Price = price;
        Stock = stock;
        Category = category;
    }

    public void UpdateDetails(string name, string description, decimal price, string category)
    {
        Name = name;
        Description = description;
        Price = price;
        Category = category;
        UpdateTimestamp();
    }

    public void UpdateStock(int quantity)
    {
        Stock = quantity;
        UpdateTimestamp();
    }

    public bool HasStock(int quantity) => Stock >= quantity;

    public void ReduceStock(int quantity)
    {
        if (!HasStock(quantity))
            throw new InvalidOperationException("Insufficient stock");
        
        Stock -= quantity;
        UpdateTimestamp();
    }
}
