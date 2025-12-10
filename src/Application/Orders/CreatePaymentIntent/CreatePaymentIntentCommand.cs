using Application.Abstractions.Messaging;

namespace Application.Orders.CreatePaymentIntent;

public sealed record CreatePaymentIntentCommand(Guid UserId) : ICommand<string>;
