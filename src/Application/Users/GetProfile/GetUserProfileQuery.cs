using Application.Abstractions.Messaging;
using SharedKernel.Dtos;

namespace Application.Users.GetProfile;

public sealed record GetUserProfileQuery(Guid UserId) : IQuery<UserProfileResponse>;

public sealed record UserProfileResponse(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    Guid? AddressId,
    AddressDto? Address);
