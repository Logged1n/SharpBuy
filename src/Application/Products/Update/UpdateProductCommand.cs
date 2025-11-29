using Application.Abstractions.Messaging;

namespace Application.Products.Update;

public sealed record UpdateProductCommand(
    Guid Id,
    string Name,
    string Description,
    decimal PriceAmount,
    string PriceCurrency) : ICommand;
