using Domain.Users;
using Microsoft.AspNetCore.Identity;

namespace Application.Abstractions.Authentication;
public sealed class ApplicationUser : IdentityUser<Guid>
{
    public Guid DomainUserId { get; set; }
    public DomainUser DomainUser { get; set; }
}
