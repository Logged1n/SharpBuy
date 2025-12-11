using Application.Abstractions.Messaging;

namespace Application.Reviews.GetByProduct;

public sealed record GetReviewsByProductQuery(Guid ProductId) : IQuery<List<ReviewResponse>>;
