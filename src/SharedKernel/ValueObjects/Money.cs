using System.ComponentModel.DataAnnotations.Schema;

namespace SharedKernel.ValueObjects;

[ComplexType]
public sealed record Money(
    decimal Amount,
    string Currency
    );
