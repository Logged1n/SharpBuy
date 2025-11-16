using System.Collections.Generic;
using Domain.Categories;
using Domain.Inventories;
using Domain.Reviews;
using SharedKernel;
using SharedKernel.ValueObjects;
using Domain.ProductCategories;

namespace Domain.Products;

public sealed class Product : Entity
{
    private Product() { }

    private readonly List<ProductCategory> _categories = new();

    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public Money Price { get; private set; }
    public Guid InventoryId { get; private set; }
    public Inventory Inventory { get; private set; }
    public IReadOnlyCollection<ProductCategory> Categories => _categories.AsReadOnly();
    public string MainPhotoPath { get; private set; }
    public ICollection<string> PhotoPaths { get; private set; }

    public static Product Create(
        string name,
        string description,
        int quantity,
        decimal priceAmount,
        string priceCurrency,
        string mainPhotoPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        ArgumentException.ThrowIfNullOrWhiteSpace(priceCurrency);
        ArgumentException.ThrowIfNullOrWhiteSpace(mainPhotoPath);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);

        var productId = Guid.NewGuid();
        var inventory = Inventory.Create(productId, quantity);
        return new Product()
        {
            Id = productId,
            Name = name,
            Description = description,
            Price = new Money(priceAmount, priceCurrency),
            MainPhotoPath = mainPhotoPath,
            PhotoPaths = [mainPhotoPath],
            Inventory = inventory,
            InventoryId = inventory.Id
        };
    }

    public void AddPhoto(string photoPath) => PhotoPaths.Add(photoPath);

    public Result AddToCategory(Guid categoryId)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(categoryId, Guid.Empty);
        if (_categories.Any(pc => pc.CategoryId == categoryId))
            return Result.Failure(ProductErrors.AlreadyInCategory(Id, categoryId));

        _categories.Add(ProductCategory.Create(Id, categoryId));
        return Result.Success();
    }

    public Result RemoveFromCategory(Guid categoryId)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(categoryId, Guid.Empty);
        ProductCategory? productCategory = _categories.FirstOrDefault(pc => pc.CategoryId == categoryId);
        if (productCategory == null)
            return Result.Failure(ProductErrors.NotInCategory(Id, categoryId));
        if (_categories.Count <= 1)
            return Result.Failure(ProductErrors.NoCategoriesAssigned);

        _categories.Remove(productCategory);
        return Result.Success();
    }
}
