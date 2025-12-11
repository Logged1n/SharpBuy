using Application.Reviews.Add;
using FluentValidation.TestHelper;

namespace Application.UnitTests.Reviews.Add;

public class AddReviewCommandValidatorTests
{
    private readonly AddReviewCommandValidator _validator;

    public AddReviewCommandValidatorTests()
    {
        _validator = new AddReviewCommandValidator();
    }

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new AddReviewCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            5,
            "Great product!",
            "This product exceeded my expectations.");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyUserId_ShouldHaveValidationError()
    {
        // Arrange
        var command = new AddReviewCommand(
            Guid.Empty,
            Guid.NewGuid(),
            5,
            "Great product!",
            "Description");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void Validate_WithEmptyProductId_ShouldHaveValidationError()
    {
        // Arrange
        var command = new AddReviewCommand(
            Guid.NewGuid(),
            Guid.Empty,
            5,
            "Great product!",
            "Description");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ProductId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(6)]
    [InlineData(10)]
    public void Validate_WithScoreOutOfRange_ShouldHaveValidationError(int invalidScore)
    {
        // Arrange
        var command = new AddReviewCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            invalidScore,
            "Great product!",
            "Description");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Score);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void Validate_WithValidScore_ShouldNotHaveValidationError(int validScore)
    {
        // Arrange
        var command = new AddReviewCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            validScore,
            "Great product!",
            "Description");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Score);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_WithInvalidTitle_ShouldHaveValidationError(string? invalidTitle)
    {
        // Arrange
        var command = new AddReviewCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            5,
            invalidTitle!,
            "Description");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Validate_WithTitleTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var command = new AddReviewCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            5,
            new string('a', 201), // 201 characters (max is 200)
            "Description");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Validate_WithTitleAtMaxLength_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new AddReviewCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            5,
            new string('a', 200), // Exactly 200 characters
            "Description");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Validate_WithNullDescription_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new AddReviewCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            5,
            "Great product!",
            null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Validate_WithDescriptionTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var command = new AddReviewCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            5,
            "Great product!",
            new string('a', 2001)); // 2001 characters (max is 2000)

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Validate_WithDescriptionAtMaxLength_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new AddReviewCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            5,
            "Great product!",
            new string('a', 2000)); // Exactly 2000 characters

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }
}
