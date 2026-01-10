using Domain.Addresses;
using Domain.Carts;
using Domain.Orders;
using Domain.Reviews;
using SharedKernel;
using SharedKernel.Dtos;

namespace Domain.Users;

public sealed class DomainUser : Entity
{
    private readonly List<Address> _addresses = [];
    //private readonly List<Order> _orders = [];

    private DomainUser() { }

    public Guid Id { get; private set; }
    public string Email { get; private set; }
    public string FirstName { get;private set; }
    public string LastName { get; private set; }
    public string PhoneNumber { get; private set; }
    public Cart Cart { get; private set; }
    public Guid? PrimaryAddressId { get; private set; }
    public bool EmailVerified { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public IReadOnlyCollection<Address> Addresses => _addresses.AsReadOnly();
    //public IReadOnlyCollection<Order> Orders => _orders.AsReadOnly();
    public string FullName => $"{FirstName} {LastName}";

    public static DomainUser Create(
        string email,
        string firstName,
        string lastName,
        string phoneNumber)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        var userId = Guid.NewGuid();
        var cart = Cart.Create(userId);

        return new DomainUser
        {
            Id = userId,
            Email = email.ToUpperInvariant(),
            FirstName = firstName,
            LastName = lastName,
            PhoneNumber = phoneNumber ?? string.Empty,
            EmailVerified = false,
            CreatedAt = DateTime.UtcNow,
            Cart = cart
        };
    }

    public Result AddAddress(string line1, string? line2, string city, string postalCode, string country)
    {
        var address = Address.Create(line1, line2, city, postalCode, country);

        _addresses.Add(address);

        PrimaryAddressId ??= address.Id;

        return Result.Success();
    }

    public Result VerifyEmail()
    {
        if (EmailVerified)
            return Result.Failure(UserErrors.EmailAlreadyVerified);

        EmailVerified = true;

        return Result.Success();
    }

    public Result Update(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            return Result.Failure(UserErrors.InvalidFirstName);

        if (string.IsNullOrWhiteSpace(lastName))
            return Result.Failure(UserErrors.InvalidLastName);

        FirstName = firstName;
        LastName = lastName;

        return Result.Success();
    }
}
