using Application.Abstractions.Messaging;
using Application.Users.RemoveRole;
using Domain.Users;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class RemoveRole : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("users/{userId:guid}/roles/{roleName}", async (
            Guid userId,
            string roleName,
            ICommandHandler<RemoveRoleFromUserCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new RemoveRoleFromUserCommand(userId, roleName);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(
                () => Results.NoContent(),
                CustomResults.Problem);
        })
        .WithTags(Tags.Users)
        .RequireRoles(Roles.Admin);
    }
}
