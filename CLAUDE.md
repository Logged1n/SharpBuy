# CLAUDE.md - SharpBuy AI Assistant Guide

**Last Updated:** 2025-11-16
**Project:** SharpBuy - E-commerce Platform
**Architecture:** Clean Architecture with Domain-Driven Design (DDD)
**Framework:** .NET 9.0

---

## Table of Contents

1. [Project Overview](#project-overview)
2. [Architecture & Design Principles](#architecture--design-principles)
3. [Technology Stack](#technology-stack)
4. [Project Structure](#project-structure)
5. [Development Workflows](#development-workflows)
6. [Coding Conventions & Patterns](#coding-conventions--patterns)
7. [Database & Migrations](#database--migrations)
8. [API Design Guidelines](#api-design-guidelines)
9. [Testing Strategy](#testing-strategy)
10. [Common Tasks & Examples](#common-tasks--examples)
11. [Important Files & Configurations](#important-files--configurations)
12. [Troubleshooting](#troubleshooting)

---

## Project Overview

SharpBuy is a modern e-commerce platform built using Clean Architecture and Domain-Driven Design principles. It serves as both a production-ready template and a learning resource for .NET developers.

### Core Features
- User registration and JWT authentication
- Product catalog management
- Shopping cart functionality
- Order processing
- Product reviews
- Inventory management
- Email notifications (SMTP)

### Business Domain
The platform manages several aggregates:
- **Users**: Registration, authentication, email verification, permissions
- **Products**: Product management with inventory and categories
- **Categories**: Product categorization (many-to-many)
- **Orders**: Order placement and management
- **Carts**: Shopping cart with items
- **Reviews**: Product reviews by users
- **Addresses**: User and order addresses

---

## Architecture & Design Principles

### Clean Architecture Layers

**Dependency Flow:** `Domain ← Application ← Infrastructure ← Web.Api`

```
┌─────────────────────────────────────────┐
│           Web.Api (Presentation)        │
│  - Minimal API Endpoints                │
│  - Global Exception Handler             │
│  - Middleware                           │
└──────────────────┬──────────────────────┘
                   │
┌──────────────────▼──────────────────────┐
│         Infrastructure (External)       │
│  - EF Core DbContext                    │
│  - JWT Authentication                   │
│  - Permission Authorization             │
│  - Email Service (SMTP)                 │
│  - Domain Event Dispatcher              │
└──────────────────┬──────────────────────┘
                   │
┌──────────────────▼──────────────────────┐
│        Application (Use Cases)          │
│  - CQRS Handlers (Commands/Queries)     │
│  - Validators (FluentValidation)        │
│  - Domain Event Handlers                │
│  - Cross-cutting Behaviors              │
└──────────────────┬──────────────────────┘
                   │
┌──────────────────▼──────────────────────┐
│          Domain (Business Logic)        │
│  - Entities & Aggregates                │
│  - Domain Events                        │
│  - Business Rules                       │
│  - Domain Services                      │
└──────────────────┬──────────────────────┘
                   │
┌──────────────────▼──────────────────────┐
│         SharedKernel (Common)           │
│  - Result Pattern                       │
│  - Error Types                          │
│  - Base Entity                          │
│  - Domain Abstractions                  │
└─────────────────────────────────────────┘
```

### Architectural Constraints

**CRITICAL:** Architecture tests enforce these rules. Violations will cause build failures.

1. **Domain Layer:**
   - ✅ Can reference: SharedKernel only
   - ❌ Cannot reference: Application, Infrastructure, Presentation
   - Contains pure business logic with no external dependencies

2. **Application Layer:**
   - ✅ Can reference: Domain, SharedKernel
   - ❌ Cannot reference: Infrastructure, Presentation
   - Defines abstractions that Infrastructure implements

3. **Infrastructure Layer:**
   - ✅ Can reference: Application, Domain, SharedKernel
   - ❌ Cannot reference: Presentation
   - Implements Application abstractions

4. **Presentation Layer (Web.Api):**
   - ✅ Can reference: All layers
   - Contains HTTP-specific concerns only

### Core Design Patterns

#### 1. CQRS (Command Query Responsibility Segregation)
- **Commands:** Modify state, return `Result` or `Result<T>`
- **Queries:** Read state, return `Result<T>` with DTOs
- **Handlers:** One handler per command/query
- **Validation:** FluentValidation executed via decorator pattern

#### 2. Result Pattern (Railway-Oriented Programming)
All operations return `Result` or `Result<TValue>` instead of throwing exceptions:
```csharp
// Success
Result<User> result = Result.Success(user);

// Failure with typed error
Result<User> result = Result.Failure<User>(UserErrors.NotFound(userId));

// Pattern matching
return result.Match(
    user => Results.Ok(user),
    CustomResults.Problem
);
```

#### 3. Domain-Driven Design Patterns
- **Entities:** Rich domain models with business logic
- **Value Objects:** Immutable types (e.g., `Money`)
- **Aggregates:** Transaction boundaries (Product + Inventory)
- **Domain Events:** Cross-aggregate communication
- **Repositories:** Data access via EF Core DbContext

#### 4. Decorator Pattern
Cross-cutting concerns applied via Scrutor:
- **ValidationDecorator:** Validates commands/queries
- **LoggingDecorator:** Logs handler execution

---

## Technology Stack

### Runtime & Framework
- **.NET 9.0** (`net9.0`)
- **C# Latest** with nullable reference types enabled
- **Implicit usings** enabled globally

### Database
- **PostgreSQL 17** (containerized)
- **Entity Framework Core 9.0.10**
- **Npgsql** provider (9.0.4)
- **EFCore.NamingConventions** (snake_case)

### Authentication & Authorization
- **JWT Bearer Tokens** (Microsoft.AspNetCore.Authentication.JwtBearer 9.0.10)
- **ASP.NET Core Identity** (`IdentityDbContext`)
- **Custom Permission-based Authorization** with policy provider

### API & Documentation
- **ASP.NET Core Minimal APIs**
- **Swagger/OpenAPI** (Swashbuckle.AspNetCore 9.0.1)
- **Vertical Slice Architecture** (feature-based endpoints)

### Validation
- **FluentValidation 12.0.0** with dependency injection
- **Automatic validation** via decorator pattern

### Observability
- **Serilog** (Console + Seq)
- **OpenTelemetry** with instrumentation for:
  - ASP.NET Core, HTTP, Runtime
  - Entity Framework Core, Npgsql
- **Seq** log aggregation (http://localhost:8081)

### Email
- **FluentEmail.Smtp 3.0.2**
- **Papercut SMTP** for development (via Aspire)

### Orchestration
- **.NET Aspire 9.3.1** for local development
- Orchestrates PostgreSQL, Papercut, and Web API
- Service discovery and resilience patterns

### Testing
- **xUnit 2.9.3** (test framework)
- **NetArchTest.Rules 1.3.2** (architecture tests)
- **Shouldly 4.3.0** (assertions)

### Code Quality
- **SonarAnalyzer.CSharp 10.12.0**
- **EditorConfig** with strict style rules
- **Warnings as Errors** enabled
- **Scrutor 6.1.0** (assembly scanning)

---

## Project Structure

### Root Directory
```
/home/user/SharpBuy/
├── .editorconfig                    # Code style rules (4-space, CRLF)
├── .gitignore                       # Git ignore patterns
├── Directory.Build.props            # Global MSBuild properties
├── Directory.Packages.props         # Central Package Management
├── SharpBuy.slnx                    # Solution file
├── README.md                        # Project overview
├── CLAUDE.md                        # This file
├── .github/workflows/build.yml      # CI/CD pipeline
├── scripts/
│   ├── add-migration.ps1            # Add EF migrations
│   └── update-database.ps1          # Update database
├── src/
│   ├── SharedKernel/                # Common DDD abstractions
│   ├── Domain/                      # Business logic & entities
│   ├── Application/                 # Use cases (CQRS)
│   ├── Infrastructure/              # External concerns
│   ├── Web.Api/                     # HTTP API (Minimal APIs)
│   ├── Aspire.AppHost/              # Aspire orchestration
│   └── Aspire.ServiceDefaults/      # Shared observability config
└── tests/
    └── ArchitectureTests/           # Layer dependency tests
```

### Source Projects

#### 1. SharedKernel (`/src/SharedKernel/`)
**Purpose:** Common DDD building blocks reusable across projects

**Key Files:**
- `Entity.cs` - Base entity with ID and domain events
- `Result.cs` - Result pattern implementation
- `Error.cs` - Error type and factory methods
- `ValueObjects/Money.cs` - Money value object
- `Dtos/AddressDto.cs` - Shared DTOs
- Abstractions: `IDomainEvent`, `IDateTimeProvider`, `IEntity`

**Dependencies:** None

#### 2. Domain (`/src/Domain/`)
**Purpose:** Core business logic and entities

**Structure by Aggregate:**
```
/Products/
  ├── Product.cs              # Product entity with business rules
  └── ProductErrors.cs        # Typed errors for product operations

/Categories/
  ├── Category.cs
  └── CategoryErrors.cs

/Users/
  ├── User.cs
  ├── UserErrors.cs
  ├── UserRegisteredDomainEvent.cs
  └── EmailVerificationToken.cs

/Orders/
  ├── Order.cs
  ├── OrderItem.cs
  ├── OrderStatus.cs
  └── OrderErrors.cs

/Carts/
  ├── Cart.cs
  └── CartItem.cs

/Reviews/
  └── Review.cs

/Addresses/
  └── Address.cs

/Inventories/
  └── Inventory.cs

/ProductCategories/
  └── ProductCategory.cs      # Many-to-many join entity
```

**Dependencies:** SharedKernel only

**Key Pattern:** Rich domain models with factory methods and business rules
```csharp
public class Product : Entity
{
    private Product() { } // Private constructor for EF

    public static Product Create(...) // Factory method
    {
        // Business rule validation
        return new Product(...);
    }

    public Result AddToCategory(Guid categoryId) // Business logic
    {
        // Enforce business rules
    }
}
```

#### 3. Application (`/src/Application/`)
**Purpose:** Application business logic (use cases)

**Structure:**
```
/Abstractions/
  /Messaging/
    ├── ICommand.cs                  # Command marker interface
    ├── ICommandHandler.cs           # Command handler interface
    ├── IQuery.cs                    # Query marker interface
    └── IQueryHandler.cs             # Query handler interface
  /Behaviors/
    ├── ValidationDecorator.cs       # Auto-validates commands/queries
    └── LoggingDecorator.cs          # Logs handler execution
  /Authentication/
    ├── IUserContext.cs              # Current user abstraction
    ├── IPasswordHasher.cs           # Password hashing abstraction
    └── ITokenProvider.cs            # JWT token generation
  /Emails/
    └── IEmailService.cs             # Email sending abstraction
  /Data/
    └── IApplicationDbContext.cs     # Database abstraction

/Users/
  /Register/
    ├── RegisterUserCommand.cs       # Command
    ├── RegisterUserCommandHandler.cs
    └── RegisterUserCommandValidator.cs
  /Login/
    ├── LoginCommand.cs
    └── LoginCommandHandler.cs
  /GetById/
    ├── GetUserByIdQuery.cs
    └── GetUserByIdQueryHandler.cs
  /GetByEmail/
  /VerifyEmail/
  /DomainEvents/
    └── UserRegisteredDomainEventHandler.cs

/Products/
  /Add/
    ├── AddProductCommand.cs
    ├── AddProductCommandHandler.cs
    └── AddProductCommandValidator.cs

/Categories/
  /Add/
```

**Dependencies:** Domain, SharedKernel, FluentValidation, EF Core abstractions

**Service Registration:** Auto-scanned via Scrutor in `DependencyInjection.cs`

#### 4. Infrastructure (`/src/Infrastructure/`)
**Purpose:** Technical implementations of Application abstractions

**Structure:**
```
/Database/
  ├── ApplicationDbContext.cs       # EF Core DbContext
  └── Schemas.cs                    # Database schema constants

/Authentication/
  ├── JwtTokenProvider.cs           # JWT implementation
  ├── PasswordHasher.cs             # BCrypt hashing
  └── UserContext.cs                # HTTP context user access

/Authorization/
  ├── CustomPermissionAuthorizationPolicyProvider.cs
  └── PermissionAuthorizationHandler.cs

/DomainEvents/
  └── DomainEventDispatcher.cs      # Publishes domain events

/Time/
  └── DateTimeProvider.cs           # System time abstraction

/[EntityName]/                      # EF Core configurations
  └── ProductConfiguration.cs       # Fluent API configuration
  └── UserConfiguration.cs
  └── ... (one per entity)

DependencyInjection.cs              # Infrastructure services
```

**Dependencies:** Application, Domain, SharedKernel, EF Core, JWT, etc.

**Key Files:**
- `ApplicationDbContext.cs` - DbSets, Identity integration, domain events
- `*Configuration.cs` - EF Core Fluent API for each entity

#### 5. Web.Api (`/src/Web.Api/`)
**Purpose:** HTTP API using Minimal APIs

**Structure:**
```
/Endpoints/
  ├── IEndpoint.cs                  # Endpoint interface
  ├── Tags.cs                       # OpenAPI tags
  /Users/
    ├── Register.cs                 # POST /users/register
    ├── Login.cs                    # POST /users/login
    ├── GetById.cs                  # GET /users/{id}
    ├── VerifyEmail.cs              # POST /users/verify-email
    └── Permissions.cs              # GET /users/permissions
  /Products/
    └── Add.cs                      # POST /products
  /Categories/

/Extensions/
  ├── EndpointExtensions.cs         # Endpoint registration
  ├── ResultExtensions.cs           # Result pattern helpers
  ├── MigrationExtensions.cs        # Auto-migration
  ├── ServiceCollectionExtensions.cs # Swagger config
  └── MiddlewareExtensions.cs

/Infrastructure/
  ├── GlobalExceptionHandler.cs     # Global exception handling
  └── CustomResults.cs              # ProblemDetails from Result

/Middleware/
  └── RequestContextLoggingMiddleware.cs

Program.cs                          # Application entry point
DependencyInjection.cs              # Presentation services
Dockerfile                          # Multi-stage Docker build
appsettings.json                    # Configuration
appsettings.Development.json        # Dev configuration
```

**Dependencies:** Infrastructure, Aspire.ServiceDefaults

**Endpoint Pattern:**
```csharp
internal sealed class Register : IEndpoint
{
    public sealed record Request(string Email, string Password, ...);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("users/register", async (
            Request request,
            ICommandHandler<RegisterUserCommand, Guid> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new RegisterUserCommand(...);
            Result<Guid> result = await handler.Handle(command, cancellationToken);
            return result.Match(Results.Created, CustomResults.Problem);
        })
        .WithTags(Tags.Users);
    }
}
```

#### 6. Aspire.AppHost (`/src/Aspire.AppHost/`)
**Purpose:** .NET Aspire orchestration for local development

**Orchestrates:**
- PostgreSQL 17 (port 5002, data in `.containers/db`)
- Papercut SMTP (email testing)
- Web.Api (with service references)

**Run:** `dotnet run --project src/Aspire.AppHost` to start all services

#### 7. Aspire.ServiceDefaults (`/src/Aspire.ServiceDefaults/`)
**Purpose:** Shared observability and health check configuration

**Features:**
- OpenTelemetry instrumentation
- Health checks (liveness, readiness)
- Service discovery
- Resilience patterns

---

## Development Workflows

### Local Development

#### Option 1: Using .NET Aspire (Recommended)
```bash
# Start all services (PostgreSQL, Papercut, Web API)
dotnet run --project src/Aspire.AppHost

# Access services:
# - Web API: https://localhost:7xxx (check console output)
# - Swagger: https://localhost:7xxx/swagger
# - Seq: http://localhost:8081
# - Aspire Dashboard: http://localhost:15xxx (check console output)
```

#### Option 2: Running Web API Directly
```bash
# Ensure PostgreSQL is running
docker run -d -p 5432:5432 \
  -e POSTGRES_PASSWORD=postgres \
  -e POSTGRES_DB=SharpBuy \
  postgres:17

# Run the API
dotnet run --project src/Web.Api

# Database migrations applied automatically in Development
```

### Database Migrations

#### Add Migration
```powershell
# Using script (recommended)
./scripts/add-migration.ps1 -MigrationName "AddProductReviews"

# Or manually
dotnet ef migrations add AddProductReviews \
  --project src/Infrastructure \
  --startup-project src/Web.Api \
  --output-dir Database/Migrations
```

#### Update Database
```powershell
# Using script
./scripts/update-database.ps1

# Or manually
dotnet ef database update \
  --project src/Infrastructure \
  --startup-project src/Web.Api
```

**Note:** In Development, migrations are applied automatically via `app.ApplyMigrations()` in `Program.cs`

### CI/CD Pipeline

**File:** `.github/workflows/build.yml`

**Triggers:**
- Push to `master` branch
- Manual dispatch

**Steps:**
1. Restore dependencies
2. Build in Release configuration
3. Run all tests (including architecture tests)
4. Publish artifacts

**Workflow:**
```yaml
jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - Checkout code
      - Setup .NET 9.x
      - Restore: dotnet restore
      - Build: dotnet build --configuration Release --no-restore
      - Test: dotnet test --configuration Release --no-restore --no-build
      - Publish: dotnet publish --configuration Release --no-restore --no-build
```

**IMPORTANT:** All tests must pass, including architecture tests. Layer violations will fail the build.

### Building & Running

```bash
# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Run tests
dotnet test

# Run architecture tests specifically
dotnet test tests/ArchitectureTests

# Run with hot reload
dotnet watch run --project src/Web.Api
```

---

## Coding Conventions & Patterns

### Naming Conventions

#### Commands
```csharp
// Pattern: {Action}{Entity}Command
public sealed record RegisterUserCommand(...) : ICommand<Guid>;
public sealed record AddProductCommand(...) : ICommand<Guid>;
public sealed record UpdateOrderCommand(...) : ICommand;
```

#### Queries
```csharp
// Pattern: Get{Entity}By{Property}Query
public sealed record GetUserByIdQuery(Guid Id) : IQuery<UserResponse>;
public sealed record GetProductsByCategoryQuery(Guid CategoryId) : IQuery<List<ProductDto>>;
```

#### Handlers
```csharp
// Pattern: {CommandOrQuery}Handler
internal sealed class RegisterUserCommandHandler : ICommandHandler<RegisterUserCommand, Guid>
{
    public async Task<Result<Guid>> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        // Implementation
    }
}
```

#### Errors
```csharp
// Pattern: Static class {Entity}Errors
public static class UserErrors
{
    public static Error NotFound(Guid userId) => Error.NotFound(
        "Users.NotFound",
        $"The user with Id '{userId}' was not found"
    );

    public static readonly Error EmailAlreadyInUse = Error.Conflict(
        "Users.EmailAlreadyInUse",
        "The specified email is already in use"
    );
}
```

#### Endpoints
```csharp
// Endpoint class named after action
internal sealed class Register : IEndpoint { }
internal sealed class Login : IEndpoint { }
internal sealed class Add : IEndpoint { }
internal sealed class GetById : IEndpoint { }
```

### Error Handling

#### Error Types
```csharp
public enum ErrorType
{
    Failure = 0,      // 500 Internal Server Error
    Validation = 1,   // 400 Bad Request
    NotFound = 2,     // 404 Not Found
    Conflict = 3,     // 409 Conflict
    Problem = 4       // 400 Bad Request
}
```

#### Creating Errors
```csharp
// Factory methods on Error class
Error.NotFound(code, description);
Error.Validation(code, description);
Error.Conflict(code, description);
Error.Problem(code, description);
Error.Failure(code, description);
```

#### Using Result Pattern
```csharp
// Success
Result<User> success = Result.Success(user);

// Failure
Result<User> failure = Result.Failure<User>(UserErrors.NotFound(id));

// With validation errors
Result<User> validationFailure = Result.Failure<User>(
    Error.Validation("Users.Validation", "Validation failed", validationErrors)
);

// Pattern matching
return result.Match(
    onSuccess: user => Results.Ok(user),
    onFailure: CustomResults.Problem
);
```

### CQRS Implementation

#### Command Pattern
```csharp
// 1. Define command (in Application layer)
public sealed record RegisterUserCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName
) : ICommand<Guid>; // Returns user ID

// 2. Create validator
internal sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
    }
}

// 3. Implement handler
internal sealed class RegisterUserCommandHandler(
    IApplicationDbContext dbContext,
    IPasswordHasher passwordHasher) : ICommandHandler<RegisterUserCommand, Guid>
{
    public async Task<Result<Guid>> Handle(
        RegisterUserCommand command,
        CancellationToken cancellationToken)
    {
        // Check business rules
        if (await dbContext.Users.AnyAsync(u => u.Email == command.Email))
        {
            return Result.Failure<Guid>(UserErrors.EmailAlreadyInUse);
        }

        // Create entity using factory method
        User user = User.Create(
            command.Email,
            passwordHasher.Hash(command.Password),
            command.FirstName,
            command.LastName
        );

        // Persist
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
}

// 4. Create endpoint (in Web.Api layer)
internal sealed class Register : IEndpoint
{
    public sealed record Request(
        string Email,
        string Password,
        string FirstName,
        string LastName
    );

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("users/register", async (
            Request request,
            ICommandHandler<RegisterUserCommand, Guid> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new RegisterUserCommand(
                request.Email,
                request.Password,
                request.FirstName,
                request.LastName
            );

            Result<Guid> result = await handler.Handle(command, cancellationToken);

            return result.Match(
                userId => Results.Created($"/users/{userId}", userId),
                CustomResults.Problem
            );
        })
        .WithTags(Tags.Users)
        .AllowAnonymous();
    }
}
```

#### Query Pattern
```csharp
// 1. Define query
public sealed record GetUserByIdQuery(Guid Id) : IQuery<UserResponse>;

// 2. Define response DTO
public sealed record UserResponse(
    Guid Id,
    string Email,
    string FirstName,
    string LastName
);

// 3. Implement handler
internal sealed class GetUserByIdQueryHandler(
    IApplicationDbContext dbContext) : IQueryHandler<GetUserByIdQuery, UserResponse>
{
    public async Task<Result<UserResponse>> Handle(
        GetUserByIdQuery query,
        CancellationToken cancellationToken)
    {
        UserResponse? user = await dbContext.Users
            .Where(u => u.Id == query.Id)
            .Select(u => new UserResponse(u.Id, u.Email, u.FirstName, u.LastName))
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            return Result.Failure<UserResponse>(UserErrors.NotFound(query.Id));
        }

        return user;
    }
}
```

### Domain Events

#### Publishing Domain Events
```csharp
// 1. Define domain event (in Domain layer)
public sealed class UserRegisteredDomainEvent(Guid userId) : IDomainEvent
{
    public Guid UserId { get; } = userId;
}

// 2. Raise event in entity
public class User : Entity
{
    public static User Create(...)
    {
        var user = new User(...);
        user.RaiseDomainEvent(new UserRegisteredDomainEvent(user.Id));
        return user;
    }
}

// 3. Handle event (in Application layer)
internal sealed class UserRegisteredDomainEventHandler(
    IEmailService emailService) : IDomainEventHandler<UserRegisteredDomainEvent>
{
    public async Task Handle(
        UserRegisteredDomainEvent domainEvent,
        CancellationToken cancellationToken)
    {
        // Send welcome email
        await emailService.SendWelcomeEmailAsync(...);
    }
}
```

**Note:** Domain events are published in `ApplicationDbContext.SaveChangesAsync()` BEFORE saving changes (transactional consistency).

### Dependency Injection Patterns

#### Auto-Registration with Scrutor
```csharp
// Application/DependencyInjection.cs
services.Scan(scan => scan
    .FromAssemblies(Application.AssemblyReference.Assembly)
    .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<,>)))
    .AsImplementedInterfaces()
    .WithScopedLifetime());

// Decorator registration
services.Decorate(typeof(ICommandHandler<,>), typeof(ValidationDecorator<,>));
services.Decorate(typeof(ICommandHandler<,>), typeof(LoggingDecorator<,>));
```

#### Manual Registration
```csharp
// Infrastructure/DependencyInjection.cs
services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
services.AddScoped<IUserContext, UserContext>();
services.AddScoped<IPasswordHasher, PasswordHasher>();
services.AddScoped<ITokenProvider, JwtTokenProvider>();
```

### Value Objects

#### Money Example
```csharp
// SharedKernel/ValueObjects/Money.cs
public sealed record Money(decimal Amount, string Currency)
{
    public static Money operator +(Money first, Money second)
    {
        if (first.Currency != second.Currency)
        {
            throw new InvalidOperationException("Currencies must be the same");
        }
        return new Money(first.Amount + second.Amount, first.Currency);
    }

    public static Money Zero(string currency) => new(0, currency);
}

// EF Core configuration
builder.OwnsOne(p => p.Price, priceBuilder =>
{
    priceBuilder.Property(m => m.Amount).HasColumnName("price_amount");
    priceBuilder.Property(m => m.Currency).HasColumnName("price_currency");
});
```

### Entity Patterns

#### Base Entity
```csharp
// SharedKernel/Entity.cs
public abstract class Entity : IEntity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    protected Entity() { }

    public Guid Id { get; init; }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
```

#### Rich Domain Model
```csharp
public class Product : Entity
{
    private readonly List<ProductCategory> _productCategories = [];

    private Product() { } // For EF Core

    public string Name { get; private set; }
    public Money Price { get; private set; }
    public Inventory Inventory { get; private set; }

    public IReadOnlyCollection<ProductCategory> ProductCategories => _productCategories.AsReadOnly();

    public static Product Create(string name, Money price, int stockQuantity)
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = name,
            Price = price,
            Inventory = Inventory.Create(stockQuantity)
        };

        return product;
    }

    public Result AddToCategory(Guid categoryId)
    {
        if (_productCategories.Any(pc => pc.CategoryId == categoryId))
        {
            return Result.Failure(ProductErrors.AlreadyInCategory);
        }

        _productCategories.Add(new ProductCategory
        {
            ProductId = Id,
            CategoryId = categoryId
        });

        return Result.Success();
    }

    public Result UpdatePrice(Money newPrice)
    {
        if (newPrice.Amount < 0)
        {
            return Result.Failure(ProductErrors.InvalidPrice);
        }

        Price = newPrice;
        return Result.Success();
    }
}
```

---

## Database & Migrations

### Database Configuration

**Database:** PostgreSQL 17
**ORM:** Entity Framework Core 9.0.10
**Provider:** Npgsql 9.0.4
**Naming:** Snake case (via EFCore.NamingConventions)

### Connection Strings

**Development:**
```json
"ConnectionStrings": {
  "Database": "Host=postgres;Port=5432;Database=SharpBuy;Username=postgres;Password=postgres"
}
```

**Production:** Set via environment variable or configuration provider

### DbContext

**File:** `src/Infrastructure/Database/ApplicationDbContext.cs`

```csharp
public sealed class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    IDomainEventDispatcher domainEventDispatcher)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options), IApplicationDbContext
{
    // Domain DbSets
    public DbSet<User> Users { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<Cart> Carts { get; set; }
    // ... other DbSets

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations
        modelBuilder.ApplyConfigurationsFromAssembly(Infrastructure.AssemblyReference.Assembly);

        // Configure schema
        modelBuilder.HasDefaultSchema(Schemas.Default);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Publish domain events BEFORE saving (transactional consistency)
        await domainEventDispatcher.DispatchAsync(this, cancellationToken);

        return await base.SaveChangesAsync(cancellationToken);
    }
}
```

### Entity Configuration Pattern

**File:** `src/Infrastructure/Products/ProductConfiguration.cs`

```csharp
internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);

        // Properties
        builder.Property(p => p.Name).HasMaxLength(200).IsRequired();

        // Value object (owned entity)
        builder.OwnsOne(p => p.Price, priceBuilder =>
        {
            priceBuilder.Property(m => m.Amount).HasColumnName("price_amount");
            priceBuilder.Property(m => m.Currency).HasColumnName("price_currency");
        });

        // JSON column
        builder.Property(p => p.PhotoPaths)
            .HasConversion(
                v => JsonConvert.SerializeObject(v),
                v => JsonConvert.DeserializeObject<List<string>>(v)!
            )
            .HasColumnType("jsonb");

        // Relationships
        builder.HasOne(p => p.Inventory)
            .WithOne()
            .HasForeignKey<Product>("inventory_id");

        builder.HasMany(p => p.ProductCategories)
            .WithOne(pc => pc.Product)
            .HasForeignKey(pc => pc.ProductId);

        // Indexes
        builder.HasIndex(p => p.Name);
        builder.HasIndex("inventory_id").IsUnique();

        // Ignore domain events (not persisted)
        builder.Ignore(p => p.DomainEvents);

        // Private field access
        builder.Metadata
            .FindNavigation(nameof(Product.ProductCategories))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
```

### Migration Commands

#### Add Migration
```bash
# Using PowerShell script
./scripts/add-migration.ps1 -MigrationName "AddProductReviews"

# Using dotnet CLI
dotnet ef migrations add AddProductReviews \
  --project src/Infrastructure \
  --startup-project src/Web.Api \
  --output-dir Database/Migrations
```

#### Update Database
```bash
# Using PowerShell script
./scripts/update-database.ps1

# Using dotnet CLI
dotnet ef database update \
  --project src/Infrastructure \
  --startup-project src/Web.Api
```

#### Remove Last Migration
```bash
dotnet ef migrations remove \
  --project src/Infrastructure \
  --startup-project src/Web.Api
```

#### Generate SQL Script
```bash
dotnet ef migrations script \
  --project src/Infrastructure \
  --startup-project src/Web.Api \
  --output migrations.sql
```

### Auto-Migration in Development

**File:** `src/Web.Api/Program.cs`

```csharp
if (app.Environment.IsDevelopment())
{
    app.ApplyMigrations(); // Automatically applies pending migrations
    app.UseSwaggerWithUi();
}
```

**IMPORTANT:** Only use auto-migration in development. In production, apply migrations via deployment pipeline.

---

## API Design Guidelines

### Endpoint Structure

All endpoints implement the `IEndpoint` interface:

```csharp
public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}
```

### Endpoint Organization

**Location:** `/src/Web.Api/Endpoints/{Feature}/{Action}.cs`

**Pattern:** Vertical Slice Architecture (feature-based)

```
/Endpoints/
  /Users/
    ├── Register.cs       # POST /users/register
    ├── Login.cs          # POST /users/login
    ├── GetById.cs        # GET /users/{id}
    ├── VerifyEmail.cs    # POST /users/verify-email
    └── Permissions.cs    # GET /users/permissions
  /Products/
    ├── Add.cs            # POST /products
    ├── GetById.cs        # GET /products/{id}
    └── List.cs           # GET /products
  /Categories/
    └── Add.cs            # POST /categories
```

### Endpoint Template

```csharp
namespace Web.Api.Endpoints.{Feature};

internal sealed class {Action} : IEndpoint
{
    // Request DTO (if needed)
    public sealed record Request(/* properties */);

    // Response DTO (if needed)
    public sealed record Response(/* properties */);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.Map{HttpMethod}("{route}", async (
            /* parameters: Request, handlers, services, CancellationToken */
            Request request,
            ICommandHandler<SomeCommand, SomeResult> handler,
            CancellationToken cancellationToken) =>
        {
            // 1. Map request to command/query
            var command = new SomeCommand(request.Property1, request.Property2);

            // 2. Execute handler
            Result<SomeResult> result = await handler.Handle(command, cancellationToken);

            // 3. Map result to HTTP response
            return result.Match(
                data => Results.Ok(data),
                CustomResults.Problem
            );
        })
        .WithTags(Tags.{Feature})
        .{AuthorizationPolicy}(); // .RequireAuthorization(), .AllowAnonymous(), .HasPermission("permission")
    }
}
```

### HTTP Verb Usage

- **POST** - Create resources, non-idempotent operations
- **GET** - Retrieve resources (queries)
- **PUT** - Update entire resource
- **PATCH** - Update partial resource
- **DELETE** - Delete resource

### Response Patterns

#### Success Responses
```csharp
// 200 OK - Successful query
return Results.Ok(data);

// 201 Created - Resource created
return Results.Created($"/users/{userId}", userId);

// 204 No Content - Successful command with no response body
return Results.NoContent();
```

#### Error Responses (via Result Pattern)
```csharp
// Automatic mapping based on ErrorType
return result.Match(
    onSuccess: data => Results.Ok(data),
    onFailure: CustomResults.Problem // Maps to appropriate status code
);

// Error type mappings:
// - Validation -> 400 Bad Request
// - NotFound -> 404 Not Found
// - Conflict -> 409 Conflict
// - Problem -> 400 Bad Request
// - Failure -> 500 Internal Server Error
```

### Authentication & Authorization

#### Anonymous Endpoints
```csharp
.AllowAnonymous()
```

#### Require Authentication
```csharp
.RequireAuthorization()
```

#### Permission-Based Authorization
```csharp
.HasPermission("users.read")
.HasPermission("products.write")
```

### OpenAPI Tags

Group endpoints by feature:

```csharp
.WithTags(Tags.Users)
.WithTags(Tags.Products)
.WithTags(Tags.Categories)
```

### Request Validation

**Automatic via Decorator:** FluentValidation validators are automatically executed

```csharp
// Validator is auto-discovered and executed
internal sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
    }
}

// Endpoint doesn't need validation logic
var command = new RegisterUserCommand(...);
Result<Guid> result = await handler.Handle(command, ct);
// Validation errors automatically returned as 400 Bad Request
```

### Swagger Configuration

**Location:** `src/Web.Api/Extensions/ServiceCollectionExtensions.cs`

JWT authentication configured in Swagger:
```csharp
options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
{
    In = ParameterLocation.Header,
    Description = "Please enter token",
    Name = "Authorization",
    Type = SecuritySchemeType.Http,
    BearerFormat = "JWT",
    Scheme = "bearer"
});
```

---

## Testing Strategy

### Architecture Tests

**Purpose:** Enforce Clean Architecture layer dependencies

**Location:** `/tests/ArchitectureTests/`

**Framework:** xUnit + NetArchTest.Rules + Shouldly

**Tests:** (`LayerTests.cs`)

```csharp
[Fact]
public void Domain_Should_NotDependOn_Application()
{
    TestResult result = Types.InAssembly(DomainAssembly)
        .Should()
        .NotHaveDependencyOn(ApplicationAssembly.GetName().Name)
        .GetResult();

    result.IsSuccessful.ShouldBeTrue();
}
```

**All Tests:**
1. Domain should not depend on Application
2. Domain should not depend on Infrastructure
3. Domain should not depend on Presentation
4. Application should not depend on Infrastructure
5. Application should not depend on Presentation
6. Infrastructure should not depend on Presentation

**Run Tests:**
```bash
dotnet test tests/ArchitectureTests
```

**CRITICAL:** These tests run in CI/CD. Layer violations will fail the build.

### Testing Conventions

#### Test Project Structure
```
/tests/
  /ArchitectureTests/         # Current
  /Application.UnitTests/     # Planned (InternalsVisibleTo configured)
  /Infrastructure.IntegrationTests/  # Planned
  /Web.Api.IntegrationTests/  # Planned (Program class exposed)
```

#### Exposing Internals for Testing

**Application Layer:**
```csharp
// Application.csproj
<InternalsVisibleTo Include="Application.UnitTests" />
```

**Web.Api Layer:**
```csharp
// Program.cs
public partial class Program; // Exposes for WebApplicationFactory
```

### Future Testing Recommendations

#### Unit Tests
- Test command/query handlers in isolation
- Mock dependencies (IApplicationDbContext, etc.)
- Test validators
- Test domain entity business rules

#### Integration Tests
- Test database operations with real DbContext
- Test full CQRS flow (command -> handler -> database)
- Use in-memory PostgreSQL or test containers

#### API Tests
- Use WebApplicationFactory
- Test full HTTP request/response cycle
- Test authentication/authorization
- Test error handling

---

## Common Tasks & Examples

### Adding a New Feature (Complete Example)

**Task:** Add "Update Product Price" feature

#### Step 1: Add Domain Logic

**File:** `src/Domain/Products/Product.cs`
```csharp
public Result UpdatePrice(Money newPrice)
{
    if (newPrice.Amount <= 0)
    {
        return Result.Failure(ProductErrors.InvalidPrice);
    }

    if (newPrice.Currency != Price.Currency)
    {
        return Result.Failure(ProductErrors.CurrencyMismatch);
    }

    Price = newPrice;

    RaiseDomainEvent(new ProductPriceChangedDomainEvent(Id, Price, newPrice));

    return Result.Success();
}
```

**File:** `src/Domain/Products/ProductErrors.cs`
```csharp
public static readonly Error InvalidPrice = Error.Validation(
    "Products.InvalidPrice",
    "Product price must be greater than zero"
);

public static readonly Error CurrencyMismatch = Error.Validation(
    "Products.CurrencyMismatch",
    "Product currency cannot be changed"
);
```

#### Step 2: Create Command & Handler

**File:** `src/Application/Products/UpdatePrice/UpdateProductPriceCommand.cs`
```csharp
namespace Application.Products.UpdatePrice;

public sealed record UpdateProductPriceCommand(
    Guid ProductId,
    decimal Amount,
    string Currency
) : ICommand;
```

**File:** `src/Application/Products/UpdatePrice/UpdateProductPriceCommandValidator.cs`
```csharp
namespace Application.Products.UpdatePrice;

internal sealed class UpdateProductPriceCommandValidator : AbstractValidator<UpdateProductPriceCommand>
{
    public UpdateProductPriceCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Currency).NotEmpty().Length(3);
    }
}
```

**File:** `src/Application/Products/UpdatePrice/UpdateProductPriceCommandHandler.cs`
```csharp
namespace Application.Products.UpdatePrice;

internal sealed class UpdateProductPriceCommandHandler(
    IApplicationDbContext dbContext) : ICommandHandler<UpdateProductPriceCommand>
{
    public async Task<Result> Handle(
        UpdateProductPriceCommand command,
        CancellationToken cancellationToken)
    {
        Product? product = await dbContext.Products
            .FirstOrDefaultAsync(p => p.Id == command.ProductId, cancellationToken);

        if (product is null)
        {
            return Result.Failure(ProductErrors.NotFound(command.ProductId));
        }

        var newPrice = new Money(command.Amount, command.Currency);

        Result result = product.UpdatePrice(newPrice);

        if (result.IsFailure)
        {
            return result;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
```

#### Step 3: Create Endpoint

**File:** `src/Web.Api/Endpoints/Products/UpdatePrice.cs`
```csharp
namespace Web.Api.Endpoints.Products;

internal sealed class UpdatePrice : IEndpoint
{
    public sealed record Request(decimal Amount, string Currency);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("products/{id:guid}/price", async (
            Guid id,
            Request request,
            ICommandHandler<UpdateProductPriceCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateProductPriceCommand(
                id,
                request.Amount,
                request.Currency
            );

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(
                () => Results.NoContent(),
                CustomResults.Problem
            );
        })
        .WithTags(Tags.Products)
        .HasPermission("products.update");
    }
}
```

#### Step 4: Test
```bash
# Run architecture tests (ensure no violations)
dotnet test tests/ArchitectureTests

# Run the API
dotnet run --project src/Aspire.AppHost

# Test endpoint
curl -X PUT https://localhost:7xxx/products/{id}/price \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {token}" \
  -d '{"amount": 29.99, "currency": "USD"}'
```

### Adding a New Entity

#### Step 1: Create Domain Entity

**File:** `src/Domain/Suppliers/Supplier.cs`
```csharp
namespace Domain.Suppliers;

public class Supplier : Entity
{
    private Supplier() { } // For EF Core

    public string Name { get; private set; }
    public string Email { get; private set; }
    public string Phone { get; private set; }

    public static Supplier Create(string name, string email, string phone)
    {
        return new Supplier
        {
            Id = Guid.NewGuid(),
            Name = name,
            Email = email,
            Phone = phone
        };
    }
}
```

**File:** `src/Domain/Suppliers/SupplierErrors.cs`
```csharp
namespace Domain.Suppliers;

public static class SupplierErrors
{
    public static Error NotFound(Guid supplierId) => Error.NotFound(
        "Suppliers.NotFound",
        $"The supplier with Id '{supplierId}' was not found"
    );
}
```

#### Step 2: Add DbSet

**File:** `src/Application/Abstractions/Data/IApplicationDbContext.cs`
```csharp
public DbSet<Supplier> Suppliers { get; set; }
```

**File:** `src/Infrastructure/Database/ApplicationDbContext.cs`
```csharp
public DbSet<Supplier> Suppliers { get; set; }
```

#### Step 3: Create EF Configuration

**File:** `src/Infrastructure/Suppliers/SupplierConfiguration.cs`
```csharp
namespace Infrastructure.Suppliers;

internal sealed class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name).HasMaxLength(200).IsRequired();
        builder.Property(s => s.Email).HasMaxLength(200).IsRequired();
        builder.Property(s => s.Phone).HasMaxLength(20);

        builder.HasIndex(s => s.Email).IsUnique();

        builder.Ignore(s => s.DomainEvents);
    }
}
```

#### Step 4: Create Migration
```bash
./scripts/add-migration.ps1 -MigrationName "AddSuppliers"
```

#### Step 5: Implement Use Cases
Follow the pattern from "Adding a New Feature" above.

### Adding Authentication to an Endpoint

```csharp
public void MapEndpoint(IEndpointRouteBuilder app)
{
    app.MapGet("products/{id:guid}", async (...) => { ... })
        .WithTags(Tags.Products)
        .RequireAuthorization(); // Requires valid JWT token
}
```

### Adding Permission-Based Authorization

```csharp
public void MapEndpoint(IEndpointRouteBuilder app)
{
    app.MapPost("products", async (...) => { ... })
        .WithTags(Tags.Products)
        .HasPermission("products.create"); // Requires specific permission
}
```

**Note:** Current implementation allows all authenticated users. Implement permission checks in `PermissionAuthorizationHandler`.

### Adding a Domain Event Handler

**File:** `src/Application/Products/DomainEvents/ProductPriceChangedDomainEventHandler.cs`
```csharp
namespace Application.Products.DomainEvents;

internal sealed class ProductPriceChangedDomainEventHandler(
    IEmailService emailService,
    IApplicationDbContext dbContext) : IDomainEventHandler<ProductPriceChangedDomainEvent>
{
    public async Task Handle(
        ProductPriceChangedDomainEvent domainEvent,
        CancellationToken cancellationToken)
    {
        // Send notification to admin
        await emailService.SendAsync(
            to: "admin@sharpbuy.com",
            subject: "Product Price Changed",
            body: $"Product {domainEvent.ProductId} price changed to {domainEvent.NewPrice.Amount} {domainEvent.NewPrice.Currency}"
        );
    }
}
```

**Auto-registered** via Scrutor - no manual registration needed.

### Querying Data with Projections

```csharp
internal sealed class GetProductsQueryHandler(
    IApplicationDbContext dbContext) : IQueryHandler<GetProductsQuery, List<ProductDto>>
{
    public async Task<Result<List<ProductDto>>> Handle(
        GetProductsQuery query,
        CancellationToken cancellationToken)
    {
        List<ProductDto> products = await dbContext.Products
            .AsNoTracking() // Read-only query
            .Where(p => p.Price.Amount >= query.MinPrice)
            .Select(p => new ProductDto(
                p.Id,
                p.Name,
                p.Price.Amount,
                p.Price.Currency,
                p.Inventory.StockQuantity
            ))
            .ToListAsync(cancellationToken);

        return products;
    }
}
```

**Best Practices:**
- Use `.AsNoTracking()` for read-only queries
- Project to DTOs in the database query (not in memory)
- Return only necessary fields

---

## Important Files & Configurations

### Configuration Files

#### Root Level
- **`Directory.Build.props`** - Global MSBuild properties
  - Target framework: net9.0
  - Nullable enabled
  - Warnings as errors
  - SonarAnalyzer reference

- **`Directory.Packages.props`** - Central Package Management
  - All package versions defined here
  - Referenced without version in project files

- **`.editorconfig`** - Code style rules
  - 4-space indentation
  - CRLF line endings
  - Strict C# conventions (all set to "error")

- **`.gitignore`** - Git ignore patterns
  - bin/, obj/, .vs/, .idea/
  - *.user, *.suo
  - appsettings.Development.json (if contains secrets)

#### Application Configuration

**`src/Web.Api/appsettings.json`** - Base configuration
```json
{
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "Database": ""
  },
  "Jwt": {
    "Secret": "",
    "Issuer": "SharpBuyAPI",
    "Audience": "SharpBuyClient",
    "ExpirationInMinutes": 5
  },
  "EmailOptions": {
    "SmtpServer": "",
    "SmtpPort": 587,
    "From": "noreply@sharpbuy.com"
  }
}
```

**`src/Web.Api/appsettings.Development.json`** - Development overrides
```json
{
  "ConnectionStrings": {
    "Database": "Host=postgres;Port=5432;Database=SharpBuy;Username=postgres;Password=postgres"
  },
  "Serilog": {
    "WriteTo": [
      { "Name": "Console" },
      {
        "Name": "Seq",
        "Args": { "serverUrl": "http://localhost:8081" }
      }
    ]
  },
  "Jwt": {
    "Secret": "super-duper-secret-value-that-is-at-least-256-bits-long",
    "ExpirationInMinutes": 60
  },
  "EmailOptions": {
    "SmtpServer": "SharpBuy.Papercut",
    "SmtpPort": 25
  }
}
```

#### CI/CD

**`.github/workflows/build.yml`** - Build pipeline
- Triggers: Push to master, manual dispatch
- Steps: Restore, Build, Test, Publish
- Environment: Ubuntu latest, .NET 9.x

#### Scripts

**`scripts/add-migration.ps1`** - Add EF migration
```powershell
param([string]$MigrationName)
dotnet ef migrations add $MigrationName `
  --project src/Infrastructure `
  --startup-project src/Web.Api `
  --output-dir Database/Migrations
```

**`scripts/update-database.ps1`** - Update database
```powershell
dotnet ef database update `
  --project src/Infrastructure `
  --startup-project src/Web.Api
```

### Key Application Files

#### Dependency Injection
- `src/Application/DependencyInjection.cs` - Application services (Scrutor)
- `src/Infrastructure/DependencyInjection.cs` - Infrastructure services
- `src/Web.Api/DependencyInjection.cs` - Presentation services

#### Entry Points
- `src/Web.Api/Program.cs` - Application entry point
- `src/Aspire.AppHost/Program.cs` - Aspire orchestration

#### Core Abstractions
- `src/Application/Abstractions/Messaging/` - CQRS interfaces
- `src/Application/Abstractions/Data/IApplicationDbContext.cs` - Database abstraction
- `src/Infrastructure/Database/ApplicationDbContext.cs` - EF Core implementation

#### Cross-Cutting Concerns
- `src/Application/Abstractions/Behaviors/ValidationDecorator.cs`
- `src/Application/Abstractions/Behaviors/LoggingDecorator.cs`
- `src/Web.Api/Infrastructure/GlobalExceptionHandler.cs`

---

## Troubleshooting

### Common Issues

#### Issue: Architecture Tests Failing
**Symptom:** Build fails with "Types in {Layer} should not depend on {OtherLayer}"

**Solution:**
1. Check the dependency in the failing type
2. Move abstraction to lower layer if needed
3. Ensure you're following Clean Architecture dependency rules
4. Common violation: Infrastructure types referenced in Application

#### Issue: Migration Not Applied
**Symptom:** Database queries fail with "table does not exist"

**Solution:**
```bash
# Check pending migrations
dotnet ef migrations list --project src/Infrastructure --startup-project src/Web.Api

# Apply migrations
./scripts/update-database.ps1

# Or ensure auto-migration is enabled in Development
# Check Program.cs for: app.ApplyMigrations();
```

#### Issue: JWT Authentication Failing
**Symptom:** 401 Unauthorized on authenticated endpoints

**Checklist:**
1. Ensure JWT secret is configured in appsettings
2. Token must be sent in header: `Authorization: Bearer {token}`
3. Token must not be expired (check ExpirationInMinutes)
4. Issuer and Audience must match configuration
5. Use `/users/login` to get a valid token

#### Issue: Validation Not Working
**Symptom:** Invalid data accepted by endpoint

**Checklist:**
1. Ensure validator class exists: `{Command}Validator : AbstractValidator<{Command}>`
2. Validator must be in same assembly as command (Application layer)
3. Validator auto-discovered by Scrutor (check DependencyInjection.cs)
4. ValidationDecorator registered: `services.Decorate(typeof(ICommandHandler<,>), typeof(ValidationDecorator<,>))`

#### Issue: Domain Events Not Firing
**Symptom:** Domain event handler not executed

**Checklist:**
1. Ensure event raised: `entity.RaiseDomainEvent(new SomeEvent(...))`
2. Handler implements `IDomainEventHandler<TEvent>`
3. Handler auto-registered by Scrutor
4. `SaveChangesAsync` called (events published before save)
5. Check DomainEventDispatcher registration in Infrastructure

#### Issue: Endpoint Not Found (404)
**Symptom:** Endpoint returns 404 Not Found

**Checklist:**
1. Endpoint class implements `IEndpoint`
2. Endpoint registered: `app.MapEndpoints()` in Program.cs
3. Route matches request URL
4. HTTP verb matches (GET, POST, PUT, DELETE)
5. Check Swagger UI for registered endpoints

#### Issue: EF Core Configuration Not Applied
**Symptom:** Database schema doesn't match expectations

**Checklist:**
1. Configuration class implements `IEntityTypeConfiguration<TEntity>`
2. Configuration in Infrastructure assembly (auto-discovered)
3. Check `ApplicationDbContext.OnModelCreating`:
   ```csharp
   modelBuilder.ApplyConfigurationsFromAssembly(Infrastructure.AssemblyReference.Assembly);
   ```
4. Regenerate migration after configuration changes

#### Issue: Circular Dependency
**Symptom:** Dependency injection fails at startup

**Solution:**
1. Review constructor dependencies
2. Consider using factory pattern or lazy initialization
3. Check for bidirectional navigation properties
4. Use `IServiceScopeFactory` if needed

#### Issue: Aspire Not Starting
**Symptom:** Aspire.AppHost fails to start services

**Checklist:**
1. Docker must be running
2. Check ports not already in use (5432 for PostgreSQL)
3. Check `.containers/db` permissions
4. Review Aspire dashboard for error details

### Debugging Tips

#### Enable Detailed Logging
```json
// appsettings.Development.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.EntityFrameworkCore": "Information",
      "Microsoft.AspNetCore": "Information"
    }
  }
}
```

#### View SQL Queries
```json
"Logging": {
  "LogLevel": {
    "Microsoft.EntityFrameworkCore.Database.Command": "Information"
  }
}
```

#### Check Seq Logs
Navigate to http://localhost:8081 to view structured logs

#### Inspect Database
```bash
# Connect to PostgreSQL
docker exec -it {container_id} psql -U postgres -d SharpBuy

# List tables
\dt

# Describe table
\d products

# Query data
SELECT * FROM users;
```

### Performance Optimization

#### Use Compiled Queries
```csharp
private static readonly Func<ApplicationDbContext, Guid, Task<User?>> GetUserByIdCompiled =
    EF.CompileAsyncQuery((ApplicationDbContext context, Guid id) =>
        context.Users.FirstOrDefault(u => u.Id == id));

// Usage
User? user = await GetUserByIdCompiled(dbContext, userId);
```

#### Use AsNoTracking for Read-Only Queries
```csharp
List<ProductDto> products = await dbContext.Products
    .AsNoTracking()
    .Select(p => new ProductDto(...))
    .ToListAsync();
```

#### Batch Operations
```csharp
// Instead of multiple SaveChanges
dbContext.Products.AddRange(products);
await dbContext.SaveChangesAsync(); // Single batch
```

#### Index Frequently Queried Columns
```csharp
// In entity configuration
builder.HasIndex(p => p.Email);
builder.HasIndex(p => new { p.CategoryId, p.CreatedAt });
```

---

## Best Practices for AI Assistants

### When Adding New Features

1. **Follow the Layer Pattern:**
   - Start with Domain (entities, errors)
   - Then Application (commands, handlers, validators)
   - Finally Presentation (endpoints)

2. **Use Existing Patterns:**
   - Copy similar features as templates
   - Follow naming conventions strictly
   - Use Result pattern for all operations

3. **Maintain Architecture:**
   - Run architecture tests frequently
   - Never violate dependency rules
   - Keep abstractions in Application layer

4. **Validate Everything:**
   - Create FluentValidation validator for each command/query
   - Test with invalid data
   - Return appropriate errors

5. **Write Clean Code:**
   - Follow .editorconfig rules
   - Use meaningful names
   - Keep methods small and focused

### When Debugging Issues

1. **Check Logs First:**
   - Console output
   - Seq (http://localhost:8081)
   - Look for exceptions and warnings

2. **Verify Configuration:**
   - Connection strings
   - JWT settings
   - Email settings

3. **Test Incrementally:**
   - Test each layer independently
   - Use architecture tests
   - Verify database state

4. **Common Gotchas:**
   - Forgot to apply migrations
   - Incorrect dependency direction
   - Missing validator
   - Endpoint not registered

### When Reviewing Code

1. **Architecture Compliance:**
   - Dependencies flow inward
   - No Infrastructure in Application
   - No Application in Domain

2. **Pattern Adherence:**
   - Result pattern used
   - CQRS separation maintained
   - Domain logic in entities

3. **Code Quality:**
   - No warnings (treat warnings as errors)
   - Follows .editorconfig
   - SonarAnalyzer satisfied

4. **Security:**
   - Authentication/authorization applied
   - Input validation present
   - SQL injection prevented (EF Core parameterized queries)
   - XSS prevented (no HTML rendering)

---

## Additional Resources

### Documentation
- **.NET Aspire:** https://learn.microsoft.com/en-us/dotnet/aspire/
- **EF Core:** https://learn.microsoft.com/en-us/ef/core/
- **Minimal APIs:** https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis
- **FluentValidation:** https://docs.fluentvalidation.net/

### Project README
See `README.md` for quick overview and what's included in the template.

### Seq Log Viewer
Access structured logs at: http://localhost:8081

### Swagger UI
Access API documentation at: https://localhost:{port}/swagger (Development only)

---

**Last Updated:** 2025-11-16
**Maintained by:** SharpBuy Development Team
**Questions?** Review this document first, then check Seq logs, then ask for help.
