using Domain.Products;
using Domain.Users;
using SharedKernel;

namespace Domain.Reviews;
public sealed class Review : Entity
{
    private Review() { }

    public Guid Id { get; private set; }
    public int Score { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid UserId { get; private set; }
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static Review Create(int score, Guid productId, Guid userId, string title, string? description)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(productId, Guid.Empty);
        ArgumentOutOfRangeException.ThrowIfEqual(userId, Guid.Empty);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(score);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(score, 5);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        return new Review()
        {
            Id = Guid.NewGuid(),
            Score = score,
            ProductId = productId,
            UserId = userId,
            Title = title,
            Description = description,
            CreatedAt = DateTime.UtcNow
        };
    }
}
