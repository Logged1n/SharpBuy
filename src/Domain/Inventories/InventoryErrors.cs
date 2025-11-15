using SharedKernel;

namespace Domain.Inventories;

public static class InventoryErrors
{
    public static readonly Error InvalidProductId = Error.Failure(
        "Inventory.InvalidProductId",
        "Product ID cannot be empty.");

    public static readonly Error NegativeQuantity = Error.Failure(
        "Inventory.NegativeQuantity",
        "Quantity cannot be negative.");

    public static readonly Error NegativeReservedQuantity = Error.Failure(
        "Inventory.NegativeReservedQuantity",
        "Reserved quantity cannot be negative.");

    public static readonly Error ReservedExceedsTotal = Error.Failure(
        "Inventory.ReservedExceedsTotal",
        "Reserved quantity cannot exceed total quantity.");

    public static readonly Error InvalidQuantity = Error.Failure(
        "Inventory.InvalidQuantity",
        "Quantity must be greater than zero.");

    public static Error InsufficientStock(int requested, int available) => Error.Problem(
        "Inventory.InsufficientStock",
        $"Insufficient stock. Requested: {requested}, Available: {available}");

    public static Error InsufficientReservedStock(int requested, int reserved) => Error.Problem(
        "Inventory.InsufficientReservedStock",
        $"Cannot release {requested}. Only {reserved} reserved.");

    public static readonly Error NotFound = Error.NotFound(
        "Inventory.NotFound",
        "Inventory not found.");

    public static Error ProductNotFound(Guid productId) => Error.NotFound(
        "Inventory.ProductNotFound",
        $"Inventory for product {productId} not found.");

    public static Error AlreadyExists(Guid productId) => Error.Conflict(
        "Inventory.AlreadyExists",
        $"Inventory for product {productId} already exists.");
}
