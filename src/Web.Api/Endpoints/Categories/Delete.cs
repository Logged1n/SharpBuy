using Application.Abstractions.Messaging;
using Application.Categories.Delete;
using Domain.Users;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Categories;

public sealed class Delete : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("categories/{id:guid}", async (
            Guid id,
            ICommandHandler<DeleteCategoryCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new DeleteCategoryCommand(id);

            Result result = await handler.Handle(command, cancellationToken);
            return result.Match(
                () => Results.NoContent(),
                CustomResults.Problem);
        })
        .WithTags(Tags.Categories)
        .RequireRoles(Roles.Admin);
    }
}
