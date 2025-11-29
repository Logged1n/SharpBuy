using SharedKernel;

namespace Domain.Categories;

public static class CategoryErrors
{
    public static readonly Error InconsistentData = Error.Conflict(
        "Categories.InconsistentData",
        "Provided data conflicts with database data. Check sent related entities info.");

    public static Error CategoryNotFound(Guid categoryId) => Error.NotFound(
        "Categories.NotFound",
        $"Category with ID '{categoryId}' was not found.");

    public static Error ProductAlreadyInCategory(Guid productId, Guid categoryId) => Error.Conflict(
        "Categories.ProductAlreadyInCategory",
        $"The category with ID '{categoryId}' already contains the product with ID '{productId}'.");

    public static Error ProductNotInCategory(Guid productId, Guid categoryId) => Error.Conflict(
        "Categories.ProductNotInCategory",
        $"Product with ID '{productId}' is not in category with ID '{categoryId}'.");

    public static readonly Error CannotDelete = Error.Conflict(
        "Categories.CannotDelete",
        "Category cannot be deleted because it has products assigned.");

    public static readonly Error NameAlreadyExists = Error.Conflict(
        "Categories.NameAlreadyExists",
        "A category with this name already exists.");
}
