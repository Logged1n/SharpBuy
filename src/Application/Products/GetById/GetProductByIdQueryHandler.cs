using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Products;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Products.GetById;

internal sealed class GetProductByIdQueryHandler(IApplicationDbContext dbContext)
    : IQueryHandler<GetProductByIdQuery, ProductResponse>
{
    public async Task<Result<ProductResponse>> Handle(GetProductByIdQuery query, CancellationToken cancellationToken)
    {
        ProductResponse? product = await dbContext.Products
            .Where(p => p.Id == query.Id)
            .Select(p => new ProductResponse(
                p.Id,
                p.Name,
                p.Description,
                p.Price.Amount,
                p.Price.Currency,
                p.Inventory.Quantity,
                p.MainPhotoPath,
                p.PhotoPaths,
                p.Categories
                    .Join(dbContext.Categories,
                        pc => pc.CategoryId,
                        c => c.Id,
                        (pc, c) => new CategoryInfo(c.Id, c.Name))
                    .ToList()))
            .FirstOrDefaultAsync(cancellationToken);

        if (product is null)
        {
            return Result.Failure<ProductResponse>(ProductErrors.ProductNotFound(query.Id));
        }

        return product;
    }
}
