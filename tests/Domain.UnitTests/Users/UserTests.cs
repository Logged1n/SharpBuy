using Domain.Users;

namespace Domain.UnitTests.Users;

public class UserTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateUser()
    {
        // Arrange
        string email = "test@example.com";
        string firstName = "John";
        string lastName = "Doe";
        string phoneNumber = "+1234567890";

        // Act
        var user = DomainUser.Create(email, firstName, lastName, phoneNumber);

        // Assert
        user.ShouldNotBeNull();
        user.Id.ShouldNotBe(Guid.Empty);
        user.Email.ShouldBe(email.ToUpperInvariant());
        user.FirstName.ShouldBe(firstName);
        user.LastName.ShouldBe(lastName);
        user.PhoneNumber.ShouldBe(phoneNumber);
        user.EmailVerified.ShouldBeFalse();
        user.Cart.ShouldNotBeNull();
        user.Cart.OwnerId.ShouldBe(user.Id);
        user.FullName.ShouldBe($"{firstName} {lastName}");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithInvalidEmail_ShouldThrowArgumentException(string? invalidEmail)
    {
        // Act
        Action act = () => DomainUser.Create(invalidEmail!, "John", "Doe", "123456");

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithInvalidFirstName_ShouldThrowArgumentException(string? invalidFirstName)
    {
        // Act
        Action act = () => DomainUser.Create("test@example.com", invalidFirstName!, "Doe", "123456");

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithInvalidLastName_ShouldThrowArgumentException(string? invalidLastName)
    {
        // Act
        Action act = () => DomainUser.Create("test@example.com", "John", invalidLastName!, "123456");

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    [Fact]
    public void AddAddress_WithValidData_ShouldSucceed()
    {
        // Arrange
        DomainUser user = CreateValidUser();
        string line1 = "123 Main St";
        string city = "New York";
        string postalCode = "10001";
        string country = "USA";

        // Act
        Result result = user.AddAddress(line1, null, city, postalCode, country);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        user.Addresses.ShouldHaveSingleItem();
        user.PrimaryAddressId.ShouldNotBeNull();
        user.PrimaryAddressId.ShouldBe(user.Addresses.First().Id);
    }

    [Fact]
    public void AddAddress_MultipleAddresses_ShouldKeepFirstAsPrimary()
    {
        // Arrange
        DomainUser user = CreateValidUser();
        user.AddAddress("123 Main St", null, "New York", "10001", "USA");
        Guid? firstAddressId = user.PrimaryAddressId;

        // Act
        user.AddAddress("456 Oak Ave", null, "Los Angeles", "90001", "USA");

        // Assert
        user.Addresses.Count.ShouldBe(2);
        user.PrimaryAddressId.ShouldBe(firstAddressId);
    }

    [Fact]
    public void VerifyEmail_WhenNotVerified_ShouldSucceed()
    {
        // Arrange
        DomainUser user = CreateValidUser();

        // Act
        Result result = user.VerifyEmail();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        user.EmailVerified.ShouldBeTrue();
    }

    [Fact]
    public void VerifyEmail_WhenAlreadyVerified_ShouldReturnFailure()
    {
        // Arrange
        DomainUser user = CreateValidUser();
        user.VerifyEmail();

        // Act
        Result result = user.VerifyEmail();

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(UserErrors.EmailAlreadyVerified);
    }

    [Fact]
    public void Email_ShouldBeStoredInUppercase()
    {
        // Arrange
        string email = "test@example.com";

        // Act
        var user = DomainUser.Create(email, "John", "Doe", "123456");

        // Assert
        user.Email.ShouldBe(email.ToUpperInvariant());
    }

    private static DomainUser CreateValidUser()
    {
        return DomainUser.Create(
            "test@example.com",
            "John",
            "Doe",
            "+1234567890");
    }
}
