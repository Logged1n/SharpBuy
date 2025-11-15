using SharedKernel;

namespace Domain.Categories;

public static class CategoryErrors
{
    public static readonly Error InconsistentData = Error.Conflict(
        "Categories.InconsistentData",
        "Provided data conflicts with database data. Check sent related entities info.");

    public static Error ProductAlreadyInCategory(Guid productId, Guid categoryId) => Error.Conflict(
        "Categories.ProductAlreadyInCategory",
        $"The category with ID '{categoryId}' already contains the product with ID '{productId}'.");

    public static Error ProductNotInCategory(Guid productId, Guid categoryId) => Error.Conflict(
        "Categories.ProductNotInCategory",
        $"Product with ID '{productId}' is not in category with ID '{categoryId}'.");
}
