using Application.Abstractions.Messaging;
using Application.Categories.Update;
using Domain.Users;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Categories;

public sealed class Update : IEndpoint
{
    public sealed record Request(string Name);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("categories/{id:guid}", async (
            Guid id,
            Request request,
            ICommandHandler<UpdateCategoryCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateCategoryCommand(id, request.Name);

            Result result = await handler.Handle(command, cancellationToken);
            return result.Match(
                () => Results.NoContent(),
                CustomResults.Problem);
        })
        .WithTags(Tags.Categories)
        .RequireRoles(Roles.Admin, Roles.Salesman);
    }
}
