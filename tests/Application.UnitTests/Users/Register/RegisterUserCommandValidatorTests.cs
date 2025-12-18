using Application.Users.Register;
using FluentValidation.TestHelper;

namespace Application.UnitTests.Users.Register;

public class RegisterUserCommandValidatorTests
{
    private readonly RegisterUserCommandValidator _validator;

    public RegisterUserCommandValidatorTests()
    {
        _validator = new RegisterUserCommandValidator();
    }

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new RegisterUserCommand(
            "test@example.com",
            "SecurePassword123!",
            "John",
            "Doe",
            "123456789"); // Exactly 9 characters

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_WithInvalidEmail_ShouldHaveValidationError(string? invalidEmail)
    {
        // Arrange
        var command = new RegisterUserCommand(
            Email: invalidEmail!,
            FirstName: "John",
            LastName: "Doe",
            Password: "SecurePassword123!",
            PhoneNumber: "123456789");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("@example.com")]
    public void Validate_WithInvalidEmailFormat_ShouldHaveValidationError(string invalidEmail)
    {
        // Arrange
        var command = new RegisterUserCommand(
            Email: invalidEmail,
            FirstName: "John",
            LastName: "Doe",
            Password: "SecurePassword123!",
            PhoneNumber: "123456789");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_WithInvalidFirstName_ShouldHaveValidationError(string? invalidFirstName)
    {
        // Arrange
        var command = new RegisterUserCommand(
            Email: "test@example.com",
            FirstName: invalidFirstName!,
            LastName: "Doe",
            Password: "SecurePassword123!",
            PhoneNumber: "123456789");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_WithInvalidLastName_ShouldHaveValidationError(string? invalidLastName)
    {
        // Arrange
        var command = new RegisterUserCommand(
            Email: "test@example.com",
            FirstName: "John",
            LastName: invalidLastName!,
            Password: "SecurePassword123!",
            PhoneNumber: "123456789");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LastName);
    }

    [Theory]
    [InlineData("12345678")] // 8 characters
    [InlineData("1234567890")] // 10 characters
    [InlineData("123")] // 3 characters
    public void Validate_WithInvalidPhoneNumberLength_ShouldHaveValidationError(string phoneNumber)
    {
        // Arrange
        var command = new RegisterUserCommand(
            Email: "test@example.com",
            FirstName: "John",
            LastName: "Doe",
            Password: "SecurePassword123!",
            PhoneNumber: phoneNumber);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber);
    }

    [Fact]
    public void Validate_WithEmptyPhoneNumber_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new RegisterUserCommand(
            Email: "test@example.com",
            FirstName: "John",
            LastName: "Doe",
            Password: "SecurePassword123!",
            PhoneNumber: "");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PhoneNumber);
    }

    [Fact]
    public void Validate_WithValidPhoneNumber_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new RegisterUserCommand(
            Email: "test@example.com",
            FirstName: "John",
            LastName: "Doe",
            Password: "SecurePassword123!",
            PhoneNumber: "987654321"); // Exactly 9 characters

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PhoneNumber);
    }
}
