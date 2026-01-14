using Application.Abstractions.Messaging;
using SharedKernel;

namespace Application.Users.GetAll;

public sealed record GetAllUsersQuery(int Page = 1, int PageSize = 20) : IQuery<PagedResult<UserListItem>>;

public sealed record UserListItem(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    bool EmailConfirmed,
    List<string> Roles);
