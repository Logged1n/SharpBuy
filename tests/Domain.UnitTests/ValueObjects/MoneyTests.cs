using SharedKernel.ValueObjects;
using Shouldly;

namespace Domain.UnitTests.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void Constructor_WithValidValues_ShouldCreateMoney()
    {
        // Arrange
        decimal amount = 100.50m;
        string currency = "USD";

        // Act
        var money = new Money(amount, currency);

        // Assert
        money.Amount.ShouldBe(amount);
        money.Currency.ShouldBe(currency);
    }

    [Fact]
    public void Zero_WithValidCurrency_ShouldCreateZeroMoney()
    {
        // Arrange
        string currency = "EUR";

        // Act
        var money = Money.Zero(currency);

        // Assert
        money.Amount.ShouldBe(0m);
        money.Currency.ShouldBe(currency);
    }

    [Theory]
    [InlineData(10.00, "USD", 20.00, "USD", 30.00)]
    [InlineData(15.50, "EUR", 4.50, "EUR", 20.00)]
    [InlineData(0.00, "GBP", 10.00, "GBP", 10.00)]
    [InlineData(100.99, "PLN", 0.01, "PLN", 101.00)]
    public void AddOperator_WithSameCurrency_ShouldAddAmounts(
        decimal amount1, string currency1,
        decimal amount2, string currency2,
        decimal expectedAmount)
    {
        // Arrange
        var money1 = new Money(amount1, currency1);
        var money2 = new Money(amount2, currency2);

        // Act
        Money result = money1 + money2;

        // Assert
        result.Amount.ShouldBe(expectedAmount);
        result.Currency.ShouldBe(currency1);
    }

    [Fact]
    public void AddOperator_WithDifferentCurrencies_ShouldUseCurrencyFromRight()
    {
        // Arrange
        var money1 = new Money(10.00m, "USD");
        var money2 = new Money(20.00m, "EUR");

        // Act
        Money result = money1 + money2;

        // Assert
        result.Amount.ShouldBe(30.00m);
        result.Currency.ShouldBe("EUR"); // Uses right operand's currency
    }

    [Theory]
    [InlineData(10.00, "USD", 2, 20.00)]
    [InlineData(5.50, "EUR", 3, 16.50)]
    [InlineData(100.00, "GBP", 0, 0.00)]
    [InlineData(7.25, "PLN", 5, 36.25)]
    public void MultiplyOperator_WithInteger_ShouldMultiplyAmount(
        decimal amount, string currency,
        int multiplier,
        decimal expectedAmount)
    {
        // Arrange
        var money = new Money(amount, currency);

        // Act
        Money result = money * multiplier;

        // Assert
        result.Amount.ShouldBe(expectedAmount);
        result.Currency.ShouldBe(currency);
    }

    [Fact]
    public void MultiplyOperator_WithNegativeMultiplier_ShouldReturnNegativeAmount()
    {
        // Arrange
        var money = new Money(10.00m, "USD");

        // Act
        Money result = money * -2;

        // Assert
        result.Amount.ShouldBe(-20.00m);
        result.Currency.ShouldBe("USD");
    }

    [Fact]
    public void Equality_WithSameValues_ShouldBeEqual()
    {
        // Arrange
        var money1 = new Money(50.00m, "USD");
        var money2 = new Money(50.00m, "USD");

        // Act & Assert
        money1.ShouldBe(money2);
        (money1 == money2).ShouldBeTrue();
    }

    [Fact]
    public void Equality_WithDifferentAmounts_ShouldNotBeEqual()
    {
        // Arrange
        var money1 = new Money(50.00m, "USD");
        var money2 = new Money(60.00m, "USD");

        // Act & Assert
        money1.ShouldNotBe(money2);
        (money1 != money2).ShouldBeTrue();
    }

    [Fact]
    public void Equality_WithDifferentCurrencies_ShouldNotBeEqual()
    {
        // Arrange
        var money1 = new Money(50.00m, "USD");
        var money2 = new Money(50.00m, "EUR");

        // Act & Assert
        money1.ShouldNotBe(money2);
        (money1 != money2).ShouldBeTrue();
    }

    [Fact]
    public void GetHashCode_WithSameValues_ShouldBeSame()
    {
        // Arrange
        var money1 = new Money(50.00m, "USD");
        var money2 = new Money(50.00m, "USD");

        // Act & Assert
        money1.GetHashCode().ShouldBe(money2.GetHashCode());
    }

    [Fact]
    public void ToString_ShouldContainAmountAndCurrency()
    {
        // Arrange
        var money = new Money(123.45m, "EUR");

        // Act
        string result = money.ToString();

        // Assert
        result.ShouldContain("123.45");
        result.ShouldContain("EUR");
    }

    [Fact]
    public void Deconstruct_ShouldReturnAmountAndCurrency()
    {
        // Arrange
        var money = new Money(99.99m, "GBP");

        // Act
        (decimal amount, string currency) = money;

        // Assert
        amount.ShouldBe(99.99m);
        currency.ShouldBe("GBP");
    }
}
