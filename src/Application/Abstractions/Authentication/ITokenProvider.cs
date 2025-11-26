using Domain.Users;

namespace Application.Abstractions.Authentication;

public interface ITokenProvider
{
    Task<string> Create(ApplicationUser user);
}
