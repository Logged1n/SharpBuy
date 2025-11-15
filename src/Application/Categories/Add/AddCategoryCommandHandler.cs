using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Categories;
using Domain.Products;
using SharedKernel;

namespace Application.Categories.Add;
internal sealed class AddCategoryCommandHandler(IApplicationDbContext dbContext) : ICommandHandler<AddCategoryCommand, Guid>
{
    public async Task<Result<Guid>> Handle(AddCategoryCommand command, CancellationToken cancellationToken)
    {
        ICollection<Product>? products = null;
        if (command.ProductIds != null)
        {
            products = [..dbContext.Products
                .Where(p => command.ProductIds.Contains(p.Id))];

            if (products.Count != command.ProductIds.Count)
                return Result.Failure<Guid>(CategoryErrors.InconsistentData);
        }

        var category = Category.Create(command.Name);

        if (products != null)
        {
            foreach (Product product in products)
            {
                Result result = category.AddProduct(product.Id);
                if (result.IsFailure)
                    return Result.Failure<Guid>(result.Error);
            }
        }
        dbContext.Categories.Add(category);
        await dbContext.SaveChangesAsync(cancellationToken);

        return category.Id;
    }
}
