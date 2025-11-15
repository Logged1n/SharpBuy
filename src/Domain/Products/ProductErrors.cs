using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedKernel;

namespace Domain.Products;
public static class ProductErrors
{
    public static readonly Error InconsistentData = Error.Conflict(
        "Products.InconsistentData",
        "Provided data conflicts with database data. Check sent related entities info.");

    public static Error ProductNotFound(Guid productId) =>
        Error.NotFound(
            "Products.NotFound",
            $"Product with ID '{productId}' was not found.");

    public static Error AlreadyInCategory(Guid productId, Guid categoryId) =>
        Error.Conflict(
            "Products.AlreadyInCategory",
            $"Product with ID '{productId}' is already in category with ID '{categoryId}'.");

    public static Error NotInCategory(Guid productId, Guid categoryId) =>
        Error.Conflict(
            "Products.NotInCategory",
            $"Product with ID '{productId}' is not in category with ID '{categoryId}'.");
    public static readonly Error NoCategoriesAssigned = Error.Conflict(
        "Products.NoCategoriesAssigned",
        "Product must be assigned to at least one category.");
}
