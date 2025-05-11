namespace Domain.Users;

public sealed class EmailVerificationToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedOnUtc { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresOnUtc { get; set; } = DateTime.UtcNow.AddDays(1);
    public User User { get; set; }
}
