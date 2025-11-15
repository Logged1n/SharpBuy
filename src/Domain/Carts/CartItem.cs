using SharedKernel;
using SharedKernel.ValueObjects;

namespace Domain.Carts;

public sealed class CartItem : Entity
{
    private CartItem() { }

    public Guid CartId { get; private set; }
    public Guid ProductId { get; private set; }


    public int Quantity { get; private set; }
    public Money UnitPrice { get; private set; }
    public Money TotalPrice => UnitPrice * Quantity;
    public DateTime AddedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }


    internal static CartItem Create(Guid cartId, Guid productId, Money unitPrice, int quantity)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(cartId, Guid.Empty);
        ArgumentOutOfRangeException.ThrowIfEqual(productId, Guid.Empty);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);

        return new CartItem
        {
            CartId = cartId,
            ProductId = productId,
            Quantity = quantity,
            UnitPrice = unitPrice,
            AddedAt = DateTime.UtcNow
        };
    }

    internal Result UpdateQuantity(int newQuantity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(newQuantity);

        Quantity = newQuantity;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }
}
