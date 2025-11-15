using Domain.Products;
using SharedKernel;

namespace Domain.Inventories;

public sealed class Inventory : Entity
{
    private Inventory() { }

    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public int ReservedQuantity { get; private set; }
    public DateTime LastUpdated { get; private set; }
    public int AvailableQuantity => Quantity - ReservedQuantity;

    public static Inventory Create(Guid productId, int quantity, int reservedQuantity = 0)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(productId, Guid.Empty);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
        ArgumentOutOfRangeException.ThrowIfNegative(reservedQuantity);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(reservedQuantity, quantity);

        return new Inventory
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            Quantity = quantity,
            ReservedQuantity = reservedQuantity,
            LastUpdated = DateTime.UtcNow
        };
    }

    public Result ReserveProduct(int quantity)
    {
        if (quantity > AvailableQuantity)
            return Result.Failure(InventoryErrors.InsufficientStock(quantity, AvailableQuantity));

        ReservedQuantity += quantity;
        LastUpdated = DateTime.UtcNow;
        return Result.Success();
    }

    public Result ReleaseProduct(int quantity)
    {
        if (quantity <= 0)
            return Result.Failure(InventoryErrors.InvalidQuantity);
        if (quantity > ReservedQuantity)
            throw new InvalidOperationException("Cannot release more than the reserved quantity."); // TODO: Custom exception
        ReservedQuantity -= quantity;
        LastUpdated = DateTime.UtcNow;
        return Result.Success();
    }

    public Result AddStock(int quantity)
    {
        if (quantity <= 0)
            return Result.Failure(InventoryErrors.InvalidQuantity);

        Quantity += quantity;
        LastUpdated = DateTime.UtcNow;
        return Result.Success();
    }
    public Result RemoveStock(int quantity)
    {
        if (quantity <= 0)
            return Result.Failure(InventoryErrors.InvalidQuantity);

        if (quantity > AvailableQuantity)
            return Result.Failure(InventoryErrors.InsufficientStock(quantity, AvailableQuantity));

        Quantity -= quantity;
        LastUpdated = DateTime.UtcNow;
        return Result.Success();
    }
}
