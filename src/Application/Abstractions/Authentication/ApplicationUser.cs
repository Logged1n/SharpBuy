using Domain.Users;
using Microsoft.AspNetCore.Identity;

namespace Application.Abstractions.Authentication;
public sealed class ApplicationUser : IdentityUser<Guid>
{
    public Guid DomainUserId { get; set; }
    public User DomainUser { get; set; }
}
