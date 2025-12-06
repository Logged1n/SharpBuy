using Application.Abstractions.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Web.Api.Endpoints.Users;

internal sealed class LoginGoogle : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("users/login/google", (
            [FromQuery] string returnUrl,
            LinkGenerator linkGenerator,
            SignInManager<ApplicationUser> signInManager,
            HttpContext context) =>
        {
            AuthenticationProperties properties = signInManager.ConfigureExternalAuthenticationProperties(
                "Google",
                linkGenerator.GetPathByName(context, "GoogleLoginCallback")
                + $"?returnUrl={returnUrl}");

            return Results.Challenge(properties, ["Google"]);
        })
            .WithTags(Tags.Users)
            .AllowAnonymous();
    }
}
