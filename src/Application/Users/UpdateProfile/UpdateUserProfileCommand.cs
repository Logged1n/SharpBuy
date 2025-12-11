using Application.Abstractions.Messaging;
using SharedKernel.Dtos;

namespace Application.Users.UpdateProfile;

public sealed record UpdateUserProfileCommand(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    AddressDto? Address) : ICommand;
