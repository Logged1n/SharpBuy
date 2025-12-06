using System.Collections.Generic;
using Application.Abstractions.Messaging;
using SharedKernel.Dtos;

namespace Application.Users.Register;

public sealed record RegisterUserCommand(
    string Email,
    string FirstName,
    string LastName,
    string Password,
    string PhoneNumber,
    AddressDto? PrimaryAddress = null,
    IReadOnlyCollection<AddressDto>? AdditionalAddresses = null,
    bool EmailConfirmed = false)
    : ICommand<Guid>;
