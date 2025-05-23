namespace Domain.Users;

public sealed class RoleClaim
{
    public Guid Id { get; set; }
    public Guid RoleId { get; set; }
    public Role Role { get; set; }
    public string ClaimType { get; set; }
    public string ClaimValue { get; set; }
}
