using SharedKernel;

namespace Domain.Orders;

public static class OrderItemErrors
{
    public static readonly Error InvalidQuantity = Error.Failure(
        "OrderItem.InvalidQuantity",
        "Quantity must be greater than zero.");
}
