using Domain.Addresses;
using Domain.Carts;
using Domain.Orders;
using Domain.Reviews;
using Microsoft.AspNetCore.Identity;
using SharedKernel;

namespace Domain.Users;

public sealed class User : IdentityUser<Guid>, IEntity
{
    public new Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public Cart Cart { get; set; }
    public Address? PrimaryAddress { get; set; }
    public ICollection<Address> Addresses { get; set; } = [];
    public ICollection<Order> Orders { get; set; } = [];
    public ICollection<Review> Reviews { get; set; } = [];
    public bool EmailVerified { get; set; }

    private readonly List<IDomainEvent> _domainEvents = [];
    public List<IDomainEvent> DomainEvents => [.. _domainEvents];

    public void ClearDomainEvents()
    {
       _domainEvents.Clear();
    }

    public void Raise(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
}
