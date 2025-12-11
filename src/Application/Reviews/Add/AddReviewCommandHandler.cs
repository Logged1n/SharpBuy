using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Orders;
using Domain.Products;
using Domain.Reviews;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Reviews.Add;

internal sealed class AddReviewCommandHandler(
    IApplicationDbContext dbContext) : ICommandHandler<AddReviewCommand, Guid>
{
    public async Task<Result<Guid>> Handle(AddReviewCommand command, CancellationToken cancellationToken)
    {
        // Get userId from command
        Guid userId = command.UserId;

        // Check if product exists
        bool productExists = await dbContext.Products
            .AnyAsync(p => p.Id == command.ProductId, cancellationToken);

        if (!productExists)
        {
            return Result.Failure<Guid>(ProductErrors.ProductNotFound(command.ProductId));
        }

        // Check if user has already reviewed this product
        bool alreadyReviewed = await dbContext.Reviews
            .AnyAsync(r => r.UserId == userId && r.ProductId == command.ProductId, cancellationToken);

        if (alreadyReviewed)
        {
            return Result.Failure<Guid>(ReviewErrors.AlreadyReviewed(userId, command.ProductId));
        }

        // Check if user has purchased this product (has a completed order with this product)
        bool hasPurchased = await dbContext.Orders
            .Where(o => o.UserId == userId && o.Status == OrderStatus.Completed)
            .SelectMany(o => o.Items)
            .AnyAsync(oi => oi.ProductId == command.ProductId, cancellationToken);

        if (!hasPurchased)
        {
            return Result.Failure<Guid>(ReviewErrors.UserHasNotPurchasedProduct(userId, command.ProductId));
        }

        // Create review
        var review = Review.Create(
            command.Score,
            command.ProductId,
            userId,
            command.Title,
            command.Description);

        dbContext.Reviews.Add(review);
        await dbContext.SaveChangesAsync(cancellationToken);

        return review.Id;
    }
}
