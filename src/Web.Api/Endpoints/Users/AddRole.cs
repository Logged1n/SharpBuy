using Application.Abstractions.Messaging;
using Application.Users.AddRole;
using Domain.Users;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class AddRole : IEndpoint
{
    public sealed record Request(string RoleName);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("users/{userId:guid}/roles", async (
            Guid userId,
            Request request,
            ICommandHandler<AddRoleToUserCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new AddRoleToUserCommand(userId, request.RoleName);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(
                () => Results.NoContent(),
                CustomResults.Problem);
        })
        .WithTags(Tags.Users)
        .RequireRoles(Roles.Admin);
    }
}
