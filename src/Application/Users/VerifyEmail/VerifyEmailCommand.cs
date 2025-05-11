using Application.Abstractions.Messaging;
using SharedKernel;

namespace Application.Users.VerifyEmail;
public sealed record VerifyEmailCommand(Guid TokenId) : ICommand;
