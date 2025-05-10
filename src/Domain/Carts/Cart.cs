using Domain.Users;
using SharedKernel;
using SharedKernel.ValueObjects;

namespace Domain.Carts;

public sealed class Cart : Entity
{
    public Guid Id { get; set; }
    public ICollection<CartItem> Items { get; set; } = [];
    public Money Total { get; set; }
    public User Owner { get; set; }
}
