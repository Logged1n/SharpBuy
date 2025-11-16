using Application.Users.Register;
using Domain.Users;
using SharedKernel.Dtos;

namespace Application.IntegrationTests.Users;

public class RegisterUserCommandHandlerTests : BaseIntegrationTest
{
    public RegisterUserCommandHandlerTests(DatabaseFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldCreateUser()
    {
        // Arrange
        var passwordHasher = new TestPasswordHasher();
        var handler = new RegisterUserCommandHandler(DbContext, passwordHasher);
        var command = new RegisterUserCommand(
            "test@example.com",
            "SecurePassword123!",
            "John",
            "Doe",
            "+1234567890",
            null);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBe(Guid.Empty);

        // Verify user was created in database
        var user = await DbContext.DomainUsers
            .Include(u => u.Cart)
            .FirstOrDefaultAsync(u => u.Id == result.Value);

        user.ShouldNotBeNull();
        user.Email.ShouldBe("TEST@EXAMPLE.COM"); // Email stored in uppercase
        user.FirstName.ShouldBe("John");
        user.LastName.ShouldBe("Doe");
        user.PhoneNumber.ShouldBe("+1234567890");
        user.EmailVerified.ShouldBeFalse();
        user.Cart.ShouldNotBeNull();
        user.Cart.OwnerId.ShouldBe(user.Id);
    }

    [Fact]
    public async Task Handle_WithDuplicateEmail_ShouldReturnFailure()
    {
        // Arrange
        var email = "duplicate@example.com";
        var existingUser = User.Create(email, "John", "Doe", "+1234567890");
        DbContext.DomainUsers.Add(existingUser);
        await DbContext.SaveChangesAsync();

        var passwordHasher = new TestPasswordHasher();
        var handler = new RegisterUserCommandHandler(DbContext, passwordHasher);
        var command = new RegisterUserCommand(
            email,
            "Password123!",
            "Jane",
            "Smith",
            "+9876543210",
            null);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(UserErrors.EmailNotUnique);
    }

    [Fact]
    public async Task Handle_WithAddress_ShouldCreateUserWithAddress()
    {
        // Arrange
        var passwordHasher = new TestPasswordHasher();
        var handler = new RegisterUserCommandHandler(DbContext, passwordHasher);
        var address = new AddressDto(
            "123 Main St",
            "Apt 4",
            "New York",
            "10001",
            "USA");

        var command = new RegisterUserCommand(
            "test@example.com",
            "Password123!",
            "John",
            "Doe",
            "+1234567890",
            address);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        var user = await DbContext.DomainUsers
            .Include(u => u.Addresses)
            .FirstOrDefaultAsync(u => u.Id == result.Value);

        user.ShouldNotBeNull();
        user.Addresses.ShouldHaveSingleItem();
        user.PrimaryAddressId.ShouldNotBeNull();

        var userAddress = user.Addresses.First();
        userAddress.Line1.ShouldBe("123 Main St");
        userAddress.Line2.ShouldBe("Apt 4");
        userAddress.City.ShouldBe("New York");
        userAddress.PostalCode.ShouldBe("10001");
        userAddress.Country.ShouldBe("USA");
    }

    [Fact]
    public async Task Handle_ShouldHashPassword()
    {
        // Arrange
        var passwordHasher = new TestPasswordHasher();
        var handler = new RegisterUserCommandHandler(DbContext, passwordHasher);
        var command = new RegisterUserCommand(
            "test@example.com",
            "MySecretPassword",
            "John",
            "Doe",
            "+1234567890",
            null);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        var applicationUser = await DbContext.Set<Infrastructure.Authentication.ApplicationUser>()
            .FirstOrDefaultAsync(u => u.DomainUserId == result.Value);

        applicationUser.ShouldNotBeNull();
        applicationUser.PasswordHash.ShouldBe("hashed_MySecretPassword");
    }

    [Fact]
    public async Task Handle_ShouldCreateCartForUser()
    {
        // Arrange
        var passwordHasher = new TestPasswordHasher();
        var handler = new RegisterUserCommandHandler(DbContext, passwordHasher);
        var command = new RegisterUserCommand(
            "test@example.com",
            "Password123!",
            "John",
            "Doe",
            "+1234567890",
            null);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        var cart = await DbContext.Carts
            .FirstOrDefaultAsync(c => c.OwnerId == result.Value);

        cart.ShouldNotBeNull();
        cart.OwnerId.ShouldBe(result.Value);
        cart.Items.ShouldBeEmpty();
    }
}
