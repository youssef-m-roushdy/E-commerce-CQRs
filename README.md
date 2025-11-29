# E-Commerce CQRS Architecture

## 📋 Table of Contents
- [Architecture Overview](#architecture-overview)
- [Project Structure](#project-structure)
- [Layer Responsibilities](#layer-responsibilities)
- [CQRS Pattern Explanation](#cqrs-pattern-explanation)
- [Folder Structure Details](#folder-structure-details)
- [Getting Started](#getting-started)
- [Best Practices](#best-practices)

---

## 🏗️ Architecture Overview

This project follows **Clean Architecture** principles combined with the **CQRS (Command Query Responsibility Segregation)** pattern. The architecture is designed to be:

- **Independent of Frameworks**: Business logic doesn't depend on external libraries
- **Testable**: Business rules can be tested without UI, database, or external services
- **Independent of UI**: UI can change without affecting business logic
- **Independent of Database**: Can swap databases without affecting business rules
- **Independent of External Services**: Business logic is isolated from external concerns

### Architecture Layers (Dependency Flow)

```
┌─────────────────────────────────────────────────────────┐
│                      E-commerce.API                     │
│                  (Presentation Layer)                   │
│              Controllers, Middleware, Filters           │
└───────────────────────┬─────────────────────────────────┘
                        │ depends on
                        ▼
┌─────────────────────────────────────────────────────────┐
│                 E-commerce.Infrastructure               │
│                  (Infrastructure Layer)                 │
│        DbContext, Repositories, External Services       │
└───────────────────────┬─────────────────────────────────┘
                        │ depends on
                        ▼
┌─────────────────────────────────────────────────────────┐
│                 E-commerce.Application                  │
│                   (Application Layer)                   │
│         Commands, Queries, Handlers, Interfaces         │
└───────────────────────┬─────────────────────────────────┘
                        │ depends on
                        ▼
┌─────────────────────────────────────────────────────────┐
│                   E-commerce.Domain                     │
│                     (Domain Layer)                      │
│          Entities, Value Objects, Domain Logic          │
└─────────────────────────────────────────────────────────┘
```

---

## 📁 Project Structure

```
E-commerce/
├── src/
│   ├── E-commerce.Domain/           # Core business logic
│   ├── E-commerce.Application/      # Use cases (CQRS)
│   ├── E-commerce.Infrastructure/   # Data access & external services
│   └── E-commerce.API/              # Web API endpoints
└── E-commerce.sln
```

---

## 🎯 Layer Responsibilities

### 1️⃣ Domain Layer (`E-commerce.Domain`)

**Purpose**: Contains enterprise-wide business rules and entities.

**What belongs here**:
- ✅ Domain entities
- ✅ Value objects
- ✅ Domain events
- ✅ Domain interfaces
- ✅ Domain exceptions
- ✅ Enums

**What DOESN'T belong here**:
- ❌ Database concerns
- ❌ External service integrations
- ❌ UI logic
- ❌ Framework dependencies

**Folder Structure**:
```
E-commerce.Domain/
├── Entities/              # Aggregate roots and entities
│   ├── BaseEntity.cs
│   ├── Product.cs
│   ├── Order.cs
│   └── Customer.cs
├── ValueObjects/          # Immutable value objects
│   ├── Address.cs
│   ├── Money.cs
│   └── Email.cs
├── Enums/                 # Domain enumerations
│   ├── OrderStatus.cs
│   └── PaymentStatus.cs
├── Events/                # Domain events
│   ├── OrderCreatedEvent.cs
│   └── ProductStockChangedEvent.cs
└── Interfaces/            # Domain contracts
    └── IRepository.cs
```

**Example Entity**:
```csharp
public class Product : BaseEntity
{
    public string Name { get; private set; }
    public decimal Price { get; private set; }
    
    public void UpdatePrice(decimal newPrice)
    {
        if (newPrice <= 0)
            throw new DomainException("Price must be positive");
        Price = newPrice;
    }
}
```

---

### 2️⃣ Application Layer (`E-commerce.Application`)

**Purpose**: Contains application-specific business rules and orchestrates the flow of data.

**What belongs here**:
- ✅ Commands (write operations)
- ✅ Queries (read operations)
- ✅ Command/Query handlers
- ✅ DTOs (Data Transfer Objects)
- ✅ Validators
- ✅ Application interfaces
- ✅ Mapping profiles
- ✅ Behaviors (logging, validation, transaction)

**What DOESN'T belong here**:
- ❌ Database implementation
- ❌ HTTP concerns
- ❌ External service implementations

**Folder Structure**:
```
E-commerce.Application/
├── Commands/              # Write operations
│   ├── Products/
│   │   ├── CreateProductCommand.cs
│   │   ├── CreateProductCommandHandler.cs
│   │   ├── UpdateProductCommand.cs
│   │   └── DeleteProductCommand.cs
│   ├── Orders/
│   │   ├── CreateOrderCommand.cs
│   │   └── CancelOrderCommand.cs
│   └── Customers/
│       └── RegisterCustomerCommand.cs
├── Queries/               # Read operations
│   ├── Products/
│   │   ├── GetProductByIdQuery.cs
│   │   ├── GetProductByIdQueryHandler.cs
│   │   ├── GetProductsListQuery.cs
│   │   └── GetProductsByCategoryQuery.cs
│   ├── Orders/
│   │   ├── GetOrderByIdQuery.cs
│   │   └── GetCustomerOrdersQuery.cs
│   └── Customers/
│       └── GetCustomerByEmailQuery.cs
├── Common/
│   ├── Interfaces/        # Application abstractions
│   │   ├── ICommand.cs
│   │   ├── IQuery.cs
│   │   ├── ICommandHandler.cs
│   │   ├── IQueryHandler.cs
│   │   └── IApplicationDbContext.cs
│   ├── Behaviors/         # MediatR pipeline behaviors
│   │   ├── ValidationBehavior.cs
│   │   ├── LoggingBehavior.cs
│   │   └── TransactionBehavior.cs
│   ├── Exceptions/        # Application exceptions
│   │   ├── ValidationException.cs
│   │   └── NotFoundException.cs
│   └── Models/            # Common models
│       └── PaginatedList.cs
├── DTOs/                  # Data transfer objects
│   ├── ProductDto.cs
│   ├── OrderDto.cs
│   └── CustomerDto.cs
└── Validators/            # FluentValidation validators
    ├── CreateProductCommandValidator.cs
    └── CreateOrderCommandValidator.cs
```

**Key Concepts**:

#### Commands (Write Operations)
Commands represent **intentions to change state**. They modify data.

```csharp
// Command definition
public record CreateProductCommand(
    string Name,
    string Description,
    decimal Price,
    int Stock,
    string Category
) : ICommand<Guid>;

// Command handler
public class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public async Task<Guid> Handle(CreateProductCommand request, CancellationToken ct)
    {
        var product = new Product(request.Name, request.Description, 
                                  request.Price, request.Stock, request.Category);
        
        _context.Products.Add(product);
        await _context.SaveChangesAsync(ct);
        
        return product.Id;
    }
}
```

#### Queries (Read Operations)
Queries represent **requests for information**. They don't modify data.

```csharp
// Query definition
public record GetProductByIdQuery(Guid Id) : IQuery<ProductDto?>;

// Query handler
public class GetProductByIdQueryHandler : IQueryHandler<GetProductByIdQuery, ProductDto?>
{
    private readonly IApplicationDbContext _context;

    public async Task<ProductDto?> Handle(GetProductByIdQuery request, CancellationToken ct)
    {
        return await _context.Products
            .Where(p => p.Id == request.Id)
            .Select(p => new ProductDto { /* mapping */ })
            .FirstOrDefaultAsync(ct);
    }
}
```

---

### 3️⃣ Infrastructure Layer (`E-commerce.Infrastructure`)

**Purpose**: Implements interfaces defined in Application layer. Handles all external concerns.

**What belongs here**:
- ✅ Database context (EF Core)
- ✅ Entity configurations
- ✅ Repository implementations
- ✅ External service implementations
- ✅ File storage services
- ✅ Email services
- ✅ Caching implementations
- ✅ Identity/Authentication

**What DOESN'T belong here**:
- ❌ Business logic
- ❌ HTTP/API concerns

**Folder Structure**:
```
E-commerce.Infrastructure/
├── Persistence/
│   ├── ApplicationDbContext.cs       # EF Core DbContext
│   ├── Configurations/               # Entity configurations
│   │   ├── ProductConfiguration.cs
│   │   ├── OrderConfiguration.cs
│   │   └── CustomerConfiguration.cs
│   ├── Migrations/                   # EF Core migrations
│   │   └── [auto-generated]
│   └── Repositories/                 # Repository implementations
│       ├── ProductRepository.cs
│       └── OrderRepository.cs
├── Services/
│   ├── Email/                        # Email service
│   │   ├── EmailService.cs
│   │   └── EmailTemplates/
│   ├── Storage/                      # File storage
│   │   ├── BlobStorageService.cs
│   │   └── LocalStorageService.cs
│   └── Cache/                        # Caching
│       └── RedisCacheService.cs
└── Identity/                         # Authentication/Authorization
    ├── ApplicationUser.cs
    └── IdentityService.cs
```

**Example DbContext**:
```csharp
public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply all configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
```

**Example Entity Configuration**:
```csharp
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Price).HasPrecision(18, 2);
        builder.HasQueryFilter(p => !p.IsDeleted); // Global query filter
    }
}
```

---

### 4️⃣ API Layer (`E-commerce.API`)

**Purpose**: Entry point of the application. Handles HTTP requests/responses.

**What belongs here**:
- ✅ Controllers
- ✅ Middleware
- ✅ Filters
- ✅ API models (request/response)
- ✅ Dependency injection configuration
- ✅ API documentation setup

**What DOESN'T belong here**:
- ❌ Business logic
- ❌ Data access logic
- ❌ Complex validation logic

**Folder Structure**:
```
E-commerce.API/
├── Controllers/           # API endpoints
│   ├── BaseApiController.cs
│   ├── ProductsController.cs
│   ├── OrdersController.cs
│   └── CustomersController.cs
├── Middleware/            # Custom middleware
│   ├── ExceptionHandlingMiddleware.cs
│   └── RequestLoggingMiddleware.cs
├── Filters/               # Action filters
│   ├── ValidationFilter.cs
│   └── AuthorizationFilter.cs
├── Extensions/            # Service registration
│   ├── ServiceCollectionExtensions.cs
│   └── ApplicationBuilderExtensions.cs
├── Program.cs
└── appsettings.json
```

**Example Controller**:
```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : BaseApiController
{
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetProductByIdQuery(id);
        var result = await Mediator.Send(query);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductCommand command)
    {
        var productId = await Mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = productId }, productId);
    }
}
```

---

## 🔄 CQRS Pattern Explanation

### What is CQRS?

**CQRS (Command Query Responsibility Segregation)** separates read and write operations into different models:

- **Commands**: Change state (Create, Update, Delete)
- **Queries**: Return data without changing state (Read)

### Benefits of CQRS

1. **Separation of Concerns**: Read and write logic are separate
2. **Scalability**: Can optimize and scale reads/writes independently
3. **Performance**: Read models can be denormalized for speed
4. **Security**: Different permissions for reads vs writes
5. **Maintainability**: Easier to understand and modify

### CQRS Flow in This Project

```
┌─────────────┐
│   Client    │
└──────┬──────┘
       │
       │ HTTP Request
       ▼
┌─────────────────┐
│   Controller    │
└──────┬──────────┘
       │
       │ Command/Query
       ▼
┌─────────────────┐
│    MediatR      │ (In-process messaging)
└──────┬──────────┘
       │
       ├─────Command────▶ Handler ──▶ Write to DB
       │
       └─────Query─────▶ Handler ──▶ Read from DB
```

### When to Use Commands vs Queries

| Operation | Type | Returns Data? | Changes State? |
|-----------|------|---------------|----------------|
| Create Product | Command | ✅ (ID) | ✅ Yes |
| Update Product | Command | ❌ or ✅ | ✅ Yes |
| Delete Product | Command | ❌ | ✅ Yes |
| Get Product | Query | ✅ Yes | ❌ No |
| List Products | Query | ✅ Yes | ❌ No |
| Search Products | Query | ✅ Yes | ❌ No |

---

## 🛠️ Getting Started

### Prerequisites

- .NET 8.0 SDK
- SQL Server / PostgreSQL (or preferred database)
- Visual Studio 2022 / VS Code / JetBrains Rider

### Setup Instructions

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd E-commerce
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Add required NuGet packages**
   
   **Application Layer**:
   ```bash
   cd src/E-commerce.Application
   dotnet add package MediatR
   dotnet add package FluentValidation
   dotnet add package FluentValidation.DependencyInjectionExtensions
   dotnet add package Microsoft.EntityFrameworkCore
   ```

   **Infrastructure Layer**:
   ```bash
   cd ../E-commerce.Infrastructure
   dotnet add package Microsoft.EntityFrameworkCore.SqlServer
   dotnet add package Microsoft.EntityFrameworkCore.Tools
   ```

   **API Layer**:
   ```bash
   cd ../E-commerce.API
   dotnet add package MediatR
   dotnet add package Swashbuckle.AspNetCore
   ```

4. **Configure database connection**
   
   Update `appsettings.json` in `E-commerce.API`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=ECommerceDb;Trusted_Connection=true;"
     }
   }
   ```

5. **Apply migrations**
   ```bash
   cd src/E-commerce.Infrastructure
   dotnet ef migrations add InitialCreate --startup-project ../E-commerce.API
   dotnet ef database update --startup-project ../E-commerce.API
   ```

6. **Run the application**
   ```bash
   cd ../E-commerce.API
   dotnet run
   ```

7. **Access Swagger UI**
   
   Navigate to: `https://localhost:5001/swagger`

---

## ✨ Best Practices

### 1. Command/Query Naming Conventions

**Commands** (Write):
- `CreateProductCommand`
- `UpdateProductCommand`
- `DeleteProductCommand`
- `CancelOrderCommand`

**Queries** (Read):
- `GetProductByIdQuery`
- `GetProductsListQuery`
- `GetOrdersByCustomerQuery`
- `SearchProductsQuery`

### 2. Keep Handlers Simple

Each handler should:
- Have a single responsibility
- Be easy to test
- Not contain business logic (that belongs in Domain)
- Use dependency injection for services

### 3. Use Records for Commands/Queries

Records are immutable and perfect for CQRS:
```csharp
public record CreateProductCommand(string Name, decimal Price) : ICommand<Guid>;
```

### 4. Validate at Application Layer

Use FluentValidation for input validation:
```csharp
public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Price).GreaterThan(0);
    }
}
```

### 5. Use DTOs for Responses

Never expose domain entities directly:
```csharp
// ❌ Bad
public async Task<Product> GetProduct() { ... }

// ✅ Good
public async Task<ProductDto> GetProduct() { ... }
```

### 6. Handle Errors Gracefully

Use custom exceptions and global exception handling:
```csharp
public class NotFoundException : Exception { }
public class ValidationException : Exception { }
```

### 7. Keep Domain Layer Pure

Domain should have no dependencies:
- No `using Microsoft.EntityFrameworkCore`
- No `using System.Net.Http`
- Only business logic and domain concepts

---

## 📚 Additional Resources

- [Clean Architecture by Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [CQRS Pattern by Martin Fowler](https://martinfowler.com/bliki/CQRS.html)
- [MediatR Documentation](https://github.com/jbogard/MediatR)
- [Entity Framework Core Documentation](https://learn.microsoft.com/en-us/ef/core/)

---

## 📝 License

This project is licensed under the MIT License.
