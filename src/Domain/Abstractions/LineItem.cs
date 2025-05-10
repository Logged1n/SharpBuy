using Domain.Carts;
using Domain.Orders;
using Domain.Products;
using SharedKernel;

namespace Domain.Abstractions;

/// <summary>
/// Represents a single line item that can belong either to a <see cref="Cart"/> or an <see cref="Order"/>.
/// Uses a composite primary key of (ParentId, ProductId) to ensure uniqueness of Product per parent.
/// </summary>
/// <typeparam name="TParent">
/// The parent entity type—either <see cref="Cart"/> or <see cref="Order"/>—to which this line item belongs.
/// </typeparam>
public abstract class LineItem<TParent> : Entity
    where TParent : Entity
{
    /// <summary>
    /// Foreign key to the parent this line item belongs to.
    /// </summary>
    public Guid ParentId { get; set; }

    /// <summary>
    /// Navigation property to the parent entity.
    /// </summary>
    public TParent Parent { get; set; }

    /// <summary>
    /// Foreign key to the <see cref="Product"/> this line item represents.
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Navigation property to the <see cref="Product"/> being ordered or added to cart.
    /// </summary>
    public Product Product { get; set; }

    /// <summary>
    /// Quantity of the product in this line item.
    /// </summary>
    public int Quantity { get; set; }
}
