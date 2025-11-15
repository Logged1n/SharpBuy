using System.ComponentModel.DataAnnotations.Schema;

namespace SharedKernel.ValueObjects;

[ComplexType]
public sealed record Money(
    decimal Amount,
    string Currency
    )
{
    public static Money Zero(string currency) => new (0m, currency);
    //TODO add currency checks
    public static Money operator+(Money left, Money right) => new Money(left.Amount+right.Amount, right.Currency);
    public static Money operator*(Money left, int right) => new (left.Amount*right, left.Currency);
}
