using Domain.Carts;
using Domain.Products;
using Domain.Users;

namespace Infrastructure.IntegrationTests.Database;

public class UserRepositoryTests : BaseIntegrationTest
{
    [Fact]
    public async Task AddUser_ShouldPersistToDatabase()
    {
        // Arrange
        var user = DomainUser.Create(
            "test@example.com",
            "John",
            "Doe",
            "123456789");

        // Act
        await ExecuteInTransactionAsync(async context =>
        {
            context.DomainUsers.Add(user);
            return await context.SaveChangesAsync();
        });

        // Assert
        await ExecuteInTransactionAsync(async context =>
        {
            DomainUser? savedUser = await context.DomainUsers
                .Include(u => u.Cart)
                .FirstOrDefaultAsync(u => u.Id == user.Id);

            savedUser.ShouldNotBeNull();
            savedUser!.Email.ShouldBe("TEST@EXAMPLE.COM"); // Email is uppercase
            savedUser.FirstName.ShouldBe("John");
            savedUser.LastName.ShouldBe("Doe");
            savedUser.Cart.ShouldNotBeNull();
            savedUser.Cart.OwnerId.ShouldBe(user.Id);
            return Task.CompletedTask;
        });
    }

    [Fact]
    public async Task AddUser_WithAddress_ShouldPersistRelationship()
    {
        // Arrange
        var user = DomainUser.Create(
            "test@example.com",
            "John",
            "Doe",
            "123456789");

        user.AddAddress("123 Main St", "Apt 4", "New York", "10001", "USA");

        // Act
        await ExecuteInTransactionAsync(async context =>
        {
            context.DomainUsers.Add(user);
            return await context.SaveChangesAsync();
        });

        // Assert
        await ExecuteInTransactionAsync(async context =>
        {
            DomainUser? savedUser = await context.DomainUsers
                .Include(u => u.Addresses)
                .FirstOrDefaultAsync(u => u.Id == user.Id);

            savedUser.ShouldNotBeNull();
            savedUser!.Addresses.ShouldHaveSingleItem();
            savedUser.PrimaryAddressId.ShouldNotBeNull();
            savedUser.Addresses.First().Id.ShouldBeEquivalentTo(savedUser.PrimaryAddressId);
            return Task.CompletedTask;
        });
    }

    [Fact]
    public async Task VerifyEmail_ShouldUpdateEmailVerifiedFlag()
    {
        // Arrange
        var user = DomainUser.Create(
            "test@example.com",
            "John",
            "Doe",
            "123456789");

        await ExecuteInTransactionAsync(async context =>
        {
            context.DomainUsers.Add(user);
            return await context.SaveChangesAsync();
        });

        // Act
        await ExecuteInTransactionAsync(async context =>
        {
            DomainUser? trackedUser = await context.DomainUsers.FindAsync(user.Id);
            trackedUser!.VerifyEmail();
            return await context.SaveChangesAsync();
        });

        // Assert
        await ExecuteInTransactionAsync(async context =>
        {
            DomainUser? verifiedUser = await context.DomainUsers.FindAsync(user.Id);
            verifiedUser!.EmailVerified.ShouldBeTrue();
            return Task.CompletedTask;
        });
    }

    [Fact]
    public async Task FindUserByEmail_ShouldReturnCorrectUser()
    {
        // Arrange
        var user1 = DomainUser.Create("user1@example.com", "John", "Doe", "123456");
        var user2 = DomainUser.Create("user2@example.com", "Jane", "Smith", "789012");

        await ExecuteInTransactionAsync(async context =>
        {
            context.DomainUsers.AddRange(user1, user2);
            return await context.SaveChangesAsync();
        });

        // Act & Assert
        await ExecuteInTransactionAsync(async context =>
        {
            DomainUser? foundUser = await context.DomainUsers
                .FirstOrDefaultAsync(u => u.Email == "USER1@EXAMPLE.COM");

            foundUser.ShouldNotBeNull();
            foundUser!.FirstName.ShouldBe("John");
            return Task.CompletedTask;
        });
    }

    [Fact]
    public async Task UserCart_ShouldSupportCartOperations()
    {
        // Arrange
        var user = DomainUser.Create("test@example.com", "John", "Doe", "123456");
        var product = Product.Create(
            "Test Product",
            "Test Description",
            10,
            29.99m,
            "USD",
            "/photos/test.jpg");

        var unitPrice = new SharedKernel.ValueObjects.Money(29.99m, "USD");

        await ExecuteInTransactionAsync(async context =>
        {
            context.DomainUsers.Add(user);
            context.Products.Add(product); // Dodaj produkt do bazy!
            return await context.SaveChangesAsync();
        });

        // Act
        await ExecuteInTransactionAsync(async context =>
        {
            DomainUser? trackedUser = await context.DomainUsers
                .Include(u => u.Cart)
                .FirstOrDefaultAsync(u => u.Id == user.Id);

            trackedUser!.Cart.AddCartItem(product.Id, unitPrice, 2);
            return await context.SaveChangesAsync();
        });

        // Assert
        await ExecuteInTransactionAsync(async context =>
        {
            DomainUser? userWithCart = await context.DomainUsers
                .Include(u => u.Cart)
                .ThenInclude(c => c.Items)
                .FirstOrDefaultAsync(u => u.Id == user.Id);

            userWithCart!.Cart.Items.ShouldHaveSingleItem();
            userWithCart.Cart.Items.First().ProductId.ShouldBe(product.Id);
            userWithCart.Cart.Items.First().Quantity.ShouldBe(2);
            return Task.CompletedTask;
        });
    }
}
