using Application.Abstractions.Messaging;
using Application.Categories.Add;
using Domain.Users;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Categories;

public sealed class Add : IEndpoint
{
    public sealed record Request(string Name);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("categories", async (
            Request request,
            ICommandHandler<AddCategoryCommand, Guid> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new AddCategoryCommand(request.Name);

            Result<Guid> result = await handler.Handle(command, cancellationToken);
            return result.Match(
                id => Results.Created($"/categories/{id}", id),
                CustomResults.Problem);
        })
        .WithTags(Tags.Categories)
        .RequireRoles(Roles.Admin, Roles.Salesman);
    }
}
