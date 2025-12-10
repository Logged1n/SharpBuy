using SharedKernel;

namespace Domain.Orders;

public static class OrderErrors
{
    public static Error NotFound(Guid orderId) =>
        Error.NotFound(
            "Order.NotFound",
            $"The order with ID '{orderId}' was not found.");

    public static Error ItemAlreadyExists(Guid productId) =>
        Error.Conflict(
            "Order.ItemAlreadyExists",
            $"The order already contains an item with product ID '{productId}'.");

    public static readonly Error InvalidQuantity = Error.Failure(
        "Order.InvalidQuantity",
        "The quantity must be greater than zero.");

    public static readonly Error OrderNotOpen = Error.Conflict(
        "Order.OrderNotOpen",
        "The order is not open and cannot be modified.");

    public static readonly Error AlreadyFinished = Error.Conflict(
        "Order.AlreadyFinished",
        "The order status is Completed/Cancelled and it's state can not me changed anymore.");
}
