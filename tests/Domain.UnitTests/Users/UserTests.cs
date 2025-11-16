using Domain.Users;

namespace Domain.UnitTests.Users;

public class UserTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateUser()
    {
        // Arrange
        var email = "test@example.com";
        var firstName = "John";
        var lastName = "Doe";
        var phoneNumber = "+1234567890";

        // Act
        var user = User.Create(email, firstName, lastName, phoneNumber);

        // Assert
        user.Should().NotBeNull();
        user.Id.Should().NotBeEmpty();
        user.Email.Should().Be(email.ToUpperInvariant());
        user.FirstName.Should().Be(firstName);
        user.LastName.Should().Be(lastName);
        user.PhoneNumber.Should().Be(phoneNumber);
        user.EmailVerified.Should().BeFalse();
        user.Cart.Should().NotBeNull();
        user.Cart.OwnerId.Should().Be(user.Id);
        user.FullName.Should().Be($"{firstName} {lastName}");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithInvalidEmail_ShouldThrowArgumentException(string invalidEmail)
    {
        // Act
        Action act = () => User.Create(invalidEmail, "John", "Doe", "123456");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithInvalidFirstName_ShouldThrowArgumentException(string invalidFirstName)
    {
        // Act
        Action act = () => User.Create("test@example.com", invalidFirstName, "Doe", "123456");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithInvalidLastName_ShouldThrowArgumentException(string invalidLastName)
    {
        // Act
        Action act = () => User.Create("test@example.com", "John", invalidLastName, "123456");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddAddress_WithValidData_ShouldSucceed()
    {
        // Arrange
        var user = CreateValidUser();
        var line1 = "123 Main St";
        var city = "New York";
        var postalCode = "10001";
        var country = "USA";

        // Act
        var result = user.AddAddress(line1, null, city, postalCode, country);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.Addresses.Should().ContainSingle();
        user.PrimaryAddressId.Should().NotBeNull();
        user.PrimaryAddressId.Should().Be(user.Addresses.First().Id);
    }

    [Fact]
    public void AddAddress_MultipleAddresses_ShouldKeepFirstAsPrimary()
    {
        // Arrange
        var user = CreateValidUser();
        user.AddAddress("123 Main St", null, "New York", "10001", "USA");
        var firstAddressId = user.PrimaryAddressId;

        // Act
        user.AddAddress("456 Oak Ave", null, "Los Angeles", "90001", "USA");

        // Assert
        user.Addresses.Should().HaveCount(2);
        user.PrimaryAddressId.Should().Be(firstAddressId);
    }

    [Fact]
    public void VerifyEmail_WhenNotVerified_ShouldSucceed()
    {
        // Arrange
        var user = CreateValidUser();

        // Act
        var result = user.VerifyEmail();

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.EmailVerified.Should().BeTrue();
    }

    [Fact]
    public void VerifyEmail_WhenAlreadyVerified_ShouldReturnFailure()
    {
        // Arrange
        var user = CreateValidUser();
        user.VerifyEmail();

        // Act
        var result = user.VerifyEmail();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.EmailAlreadyVerified);
    }

    [Fact]
    public void Email_ShouldBeStoredInUppercase()
    {
        // Arrange
        var email = "test@example.com";

        // Act
        var user = User.Create(email, "John", "Doe", "123456");

        // Assert
        user.Email.Should().Be(email.ToUpperInvariant());
    }

    private static User CreateValidUser()
    {
        return User.Create(
            "test@example.com",
            "John",
            "Doe",
            "+1234567890");
    }
}
