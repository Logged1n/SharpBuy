using Application.Abstractions.Messaging;
using Application.Users.VerifyEmail;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;

namespace Web.Api.Endpoints.Users;

internal sealed class VerifyEmail : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/users/verify-email",
                async (
                    [FromQuery]Guid token,
                    ICommandHandler<VerifyEmailCommand> commandHandler,
                    CancellationToken cancellationToken) =>
                {
                    var command = new VerifyEmailCommand(token);

                    Result result = await commandHandler.Handle(command, cancellationToken);

                    return result;
                })
            .WithTags(Tags.Users)
            .WithName("VerifyEmail");
    }
}
