using Application.Abstractions.Messaging;
using Application.Products.Add;
using Domain.Users;
using SharedKernel;
using SharedKernel.ValueObjects;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Products;

public sealed class Add : IEndpoint
{
    public sealed record Request(string Name, string Description, int Quantity, Money Price, ICollection<Guid> CategoryIds, string MainPhotoPath);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("products", async (
            Request request,
            ICommandHandler<AddProductCommand, Guid> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new AddProductCommand(
                request.Name,
                request.Description,
                request.Quantity,
                request.Price,
                request.CategoryIds,
                request.MainPhotoPath);

            Result<Guid> result = await handler.Handle(command, cancellationToken);
            return result.Match(
                id => Results.Created($"/products/{id}", id),
                CustomResults.Problem);
        })
        .WithTags(Tags.Products)
        .RequireRoles(Roles.Admin, Roles.Salesman);
    }
}
