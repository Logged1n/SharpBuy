using System;
using System.Threading.Tasks;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Domain.Users;
using Infrastructure.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Web.Api.Extensions;

public static class IdentitySeedExtensions
{
    public static async Task SeedIdentityAsync(this IApplicationBuilder app)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();
        UserManager<ApplicationUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        async Task EnsureUser(Guid id, string email, string userName, string role, string password, string securityStamp)
        {
            ApplicationUser? user = await userManager.FindByEmailAsync(email);
            if (user is null)
            {
                var domainUser = DomainUser.Create(
                email: email,
                firstName: role,
                lastName: role,
                phoneNumber: "123456789");
                domainUser.VerifyEmail();

                user = new ApplicationUser
                {
                    Id = id,
                    UserName = userName,
                    NormalizedUserName = userName.ToUpperInvariant(),
                    Email = email,
                    NormalizedEmail = email.ToUpperInvariant(),
                    EmailConfirmed = true,
                    LockoutEnabled = true,
                    SecurityStamp = securityStamp,
                    DomainUser = domainUser,
                    DomainUserId = domainUser.Id
                };

                IdentityResult createResult = await userManager.CreateAsync(user, password);
                if (!createResult.Succeeded)
                {
                    throw new InvalidOperationException($"Failed to create user {email}: {string.Join(';', createResult)}");
                }

                IdentityResult addToRoleResult = await userManager.AddToRoleAsync(user, role);
                if (!addToRoleResult.Succeeded)
                {
                    throw new InvalidOperationException($"Failed to add user {email} to role {role}: {string.Join(';', addToRoleResult)}");
                }
            }
            else if (!await userManager.IsInRoleAsync(user, role))
            {
                await userManager.AddToRoleAsync(user, role);
            }
        }

        await EnsureUser(Guid.Parse("00000000-0000-0000-0000-000000000001"), "admin@admin.com", "admin@admin.com", "Admin", "Admin1!", "A1");
        await EnsureUser(Guid.Parse("00000000-0000-0000-0000-000000000002"), "salesman@salesman.com", "salesman@salesman.com", "Salesman", "Salesman1!", "A2");
        await EnsureUser(Guid.Parse("00000000-0000-0000-0000-000000000003"), "client@client.com", "client@client.com", "Client", "Client1!", "A3");
    }
}
