using Application.Abstractions.Messaging;
using Application.Products.Add;
using SharedKernel;
using SharedKernel.ValueObjects;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Products;

public sealed class Add : IEndpoint
{
    public sealed record Request(string Name, string Description, Money price, ICollection<Guid> categoryIds);

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
                request.price,
                request.categoryIds);

            Result<Guid> result = await handler.Handle(command, cancellationToken);
            return result.Match(Results.Created, CustomResults.Problem);
        })
        .WithTags(Tags.Products);
    }
}
