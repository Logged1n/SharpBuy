﻿namespace Domain.Users;

public sealed class UserClaim
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; }
    public string ClaimType { get; set; }
    public string ClaimValue { get; set; }
}
