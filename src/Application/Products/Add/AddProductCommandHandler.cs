using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Categories;
using Domain.Products;
using SharedKernel;

namespace Application.Products.Add;
internal sealed class AddProductCommandHandler(IApplicationDbContext dbContext) : ICommandHandler<AddProductCommand, Guid>
{
    public async Task<Result<Guid>> Handle(AddProductCommand command, CancellationToken cancellationToken)
    {
        //TODO: fix with tests
        //ICollection<Category> categories = [..dbContext.Categories
        //    .Where(c => command.CategoryIds.Contains(c.Id))];
        //if (categories.Count != command.CategoryIds.Count)
        //    return Result.Failure<Guid>(ProductErrors.InconsistentData);

        //TODO stock & photo handling
        var product = Product.Create(command.Name, command.Description, command.Quantity,
            command.Price.Amount, command.Price.Currency, command.MainPhotoPath);

        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync(cancellationToken);

        return product.Id;
    }
}
