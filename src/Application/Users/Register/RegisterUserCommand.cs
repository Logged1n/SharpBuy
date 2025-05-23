using Application.Abstractions.Messaging;
using SharedKernel.Dtos;

namespace Application.Users.Register;

public sealed record RegisterUserCommand(
    string Email,
    string FirstName,
    string LastName,
    string PhoneNumber,
    string Password,
    AddressDto? PrimaryAddress,
    ICollection<AddressDto>? AdditionalAddresses)
    : ICommand<Guid>;
