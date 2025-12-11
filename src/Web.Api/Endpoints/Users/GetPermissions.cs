using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Users;

internal sealed class GetPermissions : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("users/permissions", async (
            HttpContext httpContext,
            UserManager<Application.Abstractions.Authentication.ApplicationUser> userManager,
            CancellationToken cancellationToken) =>
        {
            string? userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
            {
                return Results.Ok(Array.Empty<string>());
            }

            Application.Abstractions.Authentication.ApplicationUser? user = await userManager.FindByIdAsync(userId.ToString());

            if (user is null)
            {
                return Results.Ok(Array.Empty<string>());
            }

            IList<string> roles = await userManager.GetRolesAsync(user);

            // Map roles to permissions
            var permissions = new List<string>();

            if (roles.Contains("Admin") || roles.Contains("SalesManager"))
            {
                permissions.Add("products:write");
                permissions.Add("categories:write");
                permissions.Add("orders:write");
                permissions.Add("orders:read");
            }

            return Results.Ok(permissions);
        })
        .WithTags(Tags.Users)
        .RequireAuthorization();
    }
}
