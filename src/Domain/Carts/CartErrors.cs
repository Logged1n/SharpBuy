using SharedKernel;

namespace Domain.Carts;
public static class CartErrors
{
    public static Error CartNotFound(Guid cartId) => Error.NotFound(
        "Cart.NotFound",
        $"Cart with ID {cartId} not found.");

    public static Error ItemNotFound(Guid productId) => Error.NotFound(
        "Cart.ItemNotFound",
        $"Cart item with product ID {productId} not found.");

    public static Error ItemAlreadyExists(Guid productId) => Error.Conflict(
        "Cart.ItemAlreadyExists",
        $"Cart item with product ID {productId} already exists.");

    public static readonly Error InvalidQuantity = Error.Problem(
        "Cart.InvalidQuantity",
        "Cart item quantity must be greater than zero.");

    public static readonly Error InsufficientStock = Error.Conflict(
        "Cart.InsufficientStock",
        "Requested quantity exceeds available stock.");
}
