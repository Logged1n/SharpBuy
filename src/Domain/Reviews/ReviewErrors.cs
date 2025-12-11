using SharedKernel;

namespace Domain.Reviews;

public static class ReviewErrors
{
    public static Error NotFound(Guid reviewId) => Error.NotFound(
        "Reviews.NotFound",
        $"The review with Id '{reviewId}' was not found");

    public static Error UserHasNotPurchasedProduct(Guid userId, Guid productId) => Error.Problem(
        "Reviews.UserHasNotPurchasedProduct",
        $"User '{userId}' has not purchased product '{productId}'");

    public static Error AlreadyReviewed(Guid userId, Guid productId) => Error.Conflict(
        "Reviews.AlreadyReviewed",
        $"User '{userId}' has already reviewed product '{productId}'");

    public static readonly Error InvalidScore = Error.Problem(
        "Reviews.InvalidScore",
        "Review score must be between 1 and 5");
}
