using Application.Abstractions.Messaging;

namespace Application.Reviews.Add;

public sealed record AddReviewCommand(
    Guid UserId,
    Guid ProductId,
    int Score,
    string Title,
    string? Description) : ICommand<Guid>;
