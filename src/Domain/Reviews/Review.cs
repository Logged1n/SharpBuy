using Domain.Products;
using Domain.Users;
using SharedKernel;

namespace Domain.Reviews;
public sealed class Review : Entity
{
    public Guid Id { get; set; }
    public int Score { get; set; }
    public Product Product { get; set; }
    public User User { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
