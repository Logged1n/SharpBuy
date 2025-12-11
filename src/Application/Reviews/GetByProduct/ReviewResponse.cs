namespace Application.Reviews.GetByProduct;

public sealed record ReviewResponse(
    Guid Id,
    int Score,
    string Title,
    string? Description,
    DateTime CreatedAt,
    string UserName);
