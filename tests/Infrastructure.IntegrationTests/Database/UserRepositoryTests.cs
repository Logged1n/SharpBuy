using Domain.Users;
using Domain.Carts;

namespace Infrastructure.IntegrationTests.Database;

public class UserRepositoryTests : BaseIntegrationTest
{
    public UserRepositoryTests(DatabaseFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task AddUser_ShouldPersistToDatabase()
    {
        // Arrange
        var user = User.Create(
            "test@example.com",
            "John",
            "Doe",
            "+1234567890");

        // Act
        DbContext.DomainUsers.Add(user);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        // Assert
        var savedUser = await DbContext.DomainUsers
            .Include(u => u.Cart)
            .FirstOrDefaultAsync(u => u.Id == user.Id);

        savedUser.Should().NotBeNull();
        savedUser!.Email.Should().Be("TEST@EXAMPLE.COM"); // Email is uppercase
        savedUser.FirstName.Should().Be("John");
        savedUser.LastName.Should().Be("Doe");
        savedUser.Cart.Should().NotBeNull();
        savedUser.Cart.OwnerId.Should().Be(user.Id);
    }

    [Fact]
    public async Task AddUser_WithAddress_ShouldPersistRelationship()
    {
        // Arrange
        var user = User.Create(
            "test@example.com",
            "John",
            "Doe",
            "+1234567890");

        user.AddAddress("123 Main St", "Apt 4", "New York", "10001", "USA");

        // Act
        DbContext.DomainUsers.Add(user);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        // Assert
        var savedUser = await DbContext.DomainUsers
            .Include(u => u.Addresses)
            .FirstOrDefaultAsync(u => u.Id == user.Id);

        savedUser.Should().NotBeNull();
        savedUser!.Addresses.Should().ContainSingle();
        savedUser.PrimaryAddressId.Should().NotBeNull();
        savedUser.Addresses.First().Id.Should().Be(savedUser.PrimaryAddressId);
    }

    [Fact]
    public async Task VerifyEmail_ShouldUpdateEmailVerifiedFlag()
    {
        // Arrange
        var user = User.Create(
            "test@example.com",
            "John",
            "Doe",
            "+1234567890");

        DbContext.DomainUsers.Add(user);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        // Act
        var trackedUser = await DbContext.DomainUsers.FindAsync(user.Id);
        trackedUser!.VerifyEmail();
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        // Assert
        var verifiedUser = await DbContext.DomainUsers.FindAsync(user.Id);
        verifiedUser!.EmailVerified.Should().BeTrue();
    }

    [Fact]
    public async Task FindUserByEmail_ShouldReturnCorrectUser()
    {
        // Arrange
        var user1 = User.Create("user1@example.com", "John", "Doe", "123456");
        var user2 = User.Create("user2@example.com", "Jane", "Smith", "789012");

        DbContext.DomainUsers.AddRange(user1, user2);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        // Act
        var foundUser = await DbContext.DomainUsers
            .FirstOrDefaultAsync(u => u.Email == "USER1@EXAMPLE.COM");

        // Assert
        foundUser.Should().NotBeNull();
        foundUser!.FirstName.Should().Be("John");
    }

    [Fact]
    public async Task UserCart_ShouldSupportCartOperations()
    {
        // Arrange
        var user = User.Create("test@example.com", "John", "Doe", "123456");
        DbContext.DomainUsers.Add(user);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        // Act
        var trackedUser = await DbContext.DomainUsers
            .Include(u => u.Cart)
            .FirstOrDefaultAsync(u => u.Id == user.Id);

        var productId = Guid.NewGuid();
        var unitPrice = new SharedKernel.ValueObjects.Money(29.99m, "USD");
        trackedUser!.Cart.AddCartItem(productId, unitPrice, 2);

        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        // Assert
        var userWithCart = await DbContext.DomainUsers
            .Include(u => u.Cart)
            .ThenInclude(c => c.Items)
            .FirstOrDefaultAsync(u => u.Id == user.Id);

        userWithCart!.Cart.Items.Should().ContainSingle();
        userWithCart.Cart.Items.First().ProductId.Should().Be(productId);
        userWithCart.Cart.Items.First().Quantity.Should().Be(2);
    }
}
