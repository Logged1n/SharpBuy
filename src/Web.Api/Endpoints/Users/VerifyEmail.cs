using Application.Users.VerifyEmail;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;
using Web.API.Infrastructure;

namespace Web.API.Endpoints.Users;

internal sealed class VerifyEmail : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/users/verify-email",
                async (
                    [FromQuery]Guid token,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var command = new VerifyEmailCommand(token);

                    Result result = await sender.Send(command, cancellationToken);

                    return result;
                })
            .WithTags(Tags.Users)
            .WithName("VerifyEmail");
    }
}
