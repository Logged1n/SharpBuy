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
        User? savedUser = await DbContext.DomainUsers
            .Include(u => u.Cart)
            .FirstOrDefaultAsync(u => u.Id == user.Id);

        savedUser.ShouldNotBeNull();
        savedUser!.Email.ShouldBe("TEST@EXAMPLE.COM"); // Email is uppercase
        savedUser.FirstName.ShouldBe("John");
        savedUser.LastName.ShouldBe("Doe");
        savedUser.Cart.ShouldNotBeNull();
        savedUser.Cart.OwnerId.ShouldBe(user.Id);
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
        User? savedUser = await DbContext.DomainUsers
            .Include(u => u.Addresses)
            .FirstOrDefaultAsync(u => u.Id == user.Id);

        savedUser.ShouldNotBeNull();
        savedUser!.Addresses.ShouldHaveSingleItem();
        savedUser.PrimaryAddressId.ShouldNotBeNull();
        savedUser.Addresses.First().Id.ShouldBeSameAs(savedUser.PrimaryAddressId);
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
        User? trackedUser = await DbContext.DomainUsers.FindAsync(user.Id);
        trackedUser!.VerifyEmail();
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        // Assert
        User? verifiedUser = await DbContext.DomainUsers.FindAsync(user.Id);
        verifiedUser!.EmailVerified.ShouldBeTrue();
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
        User? foundUser = await DbContext.DomainUsers
            .FirstOrDefaultAsync(u => u.Email == "USER1@EXAMPLE.COM");

        // Assert
        foundUser.ShouldNotBeNull();
        foundUser!.FirstName.ShouldBe("John");
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
        User? trackedUser = await DbContext.DomainUsers
            .Include(u => u.Cart)
            .FirstOrDefaultAsync(u => u.Id == user.Id);

        var productId = Guid.NewGuid();
        var unitPrice = new SharedKernel.ValueObjects.Money(29.99m, "USD");
        trackedUser!.Cart.AddCartItem(productId, unitPrice, 2);

        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        // Assert
        User? userWithCart = await DbContext.DomainUsers
            .Include(u => u.Cart)
            .ThenInclude(c => c.Items)
            .FirstOrDefaultAsync(u => u.Id == user.Id);

        userWithCart!.Cart.Items.ShouldHaveSingleItem();
        userWithCart.Cart.Items.First().ProductId.ShouldBe(productId);
        userWithCart.Cart.Items.First().Quantity.ShouldBe(2);
    }
}
