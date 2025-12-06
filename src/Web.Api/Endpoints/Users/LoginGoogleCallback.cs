using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Users.Login.Google;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class LoginGoogleCallback : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("users/login/callback", async (
            [FromQuery]string returnUrl,
            HttpContext context,
            ICommandHandler<LoginGoogleCommand, string> handler,
            CancellationToken cancellationToken) =>
        {
            AuthenticateResult result = await context.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
            if (!result.Succeeded)
                return Results.Unauthorized();

            Result<string> commandResult = await handler.Handle(new LoginGoogleCommand(result.Principal), cancellationToken);

            if (commandResult.IsFailure)
                return Results.Unauthorized();

            string redirectUrl = $"{returnUrl}?token={Uri.EscapeDataString(commandResult.Value)}";
            return Results.Redirect(redirectUrl);
        }).WithName("GoogleLoginCallback")
          .WithTags(Tags.Users)
          .AllowAnonymous();
    }
}
