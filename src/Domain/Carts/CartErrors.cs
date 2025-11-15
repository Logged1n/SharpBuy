using SharedKernel;

namespace Domain.Carts;
public static class CartErrors
{
    public static Error ItemNotFound(Guid productId) => Error.NotFound(
        "Cart.ItemNotFound",
        $"Cart item with product ID {productId} not found.");

    public static Error ItemAlreadyExists(Guid productId) => Error.Conflict(
        "Cart.ItemAlreadyExists",
        $"Cart item with product ID {productId} already exists.");
}
