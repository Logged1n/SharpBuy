using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Products;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Products.Update;

internal sealed class UpdateProductCommandHandler(IApplicationDbContext dbContext)
    : ICommandHandler<UpdateProductCommand>
{
    public async Task<Result> Handle(UpdateProductCommand command, CancellationToken cancellationToken)
    {
        Product? product = await dbContext.Products
            .FirstOrDefaultAsync(p => p.Id == command.Id, cancellationToken);

        if (product is null)
        {
            return Result.Failure(ProductErrors.ProductNotFound(command.Id));
        }

        Result result = product.Update(
            command.Name,
            command.Description,
            command.PriceAmount,
            command.PriceCurrency);

        if (result.IsFailure)
        {
            return result;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
