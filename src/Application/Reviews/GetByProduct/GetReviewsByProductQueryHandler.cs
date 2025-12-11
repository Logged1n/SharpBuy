using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Products;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Reviews.GetByProduct;

internal sealed class GetReviewsByProductQueryHandler(
    IApplicationDbContext dbContext) : IQueryHandler<GetReviewsByProductQuery, List<ReviewResponse>>
{
    public async Task<Result<List<ReviewResponse>>> Handle(
        GetReviewsByProductQuery query,
        CancellationToken cancellationToken)
    {
        // Check if product exists
        bool productExists = await dbContext.Products
            .AnyAsync(p => p.Id == query.ProductId, cancellationToken);

        if (!productExists)
        {
            return Result.Failure<List<ReviewResponse>>(ProductErrors.ProductNotFound(query.ProductId));
        }

        // Get reviews with user information
        List<ReviewResponse> reviews = await dbContext.Reviews
            .Where(r => r.ProductId == query.ProductId)
            .OrderByDescending(r => r.CreatedAt)
            .Join(
                dbContext.DomainUsers,
                review => review.UserId,
                user => user.Id,
                (review, user) => new ReviewResponse(
                    review.Id,
                    review.Score,
                    review.Title,
                    review.Description,
                    review.CreatedAt,
                    $"{user.FirstName} {user.LastName}"))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return reviews;
    }
}
