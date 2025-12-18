# SharpBuy Domain Model

## Class Diagram

```mermaid
classDiagram
    %% Base Classes
    class Entity {
        <<abstract>>
        +Guid Id
        +IReadOnlyCollection~IDomainEvent~ DomainEvents
        +RaiseDomainEvent(IDomainEvent)
        +ClearDomainEvents()
    }

    %% Value Objects
    class Money {
        <<value object>>
        +decimal Amount
        +string Currency
        +operator +(Money, Money) Money
        +Zero(string) Money
    }

    %% Product Aggregate
    class Product {
        +string Name
        +string Description
        +Money Price
        +string MainPhotoPath
        +List~string~ PhotoPaths
        +Inventory Inventory
        +IReadOnlyCollection~ProductCategory~ ProductCategories
        +Create(name, description, quantity, priceAmount, priceCurrency, mainPhotoPath)$ Product
        +AddToCategory(Guid categoryId) Result
        +UpdatePrice(Money newPrice) Result
    }

    class Inventory {
        +int StockQuantity
        +int ReservedQuantity
        +int AvailableQuantity
        +Create(int stockQuantity)$ Inventory
        +Reserve(int quantity) Result
        +Release(int quantity) Result
    }

    class Category {
        +string Name
        +IReadOnlyCollection~ProductCategory~ ProductCategories
        +Create(string name)$ Category
    }

    class ProductCategory {
        +Guid ProductId
        +Guid CategoryId
        +Product Product
        +Category Category
    }

    %% User Aggregate
    class DomainUser {
        +string Email
        +string FirstName
        +string LastName
        +string PhoneNumber
        +bool IsEmailVerified
        +EmailVerificationToken? EmailVerificationToken
        +Cart Cart
        +IReadOnlyCollection~Order~ Orders
        +IReadOnlyCollection~Review~ Reviews
        +Create(email, firstName, lastName, phoneNumber)$ DomainUser
        +VerifyEmail() Result
    }

    class EmailVerificationToken {
        +string Token
        +DateTime ExpiresAt
        +bool IsExpired
        +Create()$ EmailVerificationToken
    }

    %% Cart Aggregate
    class Cart {
        +Guid OwnerId
        +IReadOnlyCollection~CartItem~ Items
        +Money TotalPrice
        +AddCartItem(productId, price, quantity) Result
        +RemoveCartItem(cartItemId) Result
        +Clear()
    }

    class CartItem {
        +Guid ProductId
        +Money Price
        +int Quantity
        +Money TotalPrice
    }

    %% Order Aggregate
    class Order {
        +Guid UserId
        +Guid AddressId
        +OrderStatus Status
        +Money TotalPrice
        +DateTime CreatedAt
        +IReadOnlyCollection~OrderItem~ Items
        +Address Address
        +Create(userId, addressId)$ Order
        +AddOrderItem(productId, productName, price, quantity) Result
        +MoveToStatus(OrderStatus newStatus) Result
    }

    class OrderItem {
        +Guid OrderId
        +Guid ProductId
        +string ProductName
        +Money Price
        +int Quantity
        +Money TotalPrice
    }

    class OrderStatus {
        <<enumeration>>
        Open
        Completed
        Cancelled
    }

    class Address {
        +Guid? UserId
        +string Street
        +string? ApartmentNumber
        +string City
        +string PostalCode
        +string Country
        +Create(street, apartmentNumber, city, postalCode, country)$ Address
    }

    %% Review Aggregate
    class Review {
        +Guid ProductId
        +Guid UserId
        +int Rating
        +string Comment
        +DateTime CreatedAt
        +Create(productId, userId, rating, comment)$ Review
    }

    %% Relationships
    Entity <|-- Product
    Entity <|-- Category
    Entity <|-- DomainUser
    Entity <|-- Cart
    Entity <|-- CartItem
    Entity <|-- Order
    Entity <|-- OrderItem
    Entity <|-- Address
    Entity <|-- Review
    Entity <|-- Inventory

    Product "1" --> "1" Inventory : has
    Product "1" --> "*" ProductCategory : categories
    Product --> Money : price

    Category "1" --> "*" ProductCategory : products

    DomainUser "1" --> "1" Cart : owns
    DomainUser "1" --> "*" Order : places
    DomainUser "1" --> "*" Review : writes
    DomainUser "1" --> "0..1" EmailVerificationToken : has

    Cart "1" --> "*" CartItem : contains
    CartItem --> Money : price

    Order "1" --> "*" OrderItem : contains
    Order "1" --> "1" Address : delivery address
    Order --> OrderStatus : status
    Order --> Money : total price
    OrderItem --> Money : price

    Review "1" --> "1" Product : for
    Review "1" --> "1" DomainUser : by
```

## Aggregate Roots

### Product Aggregate
- **Root**: Product
- **Entities**: Inventory, ProductCategory
- **Value Objects**: Money
- **Purpose**: Manages product catalog with inventory and categorization

### User Aggregate
- **Root**: DomainUser (User)
- **Entities**: EmailVerificationToken
- **Purpose**: User registration, authentication, and email verification

### Cart Aggregate
- **Root**: Cart
- **Entities**: CartItem
- **Value Objects**: Money
- **Purpose**: Shopping cart management before order placement

### Order Aggregate
- **Root**: Order
- **Entities**: OrderItem, Address
- **Value Objects**: Money, OrderStatus
- **Purpose**: Order processing and fulfillment

### Review Aggregate
- **Root**: Review
- **Purpose**: Product reviews and ratings

### Category Aggregate
- **Root**: Category
- **Entities**: ProductCategory (join entity)
- **Purpose**: Product categorization (many-to-many with Product)

## Key Design Patterns

### Entity Base Class
All domain entities inherit from `Entity` base class which provides:
- Unique identifier (`Guid Id`)
- Domain event collection and management
- Common entity behavior

### Value Objects
- **Money**: Immutable value object with amount and currency
- Supports arithmetic operations with currency validation
- Prevents mixing different currencies

### Factory Methods
All aggregates use static factory methods (e.g., `Create()`) for instantiation:
- Ensures valid initial state
- Encapsulates business rules
- Private constructors for EF Core

### Domain Events
Entities can raise domain events for cross-aggregate communication:
- `UserRegisteredDomainEvent`: Triggered when user registers
- `ProductPriceChangedDomainEvent`: Triggered on price updates
- Events published in `SaveChangesAsync` (transactional consistency)

## Relationships

### One-to-One
- Product ↔ Inventory
- DomainUser ↔ Cart
- Order → Address

### One-to-Many
- DomainUser → Orders
- DomainUser → Reviews
- Cart → CartItems
- Order → OrderItems
- Product → Reviews

### Many-to-Many
- Product ↔ Category (via ProductCategory join entity)

## Business Rules

### Product
- Price must be greater than zero
- Cannot change currency after creation
- Must belong to at least one category

### Cart
- Cannot add items with insufficient inventory
- Each cart item tracks product price at time of addition
- Total price calculated from all items

### Order
- Can only transition through valid status changes: Open → Completed/Cancelled
- Total price calculated from all order items
- Requires valid delivery address

### Inventory
- Stock quantity cannot be negative
- Reserved quantity tracks pending orders
- Available quantity = Stock - Reserved

### Review
- Rating must be between 1 and 5
- User can only review products once
- Cannot review products not purchased

## Database Schema Naming

- **Convention**: snake_case (via EFCore.NamingConventions)
- **Tables**: `products`, `categories`, `orders`, `carts`, etc.
- **Columns**: `product_id`, `created_at`, `price_amount`, etc.
- **Foreign Keys**: `FK_{Table}_{ReferencedTable}_{Column}`
- **Indexes**: Automatically generated on foreign keys and specified properties
