using Application.Users.VerifyEmail;
using MediatR;
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
                    Guid id,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var command = new VerifyEmailCommand(id);

                    Result result = await sender.Send(command, cancellationToken);

                    return result;
                })
            .WithTags(Tags.Users)
            .WithName("VerifyEmail");
    }
}
