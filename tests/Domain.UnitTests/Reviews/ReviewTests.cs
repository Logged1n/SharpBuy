using Domain.Reviews;

namespace Domain.UnitTests.Reviews;

public class ReviewTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateReview()
    {
        // Arrange
        int score = 5;
        var productId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        string title = "Great product!";
        string description = "This product exceeded my expectations.";

        // Act
        var review = Review.Create(score, productId, userId, title, description);

        // Assert
        review.ShouldNotBeNull();
        review.Id.ShouldNotBe(Guid.Empty);
        review.Score.ShouldBe(score);
        review.ProductId.ShouldBe(productId);
        review.UserId.ShouldBe(userId);
        review.Title.ShouldBe(title);
        review.Description.ShouldBe(description);
        review.CreatedAt.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-5), DateTime.UtcNow.AddSeconds(5));
    }

    [Fact]
    public void Create_WithNullDescription_ShouldCreateReview()
    {
        // Arrange
        int score = 4;
        var productId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        string title = "Good product";

        // Act
        var review = Review.Create(score, productId, userId, title, null);

        // Assert
        review.ShouldNotBeNull();
        review.Description.ShouldBeNull();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithInvalidScore_ShouldThrowArgumentOutOfRangeException(int invalidScore)
    {
        // Arrange & Act
        Action act = () => Review.Create(
            invalidScore,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Title",
            "Description");

        // Assert
        Should.Throw<ArgumentOutOfRangeException>(act);
    }

    [Fact]
    public void Create_WithScoreGreaterThan5_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange & Act
        Action act = () => Review.Create(
            6,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Title",
            "Description");

        // Assert
        Should.Throw<ArgumentOutOfRangeException>(act);
    }

    [Fact]
    public void Create_WithEmptyProductId_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange & Act
        Action act = () => Review.Create(
            5,
            Guid.Empty,
            Guid.NewGuid(),
            "Title",
            "Description");

        // Assert
        Should.Throw<ArgumentOutOfRangeException>(act);
    }

    [Fact]
    public void Create_WithEmptyUserId_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange & Act
        Action act = () => Review.Create(
            5,
            Guid.NewGuid(),
            Guid.Empty,
            "Title",
            "Description");

        // Assert
        Should.Throw<ArgumentOutOfRangeException>(act);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithInvalidTitle_ShouldThrowArgumentException(string? invalidTitle)
    {
        // Arrange & Act
        Action act = () => Review.Create(
            5,
            Guid.NewGuid(),
            Guid.NewGuid(),
            invalidTitle!,
            "Description");

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void Create_WithValidScoreRange_ShouldCreateReview(int validScore)
    {
        // Arrange & Act
        var review = Review.Create(
            validScore,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Title",
            "Description");

        // Assert
        review.Score.ShouldBe(validScore);
    }
}
