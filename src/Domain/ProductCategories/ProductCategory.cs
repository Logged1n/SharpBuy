using SharedKernel;

namespace Domain.ProductCategories;

public sealed class ProductCategory : Entity
{
    private ProductCategory() { }

    public Guid ProductId { get; private set; }
    public Guid CategoryId { get; private set; }

    //Only domain layer should be able to create intersection entities
    internal static ProductCategory Create(Guid productId, Guid categoryId)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(productId, Guid.Empty);
        ArgumentOutOfRangeException.ThrowIfEqual(categoryId, Guid.Empty);
        return new ProductCategory
        {
            ProductId = productId,
            CategoryId = categoryId
        };
    }
}
