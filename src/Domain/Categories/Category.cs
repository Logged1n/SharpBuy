using Domain.ProductCategories;
using Domain.Products;
using SharedKernel;

namespace Domain.Categories;

public sealed class Category
{
    private readonly List<ProductCategory> _products = [];

    private Category() { }

    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public IReadOnlyCollection<ProductCategory> Products => _products.AsReadOnly();

    public static Category Create(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new Category()
        {
            Id = Guid.NewGuid(),
            Name = name
        };
    }

    public Result AddProduct(Guid productId)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(productId, Guid.Empty);

        if (_products.Any(p => p.ProductId == productId))
            return Result.Failure(CategoryErrors.ProductAlreadyInCategory(productId, Id));

        var productCategory = ProductCategory.Create(productId, Id);
        _products.Add(productCategory);

        return Result.Success();
    }

    public Result RemoveProduct(Guid productId)
    {
        ProductCategory? productCategory = _products.FirstOrDefault(p => p.ProductId == productId);

        if (productCategory is null)
            return Result.Failure(CategoryErrors.ProductNotInCategory(productId, Id));

        _products.Remove(productCategory);
        return Result.Success();
    }
}
