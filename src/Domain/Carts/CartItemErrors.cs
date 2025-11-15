using SharedKernel;

namespace Domain.Carts;

public static class CartItemErrors
{
    public static readonly Error InvalidQuantity = Error.Failure(
        "CartItem.InvalidQuantity",
        "Quantity must be greater than zero.");
}
