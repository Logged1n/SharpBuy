using Application.Abstractions.Authentication;
using Application.Users.Register;
using Domain.Addresses;
using Domain.Carts;
using Domain.Users;
using Infrastructure.Authentication;
using Microsoft.EntityFrameworkCore;
using SharedKernel;
using SharedKernel.Dtos;
using Shouldly;
using Tests.Integration.Infrastructure;
using Xunit;

namespace Application.IntegrationTests.Users;

public class RegisterUserCommandHandlerTests : IntegrationTestBase
{
    [Fact]
    public async Task Handle_WithValidData_ShouldCreateUser()
    {
        // Arrange
        var passwordHasher = new PasswordHasher();
        var handler = new RegisterUserCommandHandler(DbContext, passwordHasher);
        var command = new RegisterUserCommand(
            "test@example.com",
            "John",
            "Doe",
            "SecurePassword123!",
            "123456789",
            null,
            null);

        // Act
        Result<Guid> result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBe(Guid.Empty);

        DomainUser? user = await DbContext.DomainUsers
            .Include(u => u.Cart)
            .FirstOrDefaultAsync(u => u.Id == result.Value);

        user.ShouldNotBeNull();
        user.Email.ShouldBe("TEST@EXAMPLE.COM");
        user.FirstName.ShouldBe("John");
        user.LastName.ShouldBe("Doe");
        user.PhoneNumber.ShouldBe("123456789");
        user.EmailVerified.ShouldBeFalse();
        user.Cart.ShouldNotBeNull();
        user.Cart.OwnerId.ShouldBe(user.Id);
    }

    [Fact]
    public async Task Handle_WithDuplicateEmail_ShouldReturnFailure()
    {
        // Arrange
        string email = "duplicate@example.com";
        var existingUser = DomainUser.Create(email, "John", "Doe", "123456789");
        DbContext.DomainUsers.Add(existingUser);
        await DbContext.SaveChangesAsync();

        var passwordHasher = new PasswordHasher();
        var handler = new RegisterUserCommandHandler(DbContext, passwordHasher);
        var command = new RegisterUserCommand(
            email,
            "Jane",
            "Smith",
            "Password123!",
            "987654321",
            null,
            null);

        // Act
        await handler.Handle(command, CancellationToken.None).ShouldThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task Handle_WithAddress_ShouldCreateUserWithAddress()
    {
        // Arrange
        var passwordHasher = new PasswordHasher();
        var handler = new RegisterUserCommandHandler(DbContext, passwordHasher);
        var address = new AddressDto(
            "123 Main St",
            "Apt 4",
            "New York",
            "10001",
            "USA");

        var command = new RegisterUserCommand(
            "test@example.com",
            "John",
            "Doe",
            "Password123!",
            "123456789",
            address,
            null);

        // Act
        Result<Guid> result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        DomainUser? user = await DbContext.DomainUsers
            .Include(u => u.Addresses)
            .FirstOrDefaultAsync(u => u.Id == result.Value);

        user.ShouldNotBeNull();
        user.Addresses.ShouldHaveSingleItem();
        user.PrimaryAddressId.ShouldNotBeNull();

        Address userAddress = user.Addresses.First();
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
        var passwordHasher = new PasswordHasher();
        var handler = new RegisterUserCommandHandler(DbContext, passwordHasher);
        var command = new RegisterUserCommand(
            "test@example.com",
            "John",
            "Doe",
            "MySecretPassword",
            "123456789",
            null,
            null);

        // Act
        Result<Guid> result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        ApplicationUser? applicationUser = await DbContext.Set<ApplicationUser>()
            .FirstOrDefaultAsync(u => u.DomainUserId == result.Value);

        applicationUser.ShouldNotBeNull();
    }

    [Fact]
    public async Task Handle_ShouldCreateCartForUser()
    {
        // Arrange
        var passwordHasher = new PasswordHasher();
        var handler = new RegisterUserCommandHandler(DbContext, passwordHasher);
        var command = new RegisterUserCommand(
            "test@example.com",
            "John",
            "Doe",
            "Password123!",
            "123456789",
            null,
            null);

        // Act
        Result<Guid> result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        Cart? cart = await DbContext.Carts
            .FirstOrDefaultAsync(c => c.OwnerId == result.Value);

        cart.ShouldNotBeNull();
        cart.OwnerId.ShouldBe(result.Value);
        cart.Items.ShouldBeEmpty();
    }
}
