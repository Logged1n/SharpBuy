using Domain.Addresses;
using Domain.Carts;
using Domain.Orders;
using Domain.Reviews;
using SharedKernel;

namespace Domain.Users;

public sealed class User : Entity
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PasswordHash { get; set; }
    public Cart Cart { get; set; }
    public Address? PrimaryAddress { get; set; }
    public ICollection<Address> Addresses { get; set; } = [];
    public ICollection<Order> Orders { get; set; } = [];
    public ICollection<Review> Reviews { get; set; } = [];
    public bool IsEmailVerified { get; set; }
}
