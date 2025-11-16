using Domain.Users;
using SharedKernel;
using SharedKernel.ValueObjects;

namespace Domain.Carts;

public sealed class Cart : Entity
{
    private readonly List<CartItem> _items = new();
    private Cart() { }

    public ICollection<CartItem> Items => _items.AsReadOnly();
    public Guid OwnerId { get; private set; }
    public Money Total => _items
            .Select(item => item.TotalPrice)
            .Aggregate((a, b) => a + b);

    public static Cart Create(Guid ownerId)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(ownerId, Guid.Empty);

        return new Cart { OwnerId = ownerId };
    }

    public Result AddCartItem(Guid productId, Money unitPrice, int quantity)
    {
        if (quantity <= 0)
            return Result.Failure(CartItemErrors.InvalidQuantity);

        CartItem? item = Items.FirstOrDefault(oi => oi.ProductId == productId);
        if (item != null)
            return Result.Failure(CartErrors.ItemAlreadyExists(productId));

        _items.Add(CartItem.Create(OwnerId, productId, unitPrice, quantity));

        return Result.Success();
    }

    public Result RemoveCartItem(Guid productId)
    {
        CartItem? item = Items.FirstOrDefault(oi => oi.ProductId == productId);
        if (item == null)
            return Result.Failure(CartErrors.ItemNotFound(productId));

        _items.Remove(item);

        return Result.Success();
    }

    public Result ChnageItemQuantity(Guid productId, int quantity)
    {
        if (quantity <= 0)
            return Result.Failure(CartItemErrors.InvalidQuantity);

        CartItem? item = _items.FirstOrDefault(oi => oi.ProductId == productId);
        if (item == null)
            return Result.Failure(CartErrors.ItemNotFound(productId));

        item.UpdateQuantity(quantity);

        return Result.Success();
    }

}
