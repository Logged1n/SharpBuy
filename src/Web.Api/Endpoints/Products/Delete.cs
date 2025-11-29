using Application.Abstractions.Messaging;
using Application.Products.Delete;
using Domain.Users;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Products;

public sealed class Delete : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("products/{id:guid}", async (
            Guid id,
            ICommandHandler<DeleteProductCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new DeleteProductCommand(id);

            Result result = await handler.Handle(command, cancellationToken);
            return result.Match(
                () => Results.NoContent(),
                CustomResults.Problem);
        })
        .WithTags(Tags.Products)
        .RequireRoles(Roles.Admin);
    }
}
