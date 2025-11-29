# Entity vs Value Object - Complete Guide

## 🎯 The Golden Rule

**Ask this ONE question**: *"Do I care about WHICH ONE it is, or just WHAT it contains?"*

---

## 📊 Quick Decision Table

| Concept | Entity or Value Object? | Why? |
|---------|------------------------|------|
| Product | ✅ **ENTITY** | Each product has unique identity (need to track WHICH product) |
| Order | ✅ **ENTITY** | Each order is unique (need to track WHICH order) |
| Customer | ✅ **ENTITY** | Each person is unique (need to track WHO) |
| Payment | ✅ **ENTITY** | Each transaction is unique (need to track WHICH payment) |
| Cart | ✅ **ENTITY** | Each cart belongs to someone (need to track WHICH cart) |
| **Address** | ⚠️ **VALUE OBJECT** | Only care WHAT the address is, not which one |
| **Money** | ⚠️ **VALUE OBJECT** | $100 is $100, identity doesn't matter |
| **Email** | ⚠️ **VALUE OBJECT** | Only the value matters |
| **PhoneNumber** | ⚠️ **VALUE OBJECT** | Only the value matters |
| **DateRange** | ⚠️ **VALUE OBJECT** | Only the dates matter |

---

## 🔍 Deep Dive: Key Differences

### **ENTITY** ✅
- **Has Identity** (ID/GUID)
- **Has Lifecycle** (created, modified, deleted)
- **Mutable** (can change over time)
- **Compared by ID** (two products with same name but different IDs are different)
- **Has History** (track changes over time)
- **Inherits from BaseEntity**

**Example - Product is an Entity:**
```csharp
// These are DIFFERENT products even with same properties
var product1 = new Product("iPhone", 999m); // Id: abc123
var product2 = new Product("iPhone", 999m); // Id: xyz789

product1 == product2 // FALSE! Different IDs = different products

// Track changes
product1.UpdatePrice(899m); // Changes THIS specific product
```

---

### **VALUE OBJECT** ⚠️
- **NO Identity** (no ID needed)
- **Immutable** (never changes, create new instead)
- **Compared by Value** (two addresses with same street/city are equal)
- **Replaceable** (if address changes, replace whole object)
- **NO History** (don't track changes)
- **Does NOT inherit from BaseEntity**

**Example - Address is a Value Object:**
```csharp
// These are THE SAME address (same values)
var address1 = new Address("123 Main St", "New York", "10001");
var address2 = new Address("123 Main St", "New York", "10001");

address1 == address2 // TRUE! Same values = equal

// To "change" address, you REPLACE it entirely
customer.Address = new Address("456 Oak Ave", "Boston", "02101");
```

---

## 🏗️ Implementation Patterns

### ✅ **Entity Pattern** (Mutable, Has Identity)

```csharp
// Inherits from BaseEntity → Gets Id, CreatedAt, UpdatedAt
public class Product : BaseEntity
{
    // Properties with private setters (controlled mutation)
    public string Name { get; private set; }
    public decimal Price { get; private set; }
    
    // Constructor
    public Product(string name, decimal price)
    {
        Name = name;
        Price = price;
        // Id is auto-generated in BaseEntity
    }
    
    // Methods to change state (mutable)
    public void UpdatePrice(decimal newPrice)
    {
        Price = newPrice;
        UpdateTimestamp(); // Track when it changed
    }
    
    public void Rename(string newName)
    {
        Name = newName;
        UpdateTimestamp();
    }
}

// Usage:
var product = new Product("Laptop", 999m);
product.UpdatePrice(899m); // Mutate the SAME product
// Still the same product, just different price
```

---

### ⚠️ **Value Object Pattern** (Immutable, No Identity)

```csharp
// Does NOT inherit from BaseEntity (no Id needed!)
public class Address : ValueObject
{
    // Properties with private setters + init (immutable after creation)
    public string Street { get; private init; }
    public string City { get; private init; }
    public string ZipCode { get; private init; }
    public string Country { get; private init; }
    
    // Constructor - only way to create
    public Address(string street, string city, string zipCode, string country)
    {
        Street = street;
        City = city;
        ZipCode = zipCode;
        Country = country;
    }
    
    // NO mutation methods! Create new instead
    public Address ChangeCity(string newCity)
    {
        return new Address(Street, newCity, ZipCode, Country);
    }
    
    // Value objects must implement equality by value
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return ZipCode;
        yield return Country;
    }
}

// Usage:
var address1 = new Address("123 Main", "NYC", "10001", "USA");
// Can't do: address1.City = "Boston"; // ❌ Immutable!

// Instead, replace entirely:
var address2 = address1.ChangeCity("Boston"); // New object
```

---

## 🔴 Common Mistakes in Your Code

### ❌ **MISTAKE 1: Address as Entity**

```csharp
// WRONG! Address in Entities folder
public class Address : BaseEntity  // ❌ No! Address shouldn't have Id
{
    public string Street { get; set; }
    public string City { get; set; }
}
```

**Why wrong?**
- You don't care WHICH address, only WHAT address
- If customer moves, you replace address, not update same one
- Two customers at same address should have equal addresses (not different IDs)

**✅ CORRECT:**
```csharp
// Move to ValueObjects folder
public class Address : ValueObject
{
    public string Street { get; private init; }
    public string City { get; private init; }
    // ... immutable, no Id
}
```

---

### ❌ **MISTAKE 2: Money as separate properties**

```csharp
// WRONG! Money scattered in entities
public class Order : BaseEntity
{
    public decimal TotalAmount { get; set; }  // ❌ What currency?
    public decimal ShippingCost { get; set; } // ❌ No validation
}
```

**✅ CORRECT:**
```csharp
// Money as Value Object
public class Money : ValueObject
{
    public decimal Amount { get; private init; }
    public string Currency { get; private init; }
    
    public Money(decimal amount, string currency)
    {
        if (amount < 0) throw new ArgumentException("Amount cannot be negative");
        Amount = amount;
        Currency = currency;
    }
    
    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot add different currencies");
        return new Money(Amount + other.Amount, Currency);
    }
}

// Usage in Entity:
public class Order : BaseEntity
{
    public Money TotalAmount { get; private set; }
    public Money ShippingCost { get; private set; }
}
```

---

## 📋 Your E-commerce Entities Analysis

### ✅ **Should Be ENTITIES** (Keep in Entities folder)

| Class | Why Entity? | Keep BaseEntity? |
|-------|-------------|------------------|
| **Product** | ✅ Each product is unique, tracked individually | ✅ YES |
| **Order** | ✅ Each order is unique transaction | ✅ YES |
| **Customer** | ✅ Each person is unique | ✅ YES |
| **Payment** | ✅ Each payment transaction is unique | ✅ YES |
| **Cart** | ✅ Each cart belongs to specific user | ✅ YES |
| **OrderItem** | ✅ Each line item in order is tracked | ✅ YES |
| **TrackingDetail** | ✅ Tracks specific shipment | ✅ YES |
| **ProductCategory** | ⚠️ Depends... | Maybe |

---

### ⚠️ **Should Be VALUE OBJECTS** (Move to ValueObjects folder)

| Class | Why Value Object? | Example |
|-------|------------------|---------|
| **Address** | ❌ Don't care which one, only what it is | `new Address("123 Main", "NYC")` |
| **Money** | ❌ $100 is $100, no identity | `new Money(100, "USD")` |
| **Email** | ❌ Only value matters | `new Email("user@email.com")` |
| **PhoneNumber** | ❌ Only value matters | `new PhoneNumber("+1234567890")` |
| **DateRange** | ❌ Just start/end dates | `new DateRange(start, end)` |
| **Dimensions** | ❌ Width/Height/Length | `new Dimensions(10, 20, 5)` |

---

## 🎯 The Test Questions

**Ask yourself these 5 questions:**

### 1. **"Does it need an ID to distinguish it from others?"**
- YES → Entity
- NO → Value Object

### 2. **"Do I need to track its lifecycle (created, modified, deleted)?"**
- YES → Entity
- NO → Value Object

### 3. **"Can two instances with same properties be considered equal?"**
- YES → Value Object (equal by value)
- NO → Entity (equal by ID)

### 4. **"When it changes, do I update it or replace it?"**
- Update → Entity
- Replace → Value Object

### 5. **"Does it make sense to share this between multiple entities?"**
- YES → Value Object (multiple customers can have same address)
- NO → Entity

---

## 💡 Real Examples from E-commerce

### Example 1: Customer and Address

```csharp
// Customer is ENTITY (has identity, lifecycle)
public class Customer : BaseEntity
{
    public string Name { get; private set; }
    public Email Email { get; private set; }        // Value Object
    public Address ShippingAddress { get; private set; } // Value Object
    public Address BillingAddress { get; private set; }  // Value Object
    
    public void UpdateShippingAddress(Address newAddress)
    {
        ShippingAddress = newAddress; // Replace, not update!
        UpdateTimestamp();
    }
}

// Address is VALUE OBJECT (no identity, immutable)
public class Address : ValueObject
{
    public string Street { get; private init; }
    public string City { get; private init; }
    public string ZipCode { get; private init; }
    
    public Address(string street, string city, string zipCode)
    {
        Street = street;
        City = city;
        ZipCode = zipCode;
    }
}
```

### Example 2: Order and Money

```csharp
// Order is ENTITY
public class Order : BaseEntity
{
    public Guid CustomerId { get; private set; }
    public Money Subtotal { get; private set; }    // Value Object
    public Money Tax { get; private set; }         // Value Object
    public Money Total { get; private set; }       // Value Object
    
    public void CalculateTotal()
    {
        Total = Subtotal.Add(Tax); // Value objects are immutable
        UpdateTimestamp();
    }
}

// Money is VALUE OBJECT
public class Money : ValueObject
{
    public decimal Amount { get; private init; }
    public string Currency { get; private init; }
    
    public Money Add(Money other)
    {
        return new Money(Amount + other.Amount, Currency);
    }
}
```

---

## 📁 Recommended Folder Structure

```
E-commerce.Domain/
├── Entities/                    # Has Id, mutable, lifecycle
│   ├── BaseEntity.cs           # Base for all entities
│   ├── Product.cs              ✅ Entity
│   ├── Order.cs                ✅ Entity
│   ├── Customer.cs             ✅ Entity
│   ├── Payment.cs              ✅ Entity
│   ├── Cart.cs                 ✅ Entity
│   ├── OrderItem.cs            ✅ Entity
│   └── TrackingDetail.cs       ✅ Entity
│
└── ValueObjects/                # No Id, immutable, compared by value
    ├── ValueObject.cs          # Base for all value objects
    ├── Address.cs              ⚠️ Move here!
    ├── Money.cs                ⚠️ Create this
    ├── Email.cs                ⚠️ Create this
    ├── PhoneNumber.cs          ⚠️ Create this
    └── ProductDimensions.cs    ⚠️ Create this
```

---

## 🛠️ Base Classes You Need

### BaseEntity (Already have!)
```csharp
public abstract class BaseEntity
{
    public Guid Id { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public DateTime? UpdatedAt { get; protected set; }
    // ... mutable, has lifecycle
}
```

### ValueObject (Need to create!)
```csharp
public abstract class ValueObject
{
    // Equals by value, not by reference
    protected abstract IEnumerable<object> GetEqualityComponents();
    
    public override bool Equals(object? obj)
    {
        if (obj == null || obj.GetType() != GetType())
            return false;
            
        var other = (ValueObject)obj;
        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }
    
    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Select(x => x?.GetHashCode() ?? 0)
            .Aggregate((x, y) => x ^ y);
    }
    
    public static bool operator ==(ValueObject? left, ValueObject? right)
    {
        return Equals(left, right);
    }
    
    public static bool operator !=(ValueObject? left, ValueObject? right)
    {
        return !Equals(left, right);
    }
}
```

---

## ✅ Action Items for Your Project

1. **Create ValueObject base class** in `Domain/ValueObjects/`
2. **Move Address** from Entities to ValueObjects
3. **Create Money value object** for prices/amounts
4. **Create Email value object** for email validation
5. **Update entities** to use value objects instead of primitives

Your entities like Product, Order, Payment, Cart should stay as entities! 🎯
