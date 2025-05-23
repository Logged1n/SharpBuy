namespace Domain.Users;

public sealed class Role
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public ICollection<RoleClaim> Claims { get; set; }
    public ICollection<User> Users { get; set; }
}
