using Domain.Users;
using SharedKernel;
using SharedKernel.ValueObjects;

namespace Domain.Carts;

public sealed class Cart : Entity
{
    public ICollection<CartItem> Items { get; set; } = [];
    public Money Total { get; set; } = new Money(0m, string.Empty);
    public Guid OwnerId { get; set; }
    public User Owner { get; set; }
}
